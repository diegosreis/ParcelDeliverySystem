using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Infrastructure.Repositories;

public class ParcelRepository : InMemoryRepository<Parcel>, IParcelRepository
{
    public async Task<IEnumerable<Parcel>> GetByStatusAsync(ParcelStatus status)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(p => p.Status == status)
                .ToList();
        }
    }

    public async Task<IEnumerable<Parcel>> GetByWeightRangeAsync(decimal minWeight, decimal maxWeight)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(p => p.Weight >= minWeight && p.Weight <= maxWeight)
                .ToList();
        }
    }

    public async Task<IEnumerable<Parcel>> GetByValueRangeAsync(decimal minValue, decimal maxValue)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(p => p.Value >= minValue && p.Value <= maxValue)
                .ToList();
        }
    }

    public async Task<IEnumerable<Parcel>> GetRequiringInsuranceAsync()
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(p => p.RequiresInsuranceApproval)
                .ToList();
        }
    }

    public async Task<IEnumerable<Parcel>> GetByContainerIdAsync(Guid containerId)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values.ToList();
        }
    }

    protected override Guid GetEntityId(Parcel entity)
    {
        return entity.Id;
    }
}