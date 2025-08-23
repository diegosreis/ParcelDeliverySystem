namespace Domain.Validation;

public static class Guard
{
    public static string Required(string? value, string paramName, string fieldDisplayName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException(string.Format(ValidationMessages.Required, fieldDisplayName), paramName)
            : value.Trim();
    }

    public static T NotNull<T>(T? value, string paramName, string fieldDisplayName) where T : class
    {
        return value ?? throw new ArgumentNullException(paramName,
            string.Format(ValidationMessages.CannotBeNull, fieldDisplayName));
    }

    public static string TrimOrEmpty(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    public static decimal GreaterThan(decimal value, decimal minExclusive, string paramName, string fieldDisplayName)
    {
        if (value <= minExclusive)
            throw new ArgumentException(string.Format(ValidationMessages.GreaterThan, fieldDisplayName, minExclusive),
                paramName);
        return value;
    }

    public static decimal NotNegative(decimal value, string paramName, string fieldDisplayName)
    {
        return value < 0
            ? throw new ArgumentException(string.Format(ValidationMessages.NotNegative, fieldDisplayName), paramName)
            : value;
    }

    public static string DutchPostcode(string? value, string paramName, string fieldDisplayName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(string.Format(ValidationMessages.Required, fieldDisplayName), paramName);

        var clean = value.Trim().ToUpperInvariant();

        if (clean.Length == 6 &&
            clean[..4].All(char.IsDigit) &&
            clean[4..].All(char.IsLetter))
            return clean;

        throw new ArgumentException(string.Format(ValidationMessages.DutchPostcode, fieldDisplayName), paramName);
    }
}

public static class ValidationMessages
{
    public const string Required = "{0} cannot be empty";
    public const string CannotBeNull = "{0} cannot be null";
    public const string NotNegative = "{0} cannot be negative";
    public const string GreaterThan = "{0} must be greater than {1}";
    public const string DutchPostcode = "{0} must be in Dutch format 1234AB (4 digits + 2 letters)";
}

public static class FieldNames
{
    public const string Street = "Street";
    public const string Number = "Number";
    public const string Neighborhood = "Neighborhood";
    public const string City = "City";
    public const string State = "State";
    public const string Country = "Country";
    public const string ZipCode = "Zip code";
    public const string Name = "Name";
    public const string DepartmentName = "Department name";
    public const string Weight = "Weight";
    public const string Value = "Value";
    public const string Recipient = "Recipient";
    public const string Address = "Address";
    public const string Department = "Department";
}