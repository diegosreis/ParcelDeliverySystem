using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Infrastructure.Repositories;

public class ShippingContainerRepository : InMemoryRepository<ShippingContainer>, IShippingContainerRepository
{
    private readonly Dictionary<string, Guid> _containerIdToGuid = new();

    public async Task<ShippingContainer?> GetByContainerIdAsync(string containerId)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return _containerIdToGuid.TryGetValue(containerId, out var guid) ? Entities.GetValueOrDefault(guid) : null;
        }
    }

    public async Task<IEnumerable<ShippingContainer>> GetByStatusAsync(ShippingContainerStatus status)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(c => c.Status == status)
                .ToList();
        }
    }

    public async Task<IEnumerable<ShippingContainer>> GetByShippingDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(c => c.ShippingDate >= startDate && c.ShippingDate <= endDate)
                .ToList();
        }
    }

    public override async Task<ShippingContainer> AddAsync(ShippingContainer entity)
    {
        var container = await base.AddAsync(entity);

        lock (Lock)
        {
            _containerIdToGuid[container.ContainerId] = container.Id;
        }

        return container;
    }

    public override async Task<ShippingContainer> UpdateAsync(ShippingContainer entity)
    {
        var container = await base.UpdateAsync(entity);

        lock (Lock)
        {
            _containerIdToGuid[container.ContainerId] = container.Id;
        }

        return container;
    }

    public override async Task DeleteAsync(Guid id)
    {
        lock (Lock)
        {
            if (Entities.TryGetValue(id, out var container)) _containerIdToGuid.Remove(container.ContainerId);
        }

        await base.DeleteAsync(id);
    }

    protected override Guid GetEntityId(ShippingContainer entity)
    {
        return entity.Id;
    }
}