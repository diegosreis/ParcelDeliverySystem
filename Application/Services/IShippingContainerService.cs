using Application.DTOs;

namespace Application.Services;

/// <summary>
///     Service interface for managing shipping containers in the delivery system.
///     Provides business logic for container operations while maintaining separation of concerns.
/// </summary>
public interface IShippingContainerService
{
    /// <summary>
    ///     Retrieves all shipping containers in the system
    /// </summary>
    /// <returns>A collection of all shipping containers</returns>
    Task<IEnumerable<ShippingContainerDto>> GetAllContainersAsync();

    /// <summary>
    ///     Retrieves a specific shipping container by ID
    /// </summary>
    /// <param name="id">The unique identifier of the container</param>
    /// <returns>The container with the specified ID, or null if not found</returns>
    Task<ShippingContainerDto?> GetContainerByIdAsync(Guid id);

    /// <summary>
    ///     Retrieves shipping containers by status
    /// </summary>
    /// <param name="status">The container status to filter by</param>
    /// <returns>A collection of containers with the specified status</returns>
    Task<IEnumerable<ShippingContainerDto>> GetContainersByStatusAsync(Domain.Enums.ShippingContainerStatus status);

    /// <summary>
    ///     Retrieves containers within a specific date range
    /// </summary>
    /// <param name="startDate">Start date for the range</param>
    /// <param name="endDate">End date for the range</param>
    /// <returns>A collection of containers shipped within the date range</returns>
    Task<IEnumerable<ShippingContainerDto>> GetContainersByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    ///     Gets detailed container information including all parcels
    /// </summary>
    /// <param name="id">The unique identifier of the container</param>
    /// <returns>Detailed container information with parcels, or null if not found</returns>
    Task<ShippingContainerDto?> GetContainerWithParcelsAsync(Guid id);
}
