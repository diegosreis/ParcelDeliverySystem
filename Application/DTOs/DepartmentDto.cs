namespace Application.DTOs;

public record DepartmentDto(
    Guid Id,
    string Name,
    string Description,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateDepartmentDto(
    string Name,
    string Description = ""
);

public record UpdateDepartmentDto(
    string Name,
    string Description
);