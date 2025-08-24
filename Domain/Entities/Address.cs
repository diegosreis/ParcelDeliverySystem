using Domain.Validation;

namespace Domain.Entities;

/// <summary>
///     Represents a physical address with Dutch postal code validation
/// </summary>
public class Address
{
    /// <summary>
    ///     Private constructor for Entity Framework Core
    /// </summary>
    private Address()
    {
    } // EF Core constructor

    /// <summary>
    ///     Initializes a new instance of the Address class with validation
    /// </summary>
    /// <param name="street">The street name</param>
    /// <param name="number">The house or building number</param>
    /// <param name="complement">Optional complement information (apartment, unit, etc.)</param>
    /// <param name="neighborhood">The neighborhood or district</param>
    /// <param name="city">The city name</param>
    /// <param name="state">The state or province</param>
    /// <param name="zipCode">The postal code in Dutch format (1234AB)</param>
    /// <param name="country">The country name (defaults to "Nederlands")</param>
    /// <exception cref="ArgumentException">Thrown when any required field is empty or zipCode format is invalid</exception>
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

    /// <summary>
    ///     Gets the unique identifier for this address
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    ///     Gets the street name
    /// </summary>
    public string Street { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the house or building number
    /// </summary>
    public string Number { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the complement information (apartment, unit, etc.)
    /// </summary>
    public string Complement { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the neighborhood or district
    /// </summary>
    public string Neighborhood { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the city name
    /// </summary>
    public string City { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the state or province
    /// </summary>
    public string State { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the postal code in Dutch format (1234AB)
    /// </summary>
    public string ZipCode { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the country name
    /// </summary>
    public string Country { get; private set; } = string.Empty;

    /// <summary>
    ///     Gets the date and time when this address was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Gets the date and time when this address was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    ///     Updates the address with new values
    /// </summary>
    /// <param name="street">The new street name</param>
    /// <param name="number">The new house or building number</param>
    /// <param name="complement">The new complement information</param>
    /// <param name="neighborhood">The new neighborhood or district</param>
    /// <param name="city">The new city name</param>
    /// <param name="state">The new state or province</param>
    /// <param name="zipCode">The new postal code in Dutch format</param>
    /// <param name="country">The new country name</param>
    /// <exception cref="ArgumentException">Thrown when any required field is empty or zipCode format is invalid</exception>
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

    /// <summary>
    ///     Gets a formatted string representation of the address
    /// </summary>
    /// <returns>A comma-separated string containing all address components</returns>
    public string GetFormattedAddress()
    {
        var parts = new List<string> { Street, Number };

        if (!string.IsNullOrWhiteSpace(Complement))
            parts.Add(Complement);

        parts.Add(Neighborhood);
        parts.Add($"{City} - {State}");
        parts.Add(ZipCode);
        parts.Add(Country);

        return string.Join(", ", parts);
    }
}