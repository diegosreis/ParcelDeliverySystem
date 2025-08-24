using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Infrastructure.Repositories;

public class BusinessRuleRepository : InMemoryRepository<BusinessRule>, IBusinessRuleRepository
{
    public async Task<IEnumerable<BusinessRule>> GetActiveRulesByTypeAsync(BusinessRuleType type)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(r => r.Type == type && r.IsActive)
                .ToList();
        }
    }

    public async Task<IEnumerable<BusinessRule>> GetAllActiveRulesAsync()
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .Where(r => r.IsActive)
                .ToList();
        }
    }

    public async Task<BusinessRule?> GetByNameAsync(string name)
    {
        await Task.CompletedTask;
        lock (Lock)
        {
            return Entities.Values
                .FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }

    protected override Guid GetEntityId(BusinessRule entity)
    {
        return entity.Id;
    }
}