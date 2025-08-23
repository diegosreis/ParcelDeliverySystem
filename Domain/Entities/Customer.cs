using Domain.Validation;

namespace Domain.Entities;

public class Customer
{
    private Customer()
    {
    } // EF Core constructor

    public Customer(string name, Address address)
    {
        Id = Guid.NewGuid();

        Name = Guard.Required(name, nameof(name), FieldNames.Name);
        Address = Guard.NotNull(address, nameof(address), FieldNames.Address);

        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Update(string name, Address address)
    {
        Name = Guard.Required(name, nameof(name), FieldNames.Name);
        Address = Guard.NotNull(address, nameof(address), FieldNames.Address);

        UpdatedAt = DateTime.UtcNow;
    }
}