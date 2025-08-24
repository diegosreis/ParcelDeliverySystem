namespace Domain.Enums;

/// <summary>
/// Represents the current processing status of a parcel in the delivery system
/// </summary>
public enum ParcelStatus
{
    /// <summary>
    /// Parcel has been received and is waiting to be processed
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Parcel is currently being processed by assigned departments
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// Parcel requires insurance approval before further processing (value > €1000)
    /// </summary>
    InsuranceApprovalRequired = 2,
    
    /// <summary>
    /// Insurance has been approved for the parcel
    /// </summary>
    InsuranceApproved = 3,
    
    /// <summary>
    /// Insurance approval was rejected for the parcel
    /// </summary>
    InsuranceRejected = 4,
    
    /// <summary>
    /// Parcel has been assigned to appropriate departments for handling
    /// </summary>
    AssignedToDepartment = 5,
    
    /// <summary>
    /// Parcel has been fully processed and is ready for shipping
    /// </summary>
    Processed = 6,
    
    /// <summary>
    /// Parcel has been shipped from the distribution center
    /// </summary>
    Shipped = 7,
    
    /// <summary>
    /// Parcel has been successfully delivered to the recipient
    /// </summary>
    Delivered = 8,
    
    /// <summary>
    /// Processing failed due to an error or exception
    /// </summary>
    Failed = 9
}
