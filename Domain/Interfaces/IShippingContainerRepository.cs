using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IShippingContainerRepository : IRepository<ShippingContainer>
{
    Task<ShippingContainer?> GetByContainerIdAsync(string containerId);
    Task<IEnumerable<ShippingContainer>> GetByStatusAsync(ContainerStatus status);
    Task<IEnumerable<ShippingContainer>> GetByShippingDateRangeAsync(DateTime startDate, DateTime endDate);
}