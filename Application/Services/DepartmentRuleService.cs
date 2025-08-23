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

        var departments = new List<DepartmentDto>();

        // Apply weight-based rules
        var weightDepartments = await GetDepartmentsByWeightAsync(parcel.Weight);
        departments.AddRange(weightDepartments);

        // Apply value-based rules
        var valueDepartments = await GetDepartmentsByValueAsync(parcel.Value);
        departments.AddRange(valueDepartments);

        return departments.DistinctBy(d => d.Id);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByWeightAsync(decimal weight)
    {
        var departments = new List<DepartmentDto>();
        var weightRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(BusinessRuleType.Weight);

        foreach (var rule in weightRules.OrderBy(r => r.MinValue))
            if (weight >= rule.MinValue && (rule.MaxValue == null || weight <= rule.MaxValue))
            {
                var department = await _departmentRepository.GetByNameAsync(rule.TargetDepartment);
                if (department == null) continue;
                departments.Add(MapToDepartmentDto(department));
                break; // Take the first matching rule
            }

        // Fallback to default rules if no business rules configured
        if (departments.Count == 0) departments.AddRange(await GetDefaultDepartmentsByWeightAsync(weight));

        return departments;
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByValueAsync(decimal value)
    {
        var departments = new List<DepartmentDto>();
        var valueRules = await _businessRuleRepository.GetActiveRulesByTypeAsync(BusinessRuleType.Value);

        foreach (var rule in valueRules.OrderBy(r => r.MinValue))
            if (value >= rule.MinValue &&
                (rule.MaxValue == null || value <= rule.MaxValue))
            {
                var department = await _departmentRepository.GetByNameAsync(rule.TargetDepartment);
                if (department != null) departments.Add(MapToDepartmentDto(department));
            }

        // Fallback to default rules if no business rules configured
        if (departments.Count == 0) departments.AddRange(await GetDefaultDepartmentsByValueAsync(value));

        return departments;
    }

    // Default business rules when no custom rules are configured
    private async Task<IEnumerable<DepartmentDto>> GetDefaultDepartmentsByWeightAsync(decimal weight)
    {
        var departments = new List<DepartmentDto>();

        switch (weight)
        {
            case <= 1m:
            {
                var mailDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Mail);
                if (mailDept != null)
                    departments.Add(MapToDepartmentDto(mailDept));
                break;
            }
            case <= 10m:
            {
                var regularDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Regular);
                if (regularDept != null)
                    departments.Add(MapToDepartmentDto(regularDept));
                break;
            }
            default:
            {
                var heavyDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Heavy);
                if (heavyDept != null)
                    departments.Add(MapToDepartmentDto(heavyDept));
                break;
            }
        }

        return departments;
    }

    private async Task<IEnumerable<DepartmentDto>> GetDefaultDepartmentsByValueAsync(decimal value)
    {
        var departments = new List<DepartmentDto>();

        if (value <= 1000m) return departments;
        var insuranceDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Insurance);
        if (insuranceDept != null)
            departments.Add(MapToDepartmentDto(insuranceDept));

        return departments;
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