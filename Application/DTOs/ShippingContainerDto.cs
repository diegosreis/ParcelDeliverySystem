using Domain.Enums;

namespace Application.DTOs;

public record ShippingContainerDto(
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

public record CreateShippingContainerDto(
    string ContainerId,
    DateTime ShippingDate
);

public record UpdateShippingContainerDto(
    DateTime ShippingDate,
    ContainerStatus Status
);

public record ShippingContainerWithParcelsDto(
    Guid Id,
    string ContainerId,
    DateTime ShippingDate,
    ContainerStatus Status,
    List<ParcelDto> Parcels,
    int TotalParcels,
    decimal TotalWeight,
    decimal TotalValue,
    int ParcelsRequiringInsurance,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);