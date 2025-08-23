using Application.DTOs;
using Domain.Constants;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class DepartmentRuleService(
    IParcelRepository parcelRepository,
    IDepartmentRepository departmentRepository)
    : IDepartmentRuleService
{
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

        // Check if insurance approval is required
        if (parcel.RequiresInsuranceApproval)
        {
            var insuranceDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Insurance);
            if (insuranceDept != null) departments.Add(MapToDepartmentDto(insuranceDept));
        }

        // Determine department based on weight
        var weightDepartments = await GetDepartmentsByWeightAsync(parcel.Weight);
        departments.AddRange(weightDepartments);

        return departments.DistinctBy(d => d.Id);
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByWeightAsync(decimal weight)
    {
        var departments = new List<DepartmentDto>();

        if (weight <= 1m)
        {
            var mailDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Mail);
            if (mailDept != null)
                departments.Add(MapToDepartmentDto(mailDept));
        }
        else if (weight <= 10m)
        {
            var regularDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Regular);
            if (regularDept != null)
                departments.Add(MapToDepartmentDto(regularDept));
        }
        else
        {
            var heavyDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Heavy);
            if (heavyDept != null)
                departments.Add(MapToDepartmentDto(heavyDept));
        }

        return departments;
    }

    public async Task<IEnumerable<DepartmentDto>> GetDepartmentsByValueAsync(decimal value)
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