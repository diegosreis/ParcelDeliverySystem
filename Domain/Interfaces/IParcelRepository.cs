using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IParcelRepository : IRepository<Parcel>
{
    Task<IEnumerable<Parcel>> GetByStatusAsync(ParcelStatus status);
    Task<IEnumerable<Parcel>> GetByWeightRangeAsync(decimal minWeight, decimal maxWeight);
    Task<IEnumerable<Parcel>> GetByValueRangeAsync(decimal minValue, decimal maxValue);
    Task<IEnumerable<Parcel>> GetRequiringInsuranceAsync();
    Task<IEnumerable<Parcel>> GetByContainerIdAsync(Guid containerId);
}