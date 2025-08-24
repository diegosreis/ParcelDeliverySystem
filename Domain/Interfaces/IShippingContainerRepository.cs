using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

/// <summary>
/// Repository interface for managing shipping container entities with specialized queries
/// </summary>
public interface IShippingContainerRepository : IRepository<ShippingContainer>
{
    /// <summary>
    /// Retrieves a shipping container by its business identifier
    /// </summary>
    /// <param name="containerId">The business identifier of the container</param>
    /// <returns>The container if found, null otherwise</returns>
    Task<ShippingContainer?> GetByContainerIdAsync(string containerId);
    
    /// <summary>
    /// Retrieves containers filtered by their current status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    /// <returns>Collection of containers with the specified status</returns>
    Task<IEnumerable<ShippingContainer>> GetByStatusAsync(ShippingContainerStatus status);
    
    /// <summary>
    /// Retrieves containers within a specific shipping date range
    /// </summary>
    /// <param name="startDate">The start date of the range (inclusive)</param>
    /// <param name="endDate">The end date of the range (inclusive)</param>
    /// <returns>Collection of containers within the date range</returns>
    Task<IEnumerable<ShippingContainer>> GetByShippingDateRangeAsync(DateTime startDate, DateTime endDate);
}