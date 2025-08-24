namespace Application.DTOs;

/// <summary>
/// Represents a complete address with all location details
/// </summary>
/// <param name="Id">Unique identifier for the address</param>
/// <param name="Street">Street name</param>
/// <param name="Number">House or building number</param>
/// <param name="Complement">Additional address information (apartment, suite, etc.)</param>
/// <param name="Neighborhood">Neighborhood or district name</param>
/// <param name="City">City name</param>
/// <param name="State">State or province code</param>
/// <param name="ZipCode">Postal code in Dutch format (1234AB)</param>
/// <param name="Country">Country name</param>
/// <param name="CreatedAt">Timestamp when the address was created</param>
/// <param name="UpdatedAt">Timestamp when the address was last updated</param>
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

/// <summary>
/// Data transfer object for creating a new address
/// </summary>
/// <param name="Street">Street name</param>
/// <param name="Number">House or building number</param>
/// <param name="Complement">Additional address information (apartment, suite, etc.)</param>
/// <param name="Neighborhood">Neighborhood or district name</param>
/// <param name="City">City name</param>
/// <param name="State">State or province code</param>
/// <param name="ZipCode">Postal code in Dutch format (1234AB)</param>
/// <param name="Country">Country name</param>
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

/// <summary>
/// Data transfer object for updating an existing address
/// </summary>
/// <param name="Street">Updated street name</param>
/// <param name="Number">Updated house or building number</param>
/// <param name="Complement">Updated additional address information</param>
/// <param name="Neighborhood">Updated neighborhood or district name</param>
/// <param name="City">Updated city name</param>
/// <param name="State">Updated state or province code</param>
/// <param name="ZipCode">Updated postal code in Dutch format (1234AB)</param>
/// <param name="Country">Updated country name</param>
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