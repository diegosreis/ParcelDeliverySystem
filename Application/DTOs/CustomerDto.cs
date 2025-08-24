namespace Application.DTOs;

/// <summary>
/// Represents a customer with personal information and address
/// </summary>
/// <param name="Id">Unique identifier for the customer</param>
/// <param name="Name">Full name of the customer</param>
/// <param name="Address">Physical address information</param>
/// <param name="CreatedAt">Timestamp when the customer record was created</param>
/// <param name="UpdatedAt">Timestamp when the customer record was last updated</param>
public record CustomerDto(
    Guid Id,
    string Name,
    AddressDto Address,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Data transfer object for creating a new customer
/// </summary>
/// <param name="Name">Full name of the customer</param>
/// <param name="Address">Address information for the new customer</param>
public record CreateCustomerDto(
    string Name,
    CreateAddressDto Address
);

/// <summary>
/// Data transfer object for updating an existing customer
/// </summary>
/// <param name="Name">Updated full name of the customer</param>
/// <param name="Address">Updated address information</param>
public record UpdateCustomerDto(
    string Name,
    UpdateAddressDto Address
);