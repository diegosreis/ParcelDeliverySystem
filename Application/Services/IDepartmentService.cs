using Application.DTOs;

namespace Application.Services;

/// <summary>
///     Service interface for managing departments in the parcel delivery system.
///     Provides business logic for department operations while maintaining separation of concerns.
/// </summary>
public interface IDepartmentService
{
    /// <summary>
    ///     Retrieves all departments in the system
    /// </summary>
    /// <returns>A collection of all departments</returns>
    Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();

    /// <summary>
    ///     Retrieves all active departments
    /// </summary>
    /// <returns>A collection of active departments</returns>
    Task<IEnumerable<DepartmentDto>> GetActiveDepartmentsAsync();

    /// <summary>
    ///     Retrieves all inactive departments
    /// </summary>
    /// <returns>A collection of inactive departments</returns>
    Task<IEnumerable<DepartmentDto>> GetInactiveDepartmentsAsync();

    /// <summary>
    ///     Retrieves a specific department by ID
    /// </summary>
    /// <param name="id">The unique identifier of the department</param>
    /// <returns>The department with the specified ID, or null if not found</returns>
    Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id);

    /// <summary>
    ///     Retrieves a department by name
    /// </summary>
    /// <param name="name">The name of the department to find</param>
    /// <returns>The department with the specified name, or null if not found</returns>
    Task<DepartmentDto?> GetDepartmentByNameAsync(string name);

    /// <summary>
    ///     Creates a new department in the system
    /// </summary>
    /// <param name="createDepartmentDto">The department data to create</param>
    /// <returns>The created department</returns>
    /// <exception cref="InvalidOperationException">Thrown when department with same name already exists</exception>
    Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto);

    /// <summary>
    ///     Updates an existing department
    /// </summary>
    /// <param name="id">The unique identifier of the department to update</param>
    /// <param name="updateDepartmentDto">The updated department data</param>
    /// <returns>The updated department</returns>
    /// <exception cref="InvalidOperationException">Thrown when department not found or name conflict exists</exception>
    Task<DepartmentDto> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto updateDepartmentDto);

    /// <summary>
    ///     Activates a department (makes it available for parcel processing)
    /// </summary>
    /// <param name="id">The unique identifier of the department to activate</param>
    /// <returns>The activated department</returns>
    /// <exception cref="InvalidOperationException">Thrown when department not found</exception>
    Task<DepartmentDto> ActivateDepartmentAsync(Guid id);

    /// <summary>
    ///     Deactivates a department (removes it from parcel processing without deleting)
    /// </summary>
    /// <param name="id">The unique identifier of the department to deactivate</param>
    /// <returns>The deactivated department</returns>
    /// <exception cref="InvalidOperationException">Thrown when department not found</exception>
    Task<DepartmentDto> DeactivateDepartmentAsync(Guid id);

    /// <summary>
    ///     Deletes a department from the system
    /// </summary>
    /// <param name="id">The unique identifier of the department to delete</param>
    /// <exception cref="InvalidOperationException">Thrown when department not found</exception>
    Task DeleteDepartmentAsync(Guid id);
}
