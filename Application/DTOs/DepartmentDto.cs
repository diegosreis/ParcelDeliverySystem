namespace Application.DTOs;

/// <summary>
/// Represents a department that handles parcel processing
/// </summary>
/// <param name="Id">Unique identifier for the department</param>
/// <param name="Name">Name of the department (Mail, Regular, Heavy, Insurance)</param>
/// <param name="Description">Detailed description of the department's responsibilities</param>
/// <param name="IsActive">Indicates if the department is currently active</param>
/// <param name="CreatedAt">Timestamp when the department was created</param>
/// <param name="UpdatedAt">Timestamp when the department was last updated</param>
public record DepartmentDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// Data transfer object for creating a new department
/// </summary>
/// <param name="Name">Name of the department</param>
/// <param name="Description">Description of the department's responsibilities</param>
public record CreateDepartmentDto(
    string Name,
    string Description = ""
);

/// <summary>
/// Data transfer object for updating an existing department
/// </summary>
/// <param name="Name">Updated department name</param>
/// <param name="Description">Updated department description</param>
public record UpdateDepartmentDto(
    string Name,
    string Description
);