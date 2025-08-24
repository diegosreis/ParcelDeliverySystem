using Domain.Enums;
using Domain.Validation;

namespace Domain.Entities;

/// <summary>
///     Represents a configurable business rule for parcel routing and department assignment
/// </summary>
public class BusinessRule(
    string name,
    string description,
    BusinessRuleType type,
    decimal minValue,
    decimal? maxValue,
    string targetDepartment)
{
    /// <summary>
    ///     Gets the unique identifier for this business rule
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    ///     Gets the name of the business rule
    /// </summary>
    public string Name { get; private set; } = Guard.Required(name, nameof(name), FieldNames.Name);

    /// <summary>
    ///     Gets the description of what this business rule does
    /// </summary>
    public string Description { get; private set; } = Guard.Required(description, nameof(description), "Description");

    /// <summary>
    ///     Gets the type of business rule (Weight, Value, or Combined)
    /// </summary>
    public BusinessRuleType Type { get; private set; } = type;

    /// <summary>
    ///     Gets the minimum value for the rule condition
    /// </summary>
    public decimal MinValue { get; private set; } = Guard.NotNegative(minValue, nameof(minValue), "Minimum Value");

    /// <summary>
    ///     Gets the maximum value for the rule condition (null means no upper limit)
    /// </summary>
    public decimal? MaxValue { get; private set; } = maxValue.HasValue
        ? Guard.GreaterThan(maxValue.Value, minValue, nameof(maxValue), "Maximum Value")
        : null;

    /// <summary>
    ///     Gets the target department that should handle parcels matching this rule
    /// </summary>
    public string TargetDepartment { get; private set; } =
        Guard.Required(targetDepartment, nameof(targetDepartment), FieldNames.DepartmentName);

    /// <summary>
    ///     Gets a value indicating whether this business rule is currently active
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    ///     Gets the date and time when this business rule was created
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    ///     Gets the date and time when this business rule was last updated
    /// </summary>
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    ///     Updates the business rule with new values
    /// </summary>
    /// <param name="name">The new name for the rule</param>
    /// <param name="description">The new description</param>
    /// <param name="minValue">The new minimum value</param>
    /// <param name="maxValue">The new maximum value (null for no upper limit)</param>
    /// <param name="targetDepartment">The new target department</param>
    /// <exception cref="ArgumentException">Thrown when validation fails</exception>
    public void Update(string name, string description, decimal minValue, decimal? maxValue, string targetDepartment)
    {
        Name = Guard.Required(name, nameof(name), FieldNames.Name);
        Description = Guard.Required(description, nameof(description), "Description");
        MinValue = Guard.NotNegative(minValue, nameof(minValue), "Minimum Value");
        MaxValue = maxValue.HasValue
            ? Guard.GreaterThan(maxValue.Value, minValue, nameof(maxValue), "Maximum Value")
            : null;
        TargetDepartment = Guard.Required(targetDepartment, nameof(targetDepartment), FieldNames.DepartmentName);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Activates this business rule
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Deactivates this business rule
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}