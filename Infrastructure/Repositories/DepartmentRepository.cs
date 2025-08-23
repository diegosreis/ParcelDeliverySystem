using Domain.Entities;
using Domain.Interfaces;

namespace Infrastructure.Repositories;

public class DepartmentRepository : InMemoryRepository<Department>, IDepartmentRepository
{
    private readonly Dictionary<string, Guid> _nameToGuid = new();

    public async Task<Department?> GetByNameAsync(string name)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return _nameToGuid.TryGetValue(name, out var guid) ? Entities.GetValueOrDefault(guid) : null;
        }
    }

    public async Task<IEnumerable<Department>> GetActiveDepartmentsAsync()
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(d => d.IsActive)
                .ToList();
        }
    }

    public async Task<IEnumerable<Department>> GetInactiveDepartmentsAsync()
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(d => !d.IsActive)
                .ToList();
        }
    }

    public override async Task<Department> AddAsync(Department entity)
    {
        var department = await base.AddAsync(entity);

        lock (Lock)
        {
            _nameToGuid[department.Name] = department.Id;
        }

        return department;
    }

    public override async Task<Department> UpdateAsync(Department entity)
    {
        var department = await base.UpdateAsync(entity);

        lock (Lock)
        {
            _nameToGuid[department.Name] = department.Id;
        }

        return department;
    }

    protected override Guid GetEntityId(Department entity)
    {
        return entity.Id;
    }
}