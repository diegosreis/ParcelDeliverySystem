using Domain.Enums;
using Domain.Validation;

namespace Domain.Entities;

public class Container
{
    private Container()
    {
        Parcels = [];
    } // EF Core constructor

    public Container(string containerId, DateTime shippingDate)
    {
        Id = Guid.NewGuid();

        ContainerId = Guard.Required(containerId, nameof(containerId), FieldNames.ContainerId);
        ShippingDate = Guard.NotDefault(shippingDate, nameof(shippingDate), FieldNames.ShippingDate);

        Parcels = [];
        Status = ContainerStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string ContainerId { get; private set; } = string.Empty;
    public DateTime ShippingDate { get; private set; }
    public List<Parcel> Parcels { get; }
    public ContainerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }


    public int TotalParcels => Parcels.Count;

    public decimal TotalWeight => Parcels.Sum(p => p.Weight);

    public decimal TotalValue => Parcels.Sum(p => p.Value);

    public int ParcelsRequiringInsurance => Parcels.Count(p => p.RequiresInsuranceApproval);

    public void AddParcel(Parcel parcel)
    {
        var validParcel = Guard.NotNull(parcel, nameof(parcel), FieldNames.Parcel);

        if (Parcels.Contains(validParcel)) return;
        Parcels.Add(validParcel);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveParcel(Parcel parcel)
    {
        var validParcel = Guard.NotNull(parcel, nameof(parcel), FieldNames.Parcel);

        if (!Parcels.Contains(validParcel)) return;
        Parcels.Remove(validParcel);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ContainerStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateShippingDate(DateTime shippingDate)
    {
        ShippingDate = Guard.NotDefault(shippingDate, nameof(shippingDate), FieldNames.ShippingDate);
        UpdatedAt = DateTime.UtcNow;
    }
}