namespace Application.DTOs;

public record CustomerDto(
    Guid Id,
    string Name,
    AddressDto Address,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateCustomerDto(
    string Name,
    CreateAddressDto Address
);

public record UpdateCustomerDto(
    string Name,
    UpdateAddressDto Address
);