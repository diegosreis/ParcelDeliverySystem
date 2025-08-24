using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
///     Service responsible for initializing default system data during application startup.
///     This service creates initial departments and business rules that can be modified later through the API.
/// </summary>
public interface IDataInitializationService
{
    Task InitializeAsync();
}

public class DataInitializationService(
    IDepartmentRepository departmentRepository,
    IBusinessRuleRepository businessRuleRepository,
    ILogger<DataInitializationService> logger) : IDataInitializationService
{
    private readonly IBusinessRuleRepository _businessRuleRepository =
        businessRuleRepository ?? throw new ArgumentNullException(nameof(businessRuleRepository));

    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    private readonly ILogger<DataInitializationService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task InitializeAsync()
    {
        await InitializeDepartmentsAsync();
        await InitializeBusinessRulesAsync();
    }

    private async Task InitializeDepartmentsAsync()
    {
        try
        {
            var existingDepartments = await _departmentRepository.GetAllAsync();
            if (existingDepartments.Any())
            {
                _logger.LogInformation("Departments already exist. Skipping initialization");
                return;
            }

            // Create initial departments - these can be modified/removed later via API
            var departments = new[]
            {
                new Department(DefaultDepartmentNames.Mail, "Department responsible for parcels up to 1kg"),
                new Department(DefaultDepartmentNames.Regular,
                    "Department responsible for parcels between 1kg and 10kg"),
                new Department(DefaultDepartmentNames.Heavy, "Department responsible for parcels over 10kg"),
                new Department(DefaultDepartmentNames.Insurance,
                    "Department responsible for high-value parcel approval")
            };

            foreach (var department in departments)
                await _departmentRepository.AddAsync(department);

            _logger.LogInformation(
                "Successfully initialized {Count} default departments. These can be modified through the API",
                departments.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize default departments");
            throw;
        }
    }

    private async Task InitializeBusinessRulesAsync()
    {
        try
        {
            var existingRules = await _businessRuleRepository.GetAllActiveRulesAsync();
            if (existingRules.Any())
            {
                _logger.LogInformation("Business rules already exist. Skipping initialization");
                return;
            }

            // Create default business rules based on requirements
            var defaultRules = new BusinessRule[]
            {
                new BusinessRule("Mail Weight Rule",
                    "Parcels with weight up to 1kg are handled by Mail department",
                    BusinessRuleType.Weight,
                    0m, 1m,
                    DefaultDepartmentNames.Mail),

                new BusinessRule("Regular Weight Rule",
                    "Parcels with weight between 1kg and 10kg are handled by Regular department",
                    BusinessRuleType.Weight,
                    1m, 10m,
                    DefaultDepartmentNames.Regular),

                new BusinessRule("Heavy Weight Rule",
                    "Parcels with weight over 10kg are handled by Heavy department",
                    BusinessRuleType.Weight,
                    10m, null,
                    DefaultDepartmentNames.Heavy),

                new BusinessRule("Insurance Value Rule",
                    "Parcels with value over €1000 require Insurance department approval",
                    BusinessRuleType.Value,
                    1000m, null,
                    DefaultDepartmentNames.Insurance)
            };

            foreach (var rule in defaultRules)
                await _businessRuleRepository.AddAsync(rule);

            _logger.LogInformation("Successfully initialized {Count} configurable business rules", defaultRules.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize default business rules");
            throw;
        }
    }
}