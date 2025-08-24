using Application.DTOs;
using Domain.Enums;

namespace Application.Services;

/// <summary>
///     Service interface for managing parcels in the delivery system.
/// </summary>
public interface IParcelService
{
    /// <summary>
    ///     Retrieves all parcels in the system
    /// </summary>
    /// <returns>A collection of all parcels</returns>
    Task<IEnumerable<ParcelDto>> GetAllParcelsAsync();

    /// <summary>
    ///     Retrieves a specific parcel by ID
    /// </summary>
    /// <param name="id">The unique identifier of the parcel</param>
    /// <returns>The parcel with the specified ID, or null if not found</returns>
    Task<ParcelDto?> GetParcelByIdAsync(Guid id);

    /// <summary>
    ///     Retrieves parcels filtered by processing status
    /// </summary>
    /// <param name="status">The parcel status to filter by</param>
    /// <returns>A collection of parcels with the specified status</returns>
    Task<IEnumerable<ParcelDto>> GetParcelsByStatusAsync(ParcelStatus status);

    /// <summary>
    ///     Retrieves parcels that require insurance approval (value > €1000)
    /// </summary>
    /// <returns>A collection of high-value parcels requiring insurance approval</returns>
    Task<IEnumerable<ParcelDto>> GetParcelsRequiringInsuranceAsync();

    /// <summary>
    ///     Retrieves parcels by weight range
    /// </summary>
    /// <param name="minWeight">Minimum weight in kg</param>
    /// <param name="maxWeight">Maximum weight in kg</param>
    /// <returns>A collection of parcels within the specified weight range</returns>
    Task<IEnumerable<ParcelDto>> GetParcelsByWeightRangeAsync(decimal minWeight, decimal maxWeight);

    /// <summary>
    ///     Retrieves parcels by shipping container ID
    /// </summary>
    /// <param name="containerId">The unique identifier of the shipping container</param>
    /// <returns>A collection of parcels in the specified container</returns>
    Task<IEnumerable<ParcelDto>> GetParcelsByContainerAsync(Guid containerId);
}