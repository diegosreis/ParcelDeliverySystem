namespace Domain.Enums;

/// <summary>
/// Represents the type of business rule used for parcel routing and department assignment
/// </summary>
public enum BusinessRuleType
{
    /// <summary>
    /// Rule based on parcel weight (Mail: ≤1kg, Regular: ≤10kg, Heavy: >10kg)
    /// </summary>
    Weight = 1,
    
    /// <summary>
    /// Rule based on parcel value (Insurance required for value > €1000)
    /// </summary>
    Value = 2
}