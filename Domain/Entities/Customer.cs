using Domain.Validation;

namespace Domain.Entities;

/// <summary>
///     Represents a customer in the parcel delivery system
/// </summary>
public class Customer
{
    /// <summary>
    ///     Private constructor for Entity Framework Core
    /// </summary>
    private Customer()
    {
    } // EF Core constructor

    /// <summary>
    ///     Initializes a new instance of the Customer class
    /// </summary>
    /// <param name="name">The customer's full name</param>
    /// <param name="address">The customer's address</param>
    /// <exception cref="ArgumentException">Thrown when name is empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when address is null</exception>
    public Customer(string name, Address address)
    {
        Id = Guid.NewGuid();
        Name = Guard.Required(name, nameof(name), FieldNames.Name);
        Address = Guard.NotNull(address, nameof(address), FieldNames.Address);
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Gets the unique identifier for this customer
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Gets the customer's full name
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the customer's address
    /// </summary>
    public Address Address { get; private set; } = null!;

    /// <summary>
    ///     Gets the date and time when this customer was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the date and time when this customer was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    ///     Updates the customer with new information
    /// </summary>
    /// <param name="name">The new customer name</param>
    /// <param name="address">The new customer address</param>
    /// <exception cref="ArgumentException">Thrown when name is empty</exception>
    /// <exception cref="ArgumentNullException">Thrown when address is null</exception>
    public void Update(string name, Address address)
    {
        Name = Guard.Required(name, nameof(name), FieldNames.Name);
        Address = Guard.NotNull(address, nameof(address), FieldNames.Address);

        UpdatedAt = DateTime.UtcNow;
    }
}