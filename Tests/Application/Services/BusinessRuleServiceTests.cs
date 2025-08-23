using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Moq;

namespace Tests.Application.Services;

public class BusinessRuleServiceTests
{
    private readonly Mock<IBusinessRuleRepository> _mockBusinessRuleRepository;
    private readonly BusinessRuleService _service;

    public BusinessRuleServiceTests()
    {
        _mockBusinessRuleRepository = new Mock<IBusinessRuleRepository>();
        _service = new BusinessRuleService(_mockBusinessRuleRepository.Object);
    }

    [Fact]
    public async Task CreateRuleAsync_WithValidData_ShouldCreateBusinessRule()
    {
        // Arrange
        var rule = new BusinessRule("Mail Rule", "Handle mail parcels", BusinessRuleType.Weight, 0, 1, "Mail");
        _mockBusinessRuleRepository.Setup(r => r.AddAsync(It.IsAny<BusinessRule>()))
            .ReturnsAsync(rule);

        // Act
        var result =
            await _service.CreateRuleAsync("Mail Rule", "Handle mail parcels", BusinessRuleType.Weight, 0, 1, "Mail");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mail Rule", result.Name);
        Assert.Equal(BusinessRuleType.Weight, result.Type);
        Assert.Equal("Mail", result.TargetDepartment);
    }

    [Fact]
    public async Task GetRulesByTypeAsync_WithWeightType_ShouldReturnWeightRules()
    {
        // Arrange
        var weightRules = new List<BusinessRule>
        {
            new("Mail Rule", "Handle mail parcels", BusinessRuleType.Weight, 0, 1, "Mail"),
            new("Regular Rule", "Handle regular parcels", BusinessRuleType.Weight, 1.01m, 10, "Regular")
        };

        _mockBusinessRuleRepository.Setup(r => r.GetActiveRulesByTypeAsync(BusinessRuleType.Weight))
            .ReturnsAsync(weightRules);

        // Act
        var result = await _service.GetRulesByTypeAsync(BusinessRuleType.Weight);

        // Assert
        var rules = result.ToList();
        Assert.Equal(2, rules.Count);
        Assert.All(rules, r => Assert.Equal(BusinessRuleType.Weight, r.Type));
    }

    [Fact]
    public async Task ActivateRuleAsync_WithValidId_ShouldActivateRule()
    {
        // Arrange
        var rule = new BusinessRule("Test Rule", "Test Description", BusinessRuleType.Weight, 0, 1, "Mail");
        rule.Deactivate(); // Deactivate first
        _mockBusinessRuleRepository.Setup(r => r.GetByIdAsync(rule.Id))
            .ReturnsAsync(rule);

        // Act
        await _service.ActivateRuleAsync(rule.Id);

        // Assert
        _mockBusinessRuleRepository.Verify(r => r.UpdateAsync(rule), Times.Once);
        Assert.True(rule.IsActive);
    }

    [Fact]
    public async Task DeactivateRuleAsync_WithValidId_ShouldDeactivateRule()
    {
        // Arrange
        var rule = new BusinessRule("Test Rule", "Test Description", BusinessRuleType.Weight, 0, 1, "Mail");
        _mockBusinessRuleRepository.Setup(r => r.GetByIdAsync(rule.Id))
            .ReturnsAsync(rule);

        // Act
        await _service.DeactivateRuleAsync(rule.Id);

        // Assert
        _mockBusinessRuleRepository.Verify(r => r.UpdateAsync(rule), Times.Once);
        Assert.False(rule.IsActive);
    }

    [Fact]
    public async Task DeleteRuleAsync_WithNonExistentRule_ShouldThrowArgumentException()
    {
        // Arrange
        var ruleId = Guid.NewGuid();
        _mockBusinessRuleRepository.Setup(r => r.GetByIdAsync(ruleId))
            .ReturnsAsync((BusinessRule?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _service.DeleteRuleAsync(ruleId));
    }

    [Fact]
    public async Task DeleteRuleAsync_WithValidId_ShouldDeleteRule()
    {
        // Arrange
        var rule = new BusinessRule("Test Rule", "Test Description", BusinessRuleType.Weight, 0, 1, "Mail");
        _mockBusinessRuleRepository.Setup(r => r.GetByIdAsync(rule.Id))
            .ReturnsAsync(rule);
        _mockBusinessRuleRepository.Setup(r => r.DeleteAsync(rule.Id))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteRuleAsync(rule.Id);

        // Assert
        _mockBusinessRuleRepository.Verify(r => r.GetByIdAsync(rule.Id), Times.Once);
        _mockBusinessRuleRepository.Verify(r => r.DeleteAsync(rule.Id), Times.Once);
    }

    [Fact]
    public async Task UpdateRuleAsync_WithValidData_ShouldUpdateRule()
    {
        // Arrange
        var rule = new BusinessRule("Original Rule", "Original Description", BusinessRuleType.Weight, 0, 1, "Mail");
        var updatedRule =
            new BusinessRule("Updated Rule", "Updated Description", BusinessRuleType.Weight, 0, 2, "Regular");

        _mockBusinessRuleRepository.Setup(r => r.GetByIdAsync(rule.Id))
            .ReturnsAsync(rule);
        _mockBusinessRuleRepository.Setup(r => r.UpdateAsync(It.IsAny<BusinessRule>()))
            .ReturnsAsync(updatedRule);

        // Act
        var result = await _service.UpdateRuleAsync(rule.Id, "Updated Rule", "Updated Description", 0, 2, "Regular");

        // Assert
        Assert.NotNull(result);
        _mockBusinessRuleRepository.Verify(r => r.GetByIdAsync(rule.Id), Times.Once);
        _mockBusinessRuleRepository.Verify(r => r.UpdateAsync(rule), Times.Once);
    }

    [Fact]
    public async Task UpdateRuleAsync_WithNonExistentRule_ShouldThrowArgumentException()
    {
        // Arrange
        var ruleId = Guid.NewGuid();
        _mockBusinessRuleRepository.Setup(r => r.GetByIdAsync(ruleId))
            .ReturnsAsync((BusinessRule?)null);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateRuleAsync(ruleId, "Test", "Test", 0, 1, "Mail"));
    }
}