using Domain.Enums;

namespace Application.DTOs;

public record ContainerDto(
    Guid Id,
    string ContainerId,
    DateTime ShippingDate,
    ContainerStatus Status,
    int TotalParcels,
    decimal TotalWeight,
    decimal TotalValue,
    int ParcelsRequiringInsurance,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateContainerDto(
    string ContainerId,
    DateTime ShippingDate
);

public record UpdateContainerDto(
    DateTime ShippingDate
);

public record ContainerWithParcelsDto(
    Guid Id,
    string ContainerId,
    DateTime ShippingDate,
    ContainerStatus Status,
    IEnumerable<ParcelDto> Parcels,
    int TotalParcels,
    decimal TotalWeight,
    decimal TotalValue,
    int ParcelsRequiringInsurance,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);