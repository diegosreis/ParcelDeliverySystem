using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class BusinessRuleService(IBusinessRuleRepository businessRuleRepository) : IBusinessRuleService
{
    private readonly IBusinessRuleRepository _businessRuleRepository =
        businessRuleRepository ?? throw new ArgumentNullException(nameof(businessRuleRepository));

    public async Task<IEnumerable<BusinessRuleDto>> GetAllRulesAsync()
    {
        var rules = await _businessRuleRepository.GetAllAsync();
        return rules.Select(MapToDto);
    }

    public async Task<IEnumerable<BusinessRuleDto>> GetActiveRulesAsync()
    {
        var rules = await _businessRuleRepository.GetAllActiveRulesAsync();
        return rules.Select(MapToDto);
    }

    public async Task<IEnumerable<BusinessRuleDto>> GetRulesByTypeAsync(BusinessRuleType type)
    {
        var rules = await _businessRuleRepository.GetActiveRulesByTypeAsync(type);
        return rules.Select(MapToDto);
    }

    public async Task<BusinessRuleDto?> GetRuleByIdAsync(Guid id)
    {
        var rule = await _businessRuleRepository.GetByIdAsync(id);
        return rule != null ? MapToDto(rule) : null;
    }

    public async Task<BusinessRuleDto> CreateRuleAsync(string name, string description, BusinessRuleType type,
        decimal minValue, decimal? maxValue, string targetDepartment)
    {
        var rule = new BusinessRule(name, description, type, minValue, maxValue, targetDepartment);
        var createdRule = await _businessRuleRepository.AddAsync(rule);
        return MapToDto(createdRule);
    }

    public async Task<BusinessRuleDto> UpdateRuleAsync(Guid id, string name, string description,
        decimal minValue, decimal? maxValue, string targetDepartment)
    {
        var rule = await _businessRuleRepository.GetByIdAsync(id);
        if (rule == null)
            throw new ArgumentException($"Business rule with ID {id} not found");

        rule.Update(name, description, minValue, maxValue, targetDepartment);
        var updatedRule = await _businessRuleRepository.UpdateAsync(rule);
        return MapToDto(updatedRule);
    }

    public async Task ActivateRuleAsync(Guid id)
    {
        var rule = await _businessRuleRepository.GetByIdAsync(id);
        if (rule == null)
            throw new ArgumentException($"Business rule with ID {id} not found");

        rule.Activate();
        await _businessRuleRepository.UpdateAsync(rule);
    }

    public async Task DeactivateRuleAsync(Guid id)
    {
        var rule = await _businessRuleRepository.GetByIdAsync(id);
        if (rule == null)
            throw new ArgumentException($"Business rule with ID {id} not found");

        rule.Deactivate();
        await _businessRuleRepository.UpdateAsync(rule);
    }

    public async Task DeleteRuleAsync(Guid id)
    {
        var rule = await _businessRuleRepository.GetByIdAsync(id);
        if (rule == null)
            throw new ArgumentException($"Business rule with ID {id} not found");

        await _businessRuleRepository.DeleteAsync(id);
    }

    private static BusinessRuleDto MapToDto(BusinessRule rule)
    {
        return new BusinessRuleDto(
            rule.Id,
            rule.Name,
            rule.Description,
            rule.Type,
            rule.MinValue,
            rule.MaxValue,
            rule.TargetDepartment,
            rule.IsActive,
            rule.CreatedAt,
            rule.UpdatedAt
        );
    }
}