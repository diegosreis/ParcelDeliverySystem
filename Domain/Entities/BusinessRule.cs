using Domain.Enums;

namespace Domain.Entities;

public class BusinessRule(
    string name,
    string description,
    BusinessRuleType type,
    decimal minValue,
    decimal? maxValue,
    string targetDepartment)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = name ?? throw new ArgumentNullException(nameof(name));

    public string Description { get; private set; } =
        description ?? throw new ArgumentNullException(nameof(description));

    public BusinessRuleType Type { get; private set; } = type;
    public decimal MinValue { get; private set; } = minValue;
    public decimal? MaxValue { get; private set; } = maxValue;

    public string TargetDepartment { get; private set; } =
        targetDepartment ?? throw new ArgumentNullException(nameof(targetDepartment));

    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

    public void Update(string name, string description, decimal minValue, decimal? maxValue, string targetDepartment)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        MinValue = minValue;
        MaxValue = maxValue;
        TargetDepartment = targetDepartment ?? throw new ArgumentNullException(nameof(targetDepartment));
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}