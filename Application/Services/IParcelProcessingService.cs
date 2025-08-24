using Application.DTOs;
using Domain.Enums;

namespace Application.Services;

/// <summary>
/// Service for processing parcels and managing their lifecycle
/// </summary>
public interface IParcelProcessingService
{
    /// <summary>
    /// Processes a single parcel by assigning it to appropriate departments based on business rules
    /// </summary>
    /// <param name="parcelId">Unique identifier of the parcel to process</param>
    /// <returns>Processed parcel with department assignments</returns>
    /// <exception cref="ArgumentException">Thrown when parcel is not found</exception>
    Task<ParcelDto> ProcessParcelAsync(Guid parcelId);
    
    /// <summary>
    /// Processes all parcels in a shipping container
    /// </summary>
    /// <param name="containerId">Unique identifier of the container to process</param>
    /// <returns>Collection of processed parcels with department assignments</returns>
    /// <exception cref="ArgumentException">Thrown when container is not found</exception>
    Task<IEnumerable<ParcelDto>> ProcessContainerAsync(Guid containerId);
    
    /// <summary>
    /// Manually assigns a parcel to a specific department
    /// </summary>
    /// <param name="parcelId">Unique identifier of the parcel</param>
    /// <param name="departmentId">Unique identifier of the department</param>
    /// <returns>Updated parcel with new department assignment</returns>
    /// <exception cref="ArgumentException">Thrown when parcel or department is not found</exception>
    Task<ParcelDto> AssignDepartmentAsync(Guid parcelId, Guid departmentId);
    
    /// <summary>
    /// Removes a department assignment from a parcel
    /// </summary>
    /// <param name="parcelId">Unique identifier of the parcel</param>
    /// <param name="departmentId">Unique identifier of the department to remove</param>
    /// <returns>Updated parcel without the removed department assignment</returns>
    /// <exception cref="ArgumentException">Thrown when parcel or department is not found</exception>
    Task<ParcelDto> RemoveDepartmentAsync(Guid parcelId, Guid departmentId);
    
    /// <summary>
    /// Updates the processing status of a parcel
    /// </summary>
    /// <param name="parcelId">Unique identifier of the parcel</param>
    /// <param name="status">New status for the parcel</param>
    /// <returns>Updated parcel with new status</returns>
    /// <exception cref="ArgumentException">Thrown when parcel is not found</exception>
    Task<ParcelDto> UpdateParcelStatusAsync(Guid parcelId, ParcelStatus status);
    
    /// <summary>
    /// Gets all departments currently assigned to a parcel
    /// </summary>
    /// <param name="parcelId">Unique identifier of the parcel</param>
    /// <returns>Collection of departments assigned to the parcel</returns>
    /// <exception cref="ArgumentException">Thrown when parcel is not found</exception>
    Task<IEnumerable<DepartmentDto>> GetAssignedDepartmentsAsync(Guid parcelId);
}