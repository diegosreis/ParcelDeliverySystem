using Application.DTOs;

namespace Application.Services;

public interface IDepartmentRuleService
{
    Task<IEnumerable<DepartmentDto>> DetermineRequiredDepartmentsAsync(Guid parcelId);
    Task<bool> RequiresInsuranceApprovalAsync(Guid parcelId);
    Task<IEnumerable<DepartmentDto>> GetDepartmentsByWeightAsync(decimal weight);
    Task<IEnumerable<DepartmentDto>> GetDepartmentsByValueAsync(decimal value);
}