using Domain.Enums;
using Domain.Validation;

namespace Domain.Entities;

public class Parcel
{
    private Parcel()
    {
        AssignedDepartments = [];
    } // EF Core constructor

    public Parcel(Customer recipient, decimal weight, decimal value)
    {
        Id = Guid.NewGuid();

        Recipient = Guard.NotNull(recipient, nameof(recipient), FieldNames.Recipient);
        Weight = Guard.GreaterThan(weight, 0, nameof(weight), FieldNames.Weight);
        Value = Guard.NotNegative(value, nameof(value), FieldNames.Value);

        Status = ParcelStatus.Pending;
        AssignedDepartments = [];
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Customer Recipient { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public decimal Value { get; private set; }
    public ParcelStatus Status { get; private set; }
    public List<Department> AssignedDepartments { get; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public bool RequiresInsuranceApproval => Value > 1000m;

    public bool IsHeavyParcel => Weight > 10m;

    public bool IsRegularParcel => Weight is > 1m and <= 10m;

    public bool IsMailParcel => Weight <= 1m;

    public void Update(decimal weight, decimal value)
    {
        Weight = Guard.GreaterThan(weight, 0, nameof(weight), FieldNames.Weight);
        Value = Guard.NotNegative(value, nameof(value), FieldNames.Value);

        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignDepartment(Department department)
    {
        var dep = Guard.NotNull(department, nameof(department), FieldNames.Department);

        if (AssignedDepartments.Contains(dep)) return;

        AssignedDepartments.Add(dep);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveDepartment(Department department)
    {
        var dep = Guard.NotNull(department, nameof(department), FieldNames.Department);

        if (!AssignedDepartments.Contains(dep)) return;
        AssignedDepartments.Remove(dep);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStatus(ParcelStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}