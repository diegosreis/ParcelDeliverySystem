using Domain.Enums;

namespace Application.DTOs;

public record ParcelDto(
    Guid Id,
    CustomerDto Recipient,
    decimal Weight,
    decimal Value,
    ParcelStatus Status,
    IEnumerable<DepartmentDto> AssignedDepartments,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateParcelDto(
    Guid RecipientId,
    decimal Weight,
    decimal Value
);

public record UpdateParcelDto(
    decimal Weight,
    decimal Value
);

public record ParcelAssignmentDto(
    Guid ParcelId,
    Guid DepartmentId
);