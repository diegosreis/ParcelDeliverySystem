using Domain.Validation;

namespace Domain.Entities;

public abstract class Department
{
    private Department()
    {
    } // EF Core constructor

    protected Department(string name, string description = "")
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
        if (obj is Department other)
            return Id.Equals(other.Id);

        return false;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public static bool operator ==(Department? left, Department? right)
    {
        return EqualityComparer<Department>.Default.Equals(left, right);
    }

    public static bool operator !=(Department? left, Department? right)
    {
        return !(left == right);
    }
}