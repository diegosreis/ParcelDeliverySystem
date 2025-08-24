using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Infrastructure.Repositories;

/// <summary>
///     In-memory repository implementation for shipping container entities
/// </summary>
public class ShippingContainerRepository : InMemoryRepository<ShippingContainer>, IShippingContainerRepository
{
    /// <summary>
    ///     Initializes a new instance of the ShippingContainerRepository
    /// </summary>
    public ShippingContainerRepository()
    {
    }

    /// <inheritdoc />
    public async Task<ShippingContainer?> GetByContainerIdAsync(string containerId)
    {
        return await Task.FromResult(Entities.Values.FirstOrDefault(c => c.ContainerId == containerId));
    }

    /// <inheritdoc />
    public async Task<ShippingContainer?> GetWithParcelsAsync(Guid id)
    {
        // In this in-memory implementation, parcels are already loaded
        return await GetByIdAsync(id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ShippingContainer>> GetByStatusAsync(ShippingContainerStatus status)
    {
        return await Task.FromResult(Entities.Values.Where(c => c.Status == status).ToList());
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ShippingContainer>> GetByShippingDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await Task.FromResult(Entities.Values
            .Where(c => c.ShippingDate.Date >= startDate.Date && c.ShippingDate.Date <= endDate.Date)
            .ToList());
    }

    /// <inheritdoc />
    protected override Guid GetEntityId(ShippingContainer entity)
    {
        return entity.Id;
    }
}