using Domain.Entities;

namespace Domain.Interfaces;

/// <summary>
///     Repository interface for managing departments in the parcel delivery system
/// </summary>
public interface IDepartmentRepository : IRepository<Department>
{
    /// <summary>
    ///     Retrieves a department by its name
    /// </summary>
    /// <param name="name">The name of the department to find</param>
    /// <returns>The department with the specified name, or null if not found</returns>
    Task<Department?> GetByNameAsync(string name);

    /// <summary>
    ///     Retrieves all active departments
    /// </summary>
    /// <returns>A collection of all active departments</returns>
    Task<IEnumerable<Department>> GetActiveDepartmentsAsync();

    /// <summary>
    ///     Retrieves all inactive departments
    /// </summary>
    /// <returns>A collection of all inactive departments</returns>
    Task<IEnumerable<Department>> GetInactiveDepartmentsAsync();
}