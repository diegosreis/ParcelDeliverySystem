using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

/// <summary>
///     Repository interface for managing business rules in the parcel delivery system
/// </summary>
public interface IBusinessRuleRepository : IRepository<BusinessRule>
{
    /// <summary>
    ///     Retrieves all active business rules of a specific type
    /// </summary>
    /// <param name="type">The type of business rules to retrieve</param>
    /// <returns>A collection of active business rules matching the specified type</returns>
    Task<IEnumerable<BusinessRule>> GetActiveRulesByTypeAsync(BusinessRuleType type);

    /// <summary>
    ///     Retrieves all active business rules regardless of type
    /// </summary>
    /// <returns>A collection of all active business rules</returns>
    Task<IEnumerable<BusinessRule>> GetAllActiveRulesAsync();

    /// <summary>
    ///     Retrieves a business rule by its name
    /// </summary>
    /// <param name="name">The name of the business rule to find</param>
    /// <returns>The business rule with the specified name, or null if not found</returns>
    Task<BusinessRule?> GetByNameAsync(string name);
}