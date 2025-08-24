using Application.DTOs;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using static Domain.Constants.DefaultBusinessRuleValues;

namespace Application.Services;

/// <summary>
///     Service implementation for determining department assignments based on parcel characteristics.
///     Encapsulates business logic for both configurable rules and default department routing.
/// </summary>
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

    /// <inheritdoc />
    public async Task<bool> RequiresInsuranceApprovalAsync(Guid parcelId)
    {
        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        return parcel?.RequiresInsuranceApproval ?? throw new ArgumentException($"Parcel with ID {parcelId} not found");
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<List<Department>> DetermineDepartmentsAsync(Parcel parcel)
    {
        ArgumentNullException.ThrowIfNull(parcel);

        var departments = new List<Department>();

        // Apply weight-based rules
        var weightDepartmentDtos = await GetDepartmentsByWeightAsync(parcel.Weight);
        foreach (var deptDto in weightDepartmentDtos)
        {
            var department = await _departmentRepository.GetByIdAsync(deptDto.Id);
            if (department != null)
                departments.Add(department);
        }

        // Apply value-based rules
        var valueDepartmentDtos = await GetDepartmentsByValueAsync(parcel.Value);
        foreach (var deptDto in valueDepartmentDtos)
        {
            var department = await _departmentRepository.GetByIdAsync(deptDto.Id);
            if (department != null && departments.All(d => d.Id != department.Id))
                departments.Add(department);
        }

        return departments;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<IEnumerable<DepartmentDto>> GetDefaultDepartmentsByValueAsync(decimal value)
    {
        if (value <= InsuranceValueThreshold)
            return [];

        return await GetSingleDepartmentAsync(DefaultDepartmentNames.Insurance);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DepartmentDto>> GetDefaultDepartmentsByWeightAsync(decimal weight)
    {
        return weight switch
        {
            <= MailWeightThreshold => await GetSingleDepartmentAsync(DefaultDepartmentNames.Mail),
            <= RegularWeightThreshold => await GetSingleDepartmentAsync(DefaultDepartmentNames.Regular),
            _ => await GetSingleDepartmentAsync(DefaultDepartmentNames.Heavy)
        };
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