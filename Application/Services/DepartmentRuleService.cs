using Application.DTOs;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class DepartmentRuleService(
    IParcelRepository parcelRepository,
    IDepartmentRepository departmentRepository,
    IBusinessRuleRepository businessRuleRepository)
    : IDepartmentRuleService
{
    private readonly IBusinessRuleRepository _businessRuleRepository =
        businessRuleRepository ?? throw new ArgumentNullException(nameof(businessRuleRepository));

    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    private readonly IParcelRepository _parcelRepository =
        parcelRepository ?? throw new ArgumentNullException(nameof(parcelRepository));

    public async Task<bool> RequiresInsuranceApprovalAsync(Guid parcelId)
    {
        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        return parcel?.RequiresInsuranceApproval ?? throw new ArgumentException($"Parcel with ID {parcelId} not found");
    }

    public async Task<IEnumerable<DepartmentDto>> DetermineRequiredDepartmentsAsync(Guid parcelId)
    {
        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        if (parcel == null)
            throw new ArgumentException($"Parcel with ID {parcelId} not found");

        // Apply weight-based rules
        var weightDepartments = await GetDepartmentsByWeightAsync(parcel.Weight);

        // Apply value-based rules
        var valueDepartments = await GetDepartmentsByValueAsync(parcel.Value);

        // Combine and remove duplicates using LINQ
        return weightDepartments.Concat(valueDepartments).DistinctBy(d => d.Id);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByWeightAsync(decimal weight)
    {
        var weightRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(BusinessRuleType.Weight);

        foreach (var rule in weightRules.OrderBy(r => r.MinValue))
            if (weight >= rule.MinValue && (rule.MaxValue == null || weight <= rule.MaxValue))
            {
                var department = await _departmentRepository.GetByNameAsync(rule.TargetDepartment);
                if (department == null) continue;
                return [MapToDepartmentDto(department)]; // Return single item as array
            }

        // Fallback to default rules if no business rules configured
        return await GetDefaultDepartmentsByWeightAsync(weight);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByValueAsync(decimal value)
    {
        var valueRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(BusinessRuleType.Value);
        var matchingDepartments = new List<DepartmentDto>();

        foreach (var rule in valueRules.OrderBy(r => r.MinValue))
            if (value >= rule.MinValue && (rule.MaxValue == null || value <= rule.MaxValue))
            {
                var department = await _departmentRepository.GetByNameAsync(rule.TargetDepartment);
                if (department != null)
                    matchingDepartments.Add(MapToDepartmentDto(department));
            }

        // Fallback to default rules if no business rules configured
        return matchingDepartments.Count > 0 ? matchingDepartments : await GetDefaultDepartmentsByValueAsync(value);
    }

    // Default business rules when no custom rules are configured
    private async Task<IEnumerable<DepartmentDto>> GetDefaultDepartmentsByWeightAsync(decimal weight)
    {
        return weight switch
        {
            <= 1m => await GetSingleDepartmentAsync(DefaultDepartmentNames.Mail),
            <= 10m => await GetSingleDepartmentAsync(DefaultDepartmentNames.Regular),
            _ => await GetSingleDepartmentAsync(DefaultDepartmentNames.Heavy)
        };
    }

    private async Task<IEnumerable<DepartmentDto>> GetDefaultDepartmentsByValueAsync(decimal value)
    {
        if (value <= 1000m)
            return [];

        return await GetSingleDepartmentAsync(DefaultDepartmentNames.Insurance);
    }

    private async Task<IEnumerable<DepartmentDto>> GetSingleDepartmentAsync(string departmentName)
    {
        var department = await _departmentRepository.GetByNameAsync(departmentName);
        return department != null ? [MapToDepartmentDto(department)] : [];
    }

    private static DepartmentDto MapToDepartmentDto(Department department)
    {
        return new DepartmentDto(
            department.Id,
            department.Name,
            department.Description,
            department.IsActive,
            department.CreatedAt,
            department.UpdatedAt
        );
    }
}