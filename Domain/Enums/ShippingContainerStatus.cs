namespace Domain.Enums;

/// <summary>
/// Represents the current processing status of a shipping container in the delivery system
/// </summary>
public enum ShippingContainerStatus
{
    /// <summary>
    /// Container has been received and is waiting to be processed
    /// </summary>
    Pending = 0,
    
    /// <summary>
    /// Container parcels are currently being processed by departments
    /// </summary>
    Processing = 1,
    
    /// <summary>
    /// All parcels in the container have been processed and assigned to departments
    /// </summary>
    Processed = 2,
    
    /// <summary>
    /// Container has been shipped from the distribution center
    /// </summary>
    Shipped = 3,
    
    /// <summary>
    /// Container has been delivered to its destination
    /// </summary>
    Delivered = 4,
    
    /// <summary>
    /// Container processing failed due to an error or exception
    /// </summary>
    Failed = 5
}