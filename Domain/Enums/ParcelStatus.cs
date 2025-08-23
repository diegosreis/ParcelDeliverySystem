namespace Domain.Enums;

public enum ParcelStatus
{
    Pending = 0,
    Processing = 1,
    InsuranceApprovalRequired = 2,
    InsuranceApproved = 3,
    InsuranceRejected = 4,
    AssignedToDepartment = 5,
    Processed = 6,
    Shipped = 7,
    Delivered = 8,
    Failed = 9
}
