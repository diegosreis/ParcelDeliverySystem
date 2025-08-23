using Application.DTOs;
using Domain.Enums;

namespace Application.Services;

public interface IParcelProcessingService
{
    Task<ParcelDto> ProcessParcelAsync(Guid parcelId);
    Task<IEnumerable<ParcelDto>> ProcessContainerAsync(Guid containerId);
    Task<ParcelDto> AssignDepartmentAsync(Guid parcelId, Guid departmentId);
    Task<ParcelDto> RemoveDepartmentAsync(Guid parcelId, Guid departmentId);
    Task<ParcelDto> UpdateParcelStatusAsync(Guid parcelId, ParcelStatus status);
    Task<IEnumerable<DepartmentDto>> GetAssignedDepartmentsAsync(Guid parcelId);
}