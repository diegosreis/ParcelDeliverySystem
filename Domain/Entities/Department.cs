using Domain.Validation;

namespace Domain.Entities;

public class Department
{
    private Department()
    {
    } // EF Core constructor

    public Department(string name, string description = "")
    {
        Id = Guid.NewGuid();

        Name = Guard.Required(name, nameof(name), FieldNames.DepartmentName);
        Description = Guard.TrimOrEmpty(description);

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public void Update(string name, string description)
    {
        Name = Guard.Required(name, nameof(name), FieldNames.DepartmentName);
        Description = Guard.TrimOrEmpty(description);

        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }


    public override bool Equals(object? obj)
    {
        return obj is Department other && Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Department? left, Department? right)
    {
        return ReferenceEquals(left, right) || (left is not null && right is not null && left.Id == right.Id);
    }

    public static bool operator !=(Department? left, Department? right)
    {
        return !(left == right);
    }
}