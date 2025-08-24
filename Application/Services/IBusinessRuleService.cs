using Application.DTOs;
using Domain.Enums;

namespace Application.Services;

/// <summary>
///     Service interface for managing business rules in the parcel delivery system.
///     Provides operations for configurable business logic that determines parcel routing and processing.
/// </summary>
public interface IBusinessRuleService
{
    /// <summary>
    ///     Retrieves all business rules in the system
    /// </summary>
    /// <returns>A collection of all business rules</returns>
    Task<IEnumerable<BusinessRuleDto>> GetAllRulesAsync();

    /// <summary>
    ///     Retrieves all active business rules
    /// </summary>
    /// <returns>A collection of active business rules</returns>
    Task<IEnumerable<BusinessRuleDto>> GetActiveRulesAsync();

    /// <summary>
    ///     Retrieves business rules filtered by type
    /// </summary>
    /// <param name="type">The type of business rule to filter by</param>
    /// <returns>A collection of business rules of the specified type</returns>
    Task<IEnumerable<BusinessRuleDto>> GetRulesByTypeAsync(BusinessRuleType type);

    /// <summary>
    ///     Retrieves a specific business rule by ID
    /// </summary>
    /// <param name="id">The unique identifier of the business rule</param>
    /// <returns>The business rule with the specified ID, or null if not found</returns>
    Task<BusinessRuleDto?> GetRuleByIdAsync(Guid id);

    /// <summary>
    ///     Creates a new business rule in the system
    /// </summary>
    /// <param name="name">The name of the business rule</param>
    /// <param name="description">A description of what the rule does</param>
    /// <param name="type">The type of business rule (Weight, Value, etc.)</param>
    /// <param name="minValue">The minimum value threshold for the rule</param>
    /// <param name="maxValue">The maximum value threshold for the rule (optional)</param>
    /// <param name="targetDepartment">The department name that parcels matching this rule should be assigned to</param>
    /// <returns>The created business rule</returns>
    Task<BusinessRuleDto> CreateRuleAsync(string name, string description, BusinessRuleType type,
        decimal minValue, decimal? maxValue, string targetDepartment);

    /// <summary>
    ///     Updates an existing business rule
    /// </summary>
    /// <param name="id">The unique identifier of the business rule to update</param>
    /// <param name="name">The updated name of the business rule</param>
    /// <param name="description">The updated description</param>
    /// <param name="minValue">The updated minimum value threshold</param>
    /// <param name="maxValue">The updated maximum value threshold (optional)</param>
    /// <param name="targetDepartment">The updated target department name</param>
    /// <returns>The updated business rule</returns>
    Task<BusinessRuleDto> UpdateRuleAsync(Guid id, string name, string description,
        decimal minValue, decimal? maxValue, string targetDepartment);

    /// <summary>
    ///     Activates a business rule, making it available for parcel processing
    /// </summary>
    /// <param name="id">The unique identifier of the business rule to activate</param>
    Task ActivateRuleAsync(Guid id);

    /// <summary>
    ///     Deactivates a business rule, removing it from parcel processing without deleting
    /// </summary>
    /// <param name="id">The unique identifier of the business rule to deactivate</param>
    Task DeactivateRuleAsync(Guid id);

    /// <summary>
    ///     Deletes a business rule from the system
    /// </summary>
    /// <param name="id">The unique identifier of the business rule to delete</param>
    Task DeleteRuleAsync(Guid id);
}