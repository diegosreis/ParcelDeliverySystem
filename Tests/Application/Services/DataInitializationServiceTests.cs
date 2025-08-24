using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class DataInitializationServiceTests
{
    private readonly Mock<IBusinessRuleRepository> _mockBusinessRuleRepository;
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly Mock<ILogger<DataInitializationService>> _mockLogger;
    private readonly DataInitializationService _service;

    public DataInitializationServiceTests()
    {
        _mockDepartmentRepository = new Mock<IDepartmentRepository>();
        _mockBusinessRuleRepository = new Mock<IBusinessRuleRepository>();
        _mockLogger = new Mock<ILogger<DataInitializationService>>();

        _service = new DataInitializationService(
            _mockDepartmentRepository.Object,
            _mockBusinessRuleRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task InitializeAsync_WithValidDependencies_ShouldCompleteSuccessfully()
    {
        // Arrange
        _mockDepartmentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Department>());
        _mockBusinessRuleRepository.Setup(r => r.GetAllActiveRulesAsync())
            .ReturnsAsync(new List<BusinessRule>());

        // Act
        await _service.InitializeAsync();

        // Assert
        _mockDepartmentRepository.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Exactly(4));
        _mockBusinessRuleRepository.Verify(r => r.AddAsync(It.IsAny<BusinessRule>()), Times.Exactly(4));
    }

    [Fact]
    public async Task InitializeAsync_WhenDepartmentsExist_ShouldSkipDepartmentInitialization()
    {
        // Arrange
        _mockDepartmentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Department> { new("Test", "Test") });
        _mockBusinessRuleRepository.Setup(r => r.GetAllActiveRulesAsync())
            .ReturnsAsync(new List<BusinessRule>());

        // Act
        await _service.InitializeAsync();

        // Assert
        _mockDepartmentRepository.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_WhenBusinessRulesExist_ShouldSkipBusinessRuleInitialization()
    {
        // Arrange
        _mockDepartmentRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Department>());
        _mockBusinessRuleRepository.Setup(r => r.GetAllActiveRulesAsync())
            .ReturnsAsync(new List<BusinessRule>
            {
                new("Test", "Test", BusinessRuleType.Weight, 0, 1, "Test")
            });

        // Act
        await _service.InitializeAsync();

        // Assert
        _mockBusinessRuleRepository.Verify(r => r.AddAsync(It.IsAny<BusinessRule>()), Times.Never);
    }

    [Fact]
    public async Task InitializeAsync_WhenDepartmentRepositoryFails_ShouldThrowException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database error");
        _mockDepartmentRepository.Setup(r => r.GetAllAsync())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.InitializeAsync());

        Assert.Equal(expectedException.Message, actualException.Message);
    }
}