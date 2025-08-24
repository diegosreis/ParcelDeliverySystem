using Domain.Enums;

namespace Application.DTOs;

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

public record CreateShippingContainerDto(
    string ContainerId,
    DateTime ShippingDate
);

public record UpdateShippingContainerDto(
    DateTime ShippingDate,
    ShippingContainerStatus Status
);

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
    public int TotalParcels => Parcels.Count();
    public decimal TotalWeight => Parcels.Sum(p => p.Weight);
    public decimal TotalValue => Parcels.Sum(p => p.Value);
    public int ParcelsRequiringInsurance => Parcels.Count(p => p.Value > 1000);
}