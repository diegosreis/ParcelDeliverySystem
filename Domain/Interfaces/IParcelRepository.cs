using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

/// <summary>
/// Repository interface for managing parcel entities with specialized queries for delivery processing
/// </summary>
public interface IParcelRepository : IRepository<Parcel>
{
    /// <summary>
    /// Retrieves parcels filtered by their current processing status
    /// </summary>
    /// <param name="status">The status to filter by</param>
    /// <returns>Collection of parcels with the specified status</returns>
    Task<IEnumerable<Parcel>> GetByStatusAsync(ParcelStatus status);
    
    /// <summary>
    /// Retrieves parcels within a specific weight range
    /// </summary>
    /// <param name="minWeight">The minimum weight in kilograms (inclusive)</param>
    /// <param name="maxWeight">The maximum weight in kilograms (inclusive)</param>
    /// <returns>Collection of parcels within the weight range</returns>
    Task<IEnumerable<Parcel>> GetByWeightRangeAsync(decimal minWeight, decimal maxWeight);
    
    /// <summary>
    /// Retrieves parcels within a specific value range
    /// </summary>
    /// <param name="minValue">The minimum monetary value (inclusive)</param>
    /// <param name="maxValue">The maximum monetary value (inclusive)</param>
    /// <returns>Collection of parcels within the value range</returns>
    Task<IEnumerable<Parcel>> GetByValueRangeAsync(decimal minValue, decimal maxValue);
    
    /// <summary>
    /// Retrieves parcels that require insurance approval (value > €1000)
    /// </summary>
    /// <returns>Collection of parcels requiring insurance approval</returns>
    Task<IEnumerable<Parcel>> GetRequiringInsuranceAsync();
    
    /// <summary>
    /// Retrieves all parcels belonging to a specific container
    /// </summary>
    /// <param name="containerId">The unique identifier of the container</param>
    /// <returns>Collection of parcels in the specified container</returns>
    Task<IEnumerable<Parcel>> GetByContainerIdAsync(Guid containerId);
}