using Domain.Enums;
using Domain.Validation;

namespace Domain.Entities;

/// <summary>
/// Represents a parcel in the delivery system with recipient information and department assignments
/// </summary>
public class Parcel
{
    private readonly List<Department> _assignedDepartments = new();

    private Parcel()
    {
    } // EF Core constructor

    /// <summary>
    /// Creates a new parcel with the specified recipient, weight, and value
    /// </summary>
    /// <param name="recipient">Customer who will receive the parcel</param>
    /// <param name="weight">Weight of the parcel in kilograms</param>
    /// <param name="value">Monetary value of the parcel contents</param>
    /// <exception cref="ArgumentNullException">Thrown when recipient is null</exception>
    /// <exception cref="ArgumentException">Thrown when weight is zero or negative, or value is negative</exception>
    public Parcel(Customer recipient, decimal weight, decimal value)
    {
        Id = Guid.NewGuid();
        Recipient = Guard.NotNull(recipient, nameof(recipient), FieldNames.Recipient);
        Weight = Guard.GreaterThan(weight, 0, nameof(weight), FieldNames.Weight);
        Value = Guard.NotNegative(value, nameof(value), FieldNames.Value);
        Status = ParcelStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unique identifier for the parcel
    /// </summary>
    public Guid Id { get; private set; }
    
    /// <summary>
    /// Customer who will receive the parcel
    /// </summary>
    public Customer Recipient { get; private set; } = null!;
    
    /// <summary>
    /// Weight of the parcel in kilograms
    /// </summary>
    public decimal Weight { get; private set; }
    
    /// <summary>
    /// Monetary value of the parcel contents
    /// </summary>
    public decimal Value { get; private set; }
    
    /// <summary>
    /// Current processing status of the parcel
    /// </summary>
    public ParcelStatus Status { get; private set; }
    
    /// <summary>
    /// Collection of departments assigned to handle this parcel
    /// </summary>
    public IReadOnlyList<Department> AssignedDepartments => _assignedDepartments.AsReadOnly();
    
    /// <summary>
    /// Timestamp when the parcel was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }
    
    /// <summary>
    /// Timestamp when the parcel was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    /// Indicates whether the parcel requires insurance approval (value > €1000)
    /// </summary>
    public bool RequiresInsuranceApproval => Value > 1000;

    /// <summary>
    /// Indicates whether the parcel is considered heavy (weight > 10kg)
    /// </summary>
    public bool IsHeavyParcel => Weight > 10;

    /// <summary>
    /// Indicates whether the parcel is in the regular weight category (1kg < weight ≤ 10kg)
    /// </summary>
    public bool IsRegularParcel => Weight > 1 && Weight <= 10;

    /// <summary>
    /// Indicates whether the parcel is in the mail category (weight ≤ 1kg)
    /// </summary>
    public bool IsMailParcel => Weight <= 1;

    /// <summary>
    /// Updates the parcel's weight and value
    /// </summary>
    /// <param name="weight">New weight in kilograms</param>
    /// <param name="value">New monetary value</param>
    /// <exception cref="ArgumentException">Thrown when weight is zero or negative, or value is negative</exception>
    public void Update(decimal weight, decimal value)
    {
        Weight = Guard.GreaterThan(weight, 0, nameof(weight), FieldNames.Weight);
        Value = Guard.NotNegative(value, nameof(value), FieldNames.Value);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns a department to handle this parcel
    /// </summary>
    /// <param name="department">Department to assign to the parcel</param>
    /// <exception cref="ArgumentNullException">Thrown when department is null</exception>
    public void AssignDepartment(Department department)
    {
        Guard.NotNull(department, nameof(department), FieldNames.Department);

        if (!_assignedDepartments.Any(d => d.Id == department.Id))
        {
            _assignedDepartments.Add(department);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Removes a department assignment from this parcel
    /// </summary>
    /// <param name="department">Department to remove from the parcel</param>
    /// <exception cref="ArgumentNullException">Thrown when department is null</exception>
    public void RemoveDepartment(Department department)
    {
        Guard.NotNull(department, nameof(department), FieldNames.Department);
        var existingDepartment = _assignedDepartments.FirstOrDefault(d => d.Id == department.Id);
        if (existingDepartment != null)
        {
            _assignedDepartments.Remove(existingDepartment);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Updates the processing status of the parcel
    /// </summary>
    /// <param name="status">New status for the parcel</param>
    public void UpdateStatus(ParcelStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}