using Domain.Interfaces;

namespace Infrastructure.Repositories;

public abstract class InMemoryRepository<T> : IRepository<T> where T : class
{
    protected readonly Dictionary<Guid, T> Entities = new();
    protected readonly object Lock = new();

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.GetValueOrDefault(id);
        }
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values.ToList();
        }
    }

    public virtual async Task<T> AddAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Task.CompletedTask;
        lock (Lock)
        {
            var id = GetEntityId(entity);
            return !Entities.TryAdd(id, entity)
                ? throw new InvalidOperationException($"Entity with ID {id} already exists")
                : entity;
        }
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        await Task.CompletedTask;
        lock (Lock)
        {
            var id = GetEntityId(entity);
            if (!Entities.ContainsKey(id))
                throw new InvalidOperationException($"Entity with ID {id} not found");

            Entities[id] = entity;
            return entity;
        }
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            if (!Entities.Remove(id))
                throw new InvalidOperationException($"Entity with ID {id} not found");
        }
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.ContainsKey(id);
        }
    }

    protected abstract Guid GetEntityId(T entity);
}