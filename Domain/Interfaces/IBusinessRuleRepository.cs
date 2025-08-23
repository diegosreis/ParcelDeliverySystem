using Domain.Entities;
using Domain.Enums;

namespace Domain.Interfaces;

public interface IBusinessRuleRepository : IRepository<BusinessRule>
{
    Task<IEnumerable<BusinessRule>> GetActiveRulesByTypeAsync(BusinessRuleType type);
    Task<IEnumerable<BusinessRule>> GetAllActiveRulesAsync();
    Task<BusinessRule?> GetByNameAsync(string name);
}