using Domain.Enums;
using Container = Domain.Entities.Container;

namespace Domain.Interfaces;

public interface IContainerRepository : IRepository<Container>
{
    Task<Container?> GetByContainerIdAsync(string containerId);
    Task<IEnumerable<Container>> GetByStatusAsync(ContainerStatus status);
    Task<IEnumerable<Container>> GetByShippingDateRangeAsync(DateTime startDate, DateTime endDate);
}