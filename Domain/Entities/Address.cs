using Domain.Validation;

namespace Domain.Entities;

public class Address
{
    private Address()
    {
    } // EF Core constructor

    public Address(string street, string number, string complement, string neighborhood,
        string city, string state, string zipCode, string country = "Nederlands")
    {
        Id = Guid.NewGuid();

        Street = Guard.Required(street, nameof(street), FieldNames.Street);
        Number = Guard.Required(number, nameof(number), FieldNames.Number);
        Complement = Guard.TrimOrEmpty(complement);
        Neighborhood = Guard.Required(neighborhood, nameof(neighborhood), FieldNames.Neighborhood);
        City = Guard.Required(city, nameof(city), FieldNames.City);
        State = Guard.Required(state, nameof(state), FieldNames.State);
        ZipCode = Guard.DutchPostcode(zipCode, nameof(zipCode), FieldNames.ZipCode);
        Country = Guard.Required(country, nameof(country), FieldNames.Country);

        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Street { get; private set; } = string.Empty;
    public string Number { get; private set; } = string.Empty;
    public string Complement { get; private set; } = string.Empty;
    public string Neighborhood { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Update(string street, string number, string complement, string neighborhood,
        string city, string state, string zipCode, string country)
    {
        Street = Guard.Required(street, nameof(street), FieldNames.Street);
        Number = Guard.Required(number, nameof(number), FieldNames.Number);
        Complement = Guard.TrimOrEmpty(complement);
        Neighborhood = Guard.Required(neighborhood, nameof(neighborhood), FieldNames.Neighborhood);
        City = Guard.Required(city, nameof(city), FieldNames.City);
        State = Guard.Required(state, nameof(state), FieldNames.State);
        ZipCode = Guard.DutchPostcode(zipCode, nameof(zipCode), FieldNames.ZipCode);
        Country = Guard.Required(country, nameof(country), FieldNames.Country);

        UpdatedAt = DateTime.UtcNow;
    }

    public string GetFormattedAddress()
    {
        var parts = new List<string>
        {
            $"{Street}, {Number}",
            Complement,
            Neighborhood,
            $"{City} - {State}",
            ZipCode,
            Country
        };

        return string.Join(", ", parts.Where(p => !string.IsNullOrEmpty(p)));
    }
}