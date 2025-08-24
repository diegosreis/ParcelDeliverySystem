using Domain.Enums;

namespace Application.DTOs;

/// <summary>
/// Represents a parcel with detailed information including recipient and department assignments
/// </summary>
/// <param name="Id">Unique identifier for the parcel</param>
/// <param name="Recipient">Customer information who will receive the parcel</param>
/// <param name="Weight">Weight of the parcel in kilograms</param>
/// <param name="Value">Monetary value of the parcel contents</param>
/// <param name="Status">Current processing status of the parcel</param>
/// <param name="AssignedDepartments">Departments responsible for handling this parcel</param>
/// <param name="CreatedAt">Timestamp when the parcel was created</param>
/// <param name="UpdatedAt">Timestamp when the parcel was last updated</param>
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

/// <summary>
/// Data transfer object for creating a new parcel
/// </summary>
/// <param name="RecipientId">Unique identifier of the recipient customer</param>
/// <param name="Weight">Weight of the parcel in kilograms</param>
/// <param name="Value">Monetary value of the parcel contents</param>
public record CreateParcelDto(
    Guid RecipientId,
    decimal Weight,
    decimal Value
);

/// <summary>
/// Data transfer object for updating an existing parcel
/// </summary>
/// <param name="Weight">Updated weight of the parcel in kilograms</param>
/// <param name="Value">Updated monetary value of the parcel contents</param>
public record UpdateParcelDto(
    decimal Weight,
    decimal Value
);

/// <summary>
/// Data transfer object for assigning a parcel to a department
/// </summary>
/// <param name="ParcelId">Unique identifier of the parcel</param>
/// <param name="DepartmentId">Unique identifier of the department</param>
public record ParcelAssignmentDto(
    Guid ParcelId,
    Guid DepartmentId
);