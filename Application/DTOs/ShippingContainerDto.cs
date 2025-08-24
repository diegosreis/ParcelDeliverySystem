using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Represents a shipping container with summary information
/// </summary>
/// <param name="Id">Unique identifier for the container</param>
/// <param name="ContainerId">Business identifier for the container</param>
/// <param name="ShippingDate">Date when the container is scheduled to ship</param>
/// <param name="Status">Current status of the container</param>
/// <param name="TotalParcels">Total number of parcels in the container</param>
/// <param name="TotalWeight">Combined weight of all parcels in kg</param>
/// <param name="TotalValue">Combined monetary value of all parcels</param>
/// <param name="ParcelsRequiringInsurance">Number of parcels requiring insurance approval</param>
/// <param name="CreatedAt">Timestamp when the container was created</param>
/// <param name="UpdatedAt">Timestamp when the container was last updated</param>
public record ShippingContainerDto(
    Guid Id,
    string ContainerId,
    DateTime ShippingDate,
    ShippingContainerStatus Status,
    int TotalParcels,
    decimal TotalWeight,
    decimal TotalValue,
    int ParcelsRequiringInsurance,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Data transfer object for creating a new shipping container
/// </summary>
/// <param name="ContainerId">Business identifier for the container</param>
/// <param name="ShippingDate">Date when the container is scheduled to ship</param>
public record CreateShippingContainerDto(
    string ContainerId,
    DateTime ShippingDate
);

/// <summary>
/// Data transfer object for updating an existing shipping container
/// </summary>
/// <param name="ShippingDate">Updated shipping date</param>
/// <param name="Status">Updated container status</param>
public record UpdateShippingContainerDto(
    DateTime ShippingDate,
    ShippingContainerStatus Status
);

/// <summary>
/// Represents a shipping container with detailed parcel information
/// </summary>
/// <param name="Id">Unique identifier for the container</param>
/// <param name="ContainerId">Business identifier for the container</param>
/// <param name="ShippingDate">Date when the container is scheduled to ship</param>
/// <param name="Status">Current status of the container</param>
/// <param name="Parcels">Collection of parcels in the container</param>
/// <param name="CreatedAt">Timestamp when the container was created</param>
/// <param name="UpdatedAt">Timestamp when the container was last updated</param>
public record ShippingContainerWithParcelsDto(
    Guid Id,
    string ContainerId,
    DateTime ShippingDate,
    ShippingContainerStatus Status,
    IEnumerable<ParcelDto> Parcels,
    DateTime CreatedAt,
    DateTime? UpdatedAt
)
{
    /// <summary>
    /// Gets the total number of parcels in the container
    /// </summary>
    public int TotalParcels => Parcels.Count();
    
    /// <summary>
    /// Gets the combined weight of all parcels in kg
    /// </summary>
    public decimal TotalWeight => Parcels.Sum(p => p.Weight);
    
    /// <summary>
    /// Gets the combined monetary value of all parcels
    /// </summary>
    public decimal TotalValue => Parcels.Sum(p => p.Value);
    
    /// <summary>
    /// Gets the number of parcels requiring insurance approval (value > €1000)
    /// </summary>
    public int ParcelsRequiringInsurance => Parcels.Count(p => p.Value > 1000);
}