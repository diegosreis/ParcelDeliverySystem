namespace Application.DTOs;

public record AddressDto(
    Guid Id,
    string Street,
    string Number,
    string Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateAddressDto(
    string Street,
    string Number,
    string Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country = "Nederlands"
);

public record UpdateAddressDto(
    string Street,
    string Number,
    string Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string Country
);