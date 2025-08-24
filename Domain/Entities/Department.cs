using Domain.Validation;

namespace Domain.Entities;

/// <summary>
///     Represents a department responsible for handling parcels based on business rules
/// </summary>
public class Department
{
    private Department()
    {
    } // EF Core constructor

    /// <summary>
    ///     Creates a new department with the specified name and description
    /// </summary>
    /// <param name="name">Name of the department (Mail, Regular, Heavy, Insurance)</param>
    /// <param name="description">Description of the department's responsibilities</param>
    public Department(string name, string description = "")
    {
        Id = Guid.NewGuid();

        Name = Guard.Required(name, nameof(name), FieldNames.DepartmentName);
        Description = Guard.TrimOrEmpty(description);

        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Unique identifier for the department
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Name of the department (Mail, Regular, Heavy, Insurance)
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     Detailed description of the department's responsibilities
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    ///     Indicates whether the department is currently active and accepting parcels
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    ///     Timestamp when the department was created
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    ///     Timestamp when the department was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>
    ///     Updates the department's name and description
    /// </summary>
    /// <param name="name">New name for the department</param>
    /// <param name="description">New description for the department</param>
    public void Update(string name, string description)
    {
        Name = Guard.Required(name, nameof(name), FieldNames.DepartmentName);
        Description = Guard.TrimOrEmpty(description);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Activates the department to start accepting parcels
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Deactivates the department to stop accepting new parcels
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    ///     Determines whether the specified object is equal to the current department
    /// </summary>
    /// <param name="obj">The object to compare with the current department</param>
    /// <returns>True if the objects are equal; otherwise, false</returns>
    public override bool Equals(object? obj)
    {
        return obj is Department department && Id.Equals(department.Id);
    }

    /// <summary>
    ///     Returns the hash code for this department
    /// </summary>
    /// <returns>A hash code for the current department</returns>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    ///     Determines whether two departments are equal
    /// </summary>
    /// <param name="left">The first department to compare</param>
    /// <param name="right">The second department to compare</param>
    /// <returns>True if the departments are equal; otherwise, false</returns>
    public static bool operator ==(Department? left, Department? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    ///     Determines whether two departments are not equal
    /// </summary>
    /// <param name="left">The first department to compare</param>
    /// <param name="right">The second department to compare</param>
    /// <returns>True if the departments are not equal; otherwise, false</returns>
    public static bool operator !=(Department? left, Department? right)
    {
        return !(left == right);
    }
}