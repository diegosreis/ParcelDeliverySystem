using System.Text.RegularExpressions;

namespace Domain.Validation;

/// <summary>
///     Provides validation methods for ensuring data integrity and business rules
/// </summary>
public static class Guard
{
    /// <summary>
    ///     Validates that a string value is not null or empty
    /// </summary>
    /// <param name="value">The string value to validate</param>
    /// <param name="paramName">The parameter name for error reporting</param>
    /// <param name="fieldDisplayName">The display name for error messages</param>
    /// <returns>The validated string value</returns>
    /// <exception cref="ArgumentException">Thrown when the value is null or empty</exception>
    public static string Required(string? value, string paramName, string fieldDisplayName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(string.Format(ValidationMessages.Required, fieldDisplayName), paramName);
        return value;
    }

    /// <summary>
    ///     Validates that an object is not null
    /// </summary>
    /// <typeparam name="T">The type of object to validate</typeparam>
    /// <param name="value">The object to validate</param>
    /// <param name="paramName">The parameter name for error reporting</param>
    /// <param name="fieldDisplayName">The display name for error messages</param>
    /// <returns>The validated object</returns>
    /// <exception cref="ArgumentNullException">Thrown when the value is null</exception>
    public static T NotNull<T>(T? value, string paramName, string fieldDisplayName) where T : class
    {
        if (value == null)
            throw new ArgumentNullException(paramName,
                string.Format(ValidationMessages.CannotBeNull, fieldDisplayName));
        return value;
    }

    /// <summary>
    ///     Trims whitespace from a string or returns empty string if null
    /// </summary>
    /// <param name="value">The string value to trim</param>
    /// <returns>Trimmed string or empty string if null</returns>
    public static string TrimOrEmpty(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    /// <summary>
    ///     Validates that a decimal value is greater than a specified minimum
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value</param>
    /// <param name="paramName">The parameter name for error reporting</param>
    /// <param name="fieldDisplayName">The display name for error messages</param>
    /// <returns>The validated value</returns>
    /// <exception cref="ArgumentException">Thrown when the value is not greater than minimum</exception>
    public static decimal GreaterThan(decimal value, decimal min, string paramName, string fieldDisplayName)
    {
        if (value <= min)
            throw new ArgumentException(string.Format(ValidationMessages.GreaterThan, fieldDisplayName, min),
                paramName);
        return value;
    }

    /// <summary>
    ///     Validates that a decimal value is not negative
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="paramName">The parameter name for error reporting</param>
    /// <param name="fieldDisplayName">The display name for error messages</param>
    /// <returns>The validated value</returns>
    /// <exception cref="ArgumentException">Thrown when the value is negative</exception>
    public static decimal NotNegative(decimal value, string paramName, string fieldDisplayName)
    {
        if (value < 0)
            throw new ArgumentException(string.Format(ValidationMessages.NotNegative, fieldDisplayName), paramName);
        return value;
    }

    /// <summary>
    ///     Validates that a string follows Dutch postal code format (1234AB)
    /// </summary>
    /// <param name="value">The postal code to validate</param>
    /// <param name="paramName">The parameter name for error reporting</param>
    /// <param name="fieldDisplayName">The display name for error messages</param>
    /// <returns>The validated and normalized postal code in uppercase</returns>
    /// <exception cref="ArgumentException">Thrown when the value is null, empty, or invalid format</exception>
    public static string DutchPostcode(string? value, string paramName, string fieldDisplayName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(string.Format(ValidationMessages.Required, fieldDisplayName), paramName);

        // Normalize to uppercase for validation
        var normalizedValue = value.ToUpperInvariant();

        // Dutch postal code format: 1234AB (4 digits + 2 letters)
        if (!Regex.IsMatch(normalizedValue, @"^\d{4}[A-Z]{2}$"))
            throw new ArgumentException(ValidationMessages.DutchPostcode, paramName);

        return normalizedValue;
    }

    /// <summary>
    ///     Validates that a DateTime is not the default value
    /// </summary>
    /// <param name="value">The DateTime to validate</param>
    /// <param name="paramName">The parameter name for error reporting</param>
    /// <param name="fieldDisplayName">The display name for error messages</param>
    /// <returns>The validated DateTime</returns>
    /// <exception cref="ArgumentException">Thrown when the DateTime is default</exception>
    public static DateTime NotDefault(DateTime value, string paramName, string fieldDisplayName)
    {
        if (value == default)
            throw new ArgumentException(string.Format(ValidationMessages.CannotBeDefault, fieldDisplayName),
                paramName);
        return value;
    }
}

/// <summary>
///     Contains validation error message templates
/// </summary>
public static class ValidationMessages
{
    /// <summary>
    ///     Template for required field validation errors
    /// </summary>
    public const string Required = "{0} is required and cannot be empty.";

    /// <summary>
    ///     Template for null value validation errors
    /// </summary>
    public const string CannotBeNull = "{0} cannot be null.";

    /// <summary>
    ///     Template for negative value validation errors
    /// </summary>
    public const string NotNegative = "{0} cannot be negative.";

    /// <summary>
    ///     Template for greater than validation errors
    /// </summary>
    public const string GreaterThan = "{0} must be greater than {1}.";

    /// <summary>
    ///     Error message for Dutch postal code format validation
    /// </summary>
    public const string DutchPostcode = "Zip code must be in Dutch format 1234AB (4 digits + 2 letters)";

    /// <summary>
    ///     Template for default value validation errors
    /// </summary>
    public const string CannotBeDefault = "{0} cannot be the default value.";
}

/// <summary>
///     Contains field names used in validation error messages
/// </summary>
public static class FieldNames
{
    /// <summary>
    ///     Display name for street address field
    /// </summary>
    public const string Street = "Street";

    /// <summary>
    ///     Display name for house number field
    /// </summary>
    public const string Number = "Number";

    /// <summary>
    ///     Display name for neighborhood field
    /// </summary>
    public const string Neighborhood = "Neighborhood";

    /// <summary>
    ///     Display name for city field
    /// </summary>
    public const string City = "City";

    /// <summary>
    ///     Display name for state field
    /// </summary>
    public const string State = "State";

    /// <summary>
    ///     Display name for country field
    /// </summary>
    public const string Country = "Country";

    /// <summary>
    ///     Display name for zip code field
    /// </summary>
    public const string ZipCode = "Zip Code";

    /// <summary>
    ///     Display name for name field
    /// </summary>
    public const string Name = "Name";

    /// <summary>
    ///     Display name for department name field
    /// </summary>
    public const string DepartmentName = "Department Name";

    /// <summary>
    ///     Display name for weight field
    /// </summary>
    public const string Weight = "Weight";

    /// <summary>
    ///     Display name for value field
    /// </summary>
    public const string Value = "Value";

    /// <summary>
    ///     Display name for recipient field
    /// </summary>
    public const string Recipient = "Recipient";

    /// <summary>
    ///     Display name for address field
    /// </summary>
    public const string Address = "Address";

    /// <summary>
    ///     Display name for department field
    /// </summary>
    public const string Department = "Department";

    /// <summary>
    ///     Display name for container ID field
    /// </summary>
    public const string ContainerId = "Container ID";

    /// <summary>
    ///     Display name for shipping date field
    /// </summary>
    public const string ShippingDate = "Shipping Date";

    /// <summary>
    ///     Display name for parcel field
    /// </summary>
    public const string Parcel = "Parcel";
}