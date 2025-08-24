namespace Domain.Interfaces;

/// <summary>
/// Generic repository interface providing basic CRUD operations for entities
/// </summary>
/// <typeparam name="T">The type of entity this repository manages</typeparam>
public interface IRepository<T>
{
    /// <summary>
    /// Retrieves an entity by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity</param>
    /// <returns>The entity if found, null otherwise</returns>
    Task<T?> GetByIdAsync(Guid id);
    
    /// <summary>
    /// Retrieves all entities from the repository
    /// </summary>
    /// <returns>Collection of all entities</returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The added entity with any generated values</returns>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <returns>The updated entity</returns>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Deletes an entity from the repository by its identifier
    /// </summary>
    /// <param name="id">The unique identifier of the entity to delete</param>
    Task DeleteAsync(Guid id);
    
    /// <summary>
    /// Checks if an entity exists in the repository
    /// </summary>
    /// <param name="id">The unique identifier of the entity to check</param>
    /// <returns>True if the entity exists, false otherwise</returns>
    Task<bool> ExistsAsync(Guid id);
}