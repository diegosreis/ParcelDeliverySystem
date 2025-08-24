using Application.DTOs;
using Domain.Enums;

namespace Application.Services;

/// <summary>
///     Service interface for managing shipping containers in the delivery system.
/// </summary>
public interface IShippingContainerService
{
    /// <summary>
    ///     Retrieves all shipping containers in the system
    /// </summary>
    /// <param name="includeParcelDetails">Whether to include detailed parcel information</param>
    /// <returns>A collection of all shipping containers</returns>
    Task<IEnumerable<ShippingContainerDto>> GetAllContainersAsync(bool includeParcelDetails = false);

    /// <summary>
    ///     Retrieves a specific shipping container by ID
    /// </summary>
    /// <param name="id">The unique identifier of the container</param>
    /// <param name="includeParcelDetails">Whether to include detailed parcel information</param>
    /// <returns>The container with the specified ID, or null if not found</returns>
    Task<ShippingContainerDto?> GetContainerByIdAsync(Guid id, bool includeParcelDetails = false);

    /// <summary>
    ///     Retrieves shipping containers by status
    /// </summary>
    /// <param name="status">The container status to filter by</param>
    /// <param name="includeParcelDetails">Whether to include detailed parcel information</param>
    /// <returns>A collection of containers with the specified status</returns>
    Task<IEnumerable<ShippingContainerDto>> GetContainersByStatusAsync(ShippingContainerStatus status,
        bool includeParcelDetails = false);

    /// <summary>
    ///     Retrieves containers within a specific date range
    /// </summary>
    /// <param name="startDate">Start date for the range</param>
    /// <param name="endDate">End date for the range</param>
    /// <param name="includeParcelDetails">Whether to include detailed parcel information</param>
    /// <returns>A collection of containers shipped within the date range</returns>
    Task<IEnumerable<ShippingContainerDto>> GetContainersByDateRangeAsync(DateTime startDate, DateTime endDate,
        bool includeParcelDetails = false);

    /// <summary>
    ///     Gets detailed container information including all parcels
    /// </summary>
    /// <param name="id">The unique identifier of the container</param>
    /// <returns>Detailed container information with parcels, or null if not found</returns>
    Task<ShippingContainerWithParcelsDto?> GetContainerWithParcelsAsync(Guid id);
}