namespace Application.DTOs;

/// <summary>
///     Data transfer object for creating a new department
/// </summary>
/// <param name="Name">The name of the department (required)</param>
/// <param name="Description">Optional description of the department's responsibilities</param>
public record CreateDepartmentDto(
    string Name,
    string? Description = null
);

/// <summary>
///     Data transfer object for updating an existing department
/// </summary>
/// <param name="Name">The updated name of the department (required)</param>
/// <param name="Description">Updated description of the department's responsibilities</param>
public record UpdateDepartmentDto(
    string Name,
    string? Description = null
);