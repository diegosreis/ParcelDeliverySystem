using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Represents a business rule that defines parcel routing logic
/// </summary>
/// <param name="Id">Unique identifier for the business rule</param>
/// <param name="Name">Name of the business rule</param>
/// <param name="Description">Detailed description of what the rule does</param>
/// <param name="Type">Type of rule (Weight-based or Value-based)</param>
/// <param name="MinValue">Minimum threshold value for the rule</param>
/// <param name="MaxValue">Maximum threshold value for the rule (optional)</param>
/// <param name="TargetDepartment">Department that should handle parcels matching this rule</param>
/// <param name="IsActive">Indicates if the rule is currently active</param>
/// <param name="CreatedAt">Timestamp when the rule was created</param>
/// <param name="UpdatedAt">Timestamp when the rule was last updated</param>
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