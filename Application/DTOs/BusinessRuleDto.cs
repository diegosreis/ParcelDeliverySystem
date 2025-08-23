using Domain.Enums;

namespace Application.DTOs;

public record BusinessRuleDto(
    Guid Id,
    string Name,
    string Description,
    BusinessRuleType Type,
    decimal MinValue,
    decimal? MaxValue,
    string TargetDepartment,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);