using Domain.Enums;
using Domain.Validation;

namespace Domain.Entities;

/// <summary>
/// Represents a shipping container that holds multiple parcels for delivery
/// </summary>
public class ShippingContainer
{
    private readonly List<Parcel> _parcels = new();

    private ShippingContainer()
    {
    } // EF Core constructor

    /// <summary>
    /// Creates a new shipping container with the specified container ID and shipping date
    /// </summary>
    /// <param name="containerId">Business identifier for the container</param>
    /// <param name="shippingDate">Date when the container is scheduled to ship</param>
    /// <exception cref="ArgumentException">Thrown when container ID is null or empty, or shipping date is default</exception>
    public ShippingContainer(string containerId, DateTime shippingDate)
    {
        Id = Guid.NewGuid();
        ContainerId = Guard.Required(containerId, nameof(containerId), FieldNames.ContainerId);
        ShippingDate = Guard.NotDefault(shippingDate, nameof(shippingDate), FieldNames.ShippingDate);
        Status = ShippingContainerStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unique identifier for the container
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Business identifier for the container
    /// </summary>
    public string ContainerId { get; private set; } = string.Empty;
    
    /// <summary>
    /// Date when the container is scheduled to ship
    /// </summary>
    public DateTime ShippingDate { get; private set; }
    
    /// <summary>
    /// Collection of parcels contained in this shipping container
    /// </summary>
    public IReadOnlyList<Parcel> Parcels => _parcels.AsReadOnly();
    
    /// <summary>
    /// Current processing status of the shipping container
    /// </summary>
    public ShippingContainerStatus Status { get; private set; }
    
    /// <summary>
    /// Timestamp when the container was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>
    /// Timestamp when the container was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    // Computed properties
    /// <summary>
    /// Gets the total number of parcels in the container
    /// </summary>
    public int TotalParcels => _parcels.Count;
    
    /// <summary>
    /// Gets the combined weight of all parcels in kilograms
    /// </summary>
    public decimal TotalWeight => _parcels.Sum(p => p.Weight);
    
    /// <summary>
    /// Gets the combined monetary value of all parcels
    /// </summary>
    public decimal TotalValue => _parcels.Sum(p => p.Value);
    
    /// <summary>
    /// Gets the number of parcels requiring insurance approval (value > €1000)
    /// </summary>
    public int ParcelsRequiringInsurance => _parcels.Count(p => p.RequiresInsuranceApproval);

    /// <summary>
    /// Adds a parcel to the container
    /// </summary>
    /// <param name="parcel">Parcel to add to the container</param>
    /// <exception cref="ArgumentNullException">Thrown when parcel is null</exception>
    public void AddParcel(Parcel parcel)
    {
        Guard.NotNull(parcel, nameof(parcel), FieldNames.Parcel);
        _parcels.Add(parcel);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes a parcel from the container
    /// </summary>
    /// <param name="parcel">Parcel to remove from the container</param>
    /// <exception cref="ArgumentNullException">Thrown when parcel is null</exception>
    public void RemoveParcel(Parcel parcel)
    {
        Guard.NotNull(parcel, nameof(parcel), FieldNames.Parcel);
        _parcels.Remove(parcel);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the processing status of the container
    /// </summary>
    /// <param name="status">New status for the container</param>
    public void UpdateStatus(ShippingContainerStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the shipping date of the container
    /// </summary>
    /// <param name="shippingDate">New shipping date</param>
    /// <exception cref="ArgumentException">Thrown when shipping date is default</exception>
    public void UpdateShippingDate(DateTime shippingDate)
    {
        ShippingDate = Guard.NotDefault(shippingDate, nameof(shippingDate), FieldNames.ShippingDate);
        UpdatedAt = DateTime.UtcNow;
    }
}