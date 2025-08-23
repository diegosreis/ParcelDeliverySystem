using Application.DTOs;
using Domain.Enums;

namespace Application.Services;

public interface IBusinessRuleService
{
    Task<IEnumerable<BusinessRuleDto>> GetAllRulesAsync();
    Task<IEnumerable<BusinessRuleDto>> GetActiveRulesAsync();
    Task<IEnumerable<BusinessRuleDto>> GetRulesByTypeAsync(BusinessRuleType type);
    Task<BusinessRuleDto?> GetRuleByIdAsync(Guid id);

    Task<BusinessRuleDto> CreateRuleAsync(string name, string description, BusinessRuleType type,
        decimal minValue, decimal? maxValue, string targetDepartment);

    Task<BusinessRuleDto> UpdateRuleAsync(Guid id, string name, string description,
        decimal minValue, decimal? maxValue, string targetDepartment);

    Task ActivateRuleAsync(Guid id);
    Task DeactivateRuleAsync(Guid id);
    Task DeleteRuleAsync(Guid id);
}