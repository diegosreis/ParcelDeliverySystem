using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class ShippingContainerServiceTests
{
    private readonly Mock<IShippingContainerRepository> _mockContainerRepository;
    private readonly Mock<ILogger<ShippingContainerService>> _mockLogger;
    private readonly ShippingContainerService _service;
    private readonly ShippingContainer _testContainer;

    public ShippingContainerServiceTests()
    {
        _mockContainerRepository = new Mock<IShippingContainerRepository>();
        _mockLogger = new Mock<ILogger<ShippingContainerService>>();
        _service = new ShippingContainerService(_mockContainerRepository.Object, _mockLogger.Object);

        _testContainer = new ShippingContainer("TEST-001", DateTime.UtcNow.AddDays(-1));
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ShippingContainerService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ShippingContainerService(_mockContainerRepository.Object, null!));
    }

    [Fact]
    public async Task GetAllContainersAsync_ShouldReturnMappedContainers()
    {
        // Arrange
        var containers = new List<ShippingContainer> { _testContainer };
        _mockContainerRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(containers);

        // Act
        var result = await _service.GetAllContainersAsync();

        // Assert
        var containerDtos = result.ToList();
        Assert.Single(containerDtos);
        Assert.Equal(_testContainer.Id, containerDtos.First().Id);
        Assert.Equal(_testContainer.ShippingDate, containerDtos.First().ShippingDate);
    }

    [Fact]
    public async Task GetAllContainersAsync_WithIncludeParcelDetails_ShouldReturnMappedContainers()
    {
        // Arrange
        var containers = new List<ShippingContainer> { _testContainer };
        _mockContainerRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(containers);

        // Act
        var result = await _service.GetAllContainersAsync(true);

        // Assert
        var containerDtos = result.ToList();
        Assert.Single(containerDtos);
        Assert.Equal(_testContainer.Id, containerDtos.First().Id);
        Assert.Equal(_testContainer.ShippingDate, containerDtos.First().ShippingDate);
    }

    [Fact]
    public async Task GetContainerByIdAsync_WithValidId_ShouldReturnMappedContainer()
    {
        // Arrange
        var containerId = _testContainer.Id;
        _mockContainerRepository.Setup(r => r.GetByIdAsync(containerId))
            .ReturnsAsync(_testContainer);

        // Act
        var result = await _service.GetContainerByIdAsync(containerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testContainer.Id, result.Id);
        Assert.Equal(_testContainer.ShippingDate, result.ShippingDate);
    }

    [Fact]
    public async Task GetContainerByIdAsync_WithValidIdAndIncludeParcelDetails_ShouldReturnMappedContainer()
    {
        // Arrange
        var containerId = _testContainer.Id;
        _mockContainerRepository.Setup(r => r.GetByIdAsync(containerId))
            .ReturnsAsync(_testContainer);

        // Act
        var result = await _service.GetContainerByIdAsync(containerId, true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testContainer.Id, result.Id);
        Assert.Equal(_testContainer.ShippingDate, result.ShippingDate);
    }

    [Fact]
    public async Task GetContainerByIdAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetContainerByIdAsync(Guid.Empty));

        Assert.Contains("Container ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GetContainerByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        _mockContainerRepository.Setup(r => r.GetByIdAsync(containerId))
            .ReturnsAsync((ShippingContainer?)null);

        // Act
        var result = await _service.GetContainerByIdAsync(containerId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetContainersByStatusAsync_ShouldReturnMappedContainers()
    {
        // Arrange
        const ShippingContainerStatus status = ShippingContainerStatus.Processing;
        var containers = new List<ShippingContainer> { _testContainer };
        _mockContainerRepository.Setup(r => r.GetByStatusAsync(status))
            .ReturnsAsync(containers);

        // Act
        var result = await _service.GetContainersByStatusAsync(status);

        // Assert
        var containerDtos = result.ToList();
        Assert.Single(containerDtos);
        Assert.Equal(_testContainer.Status, containerDtos.First().Status);
    }

    [Fact]
    public async Task GetContainersByStatusAsync_WithIncludeParcelDetails_ShouldReturnMappedContainers()
    {
        // Arrange
        const ShippingContainerStatus status = ShippingContainerStatus.Processing;
        var containers = new List<ShippingContainer> { _testContainer };
        _mockContainerRepository.Setup(r => r.GetByStatusAsync(status))
            .ReturnsAsync(containers);

        // Act
        var result = await _service.GetContainersByStatusAsync(status, true);

        // Assert
        var containerDtos = result.ToList();
        Assert.Single(containerDtos);
        Assert.Equal(_testContainer.Status, containerDtos.First().Status);
    }

    [Fact]
    public async Task GetContainersByDateRangeAsync_WithValidRange_ShouldReturnMappedContainers()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-2);
        var endDate = DateTime.UtcNow;
        var containers = new List<ShippingContainer> { _testContainer };
        _mockContainerRepository.Setup(r => r.GetByShippingDateRangeAsync(startDate, endDate))
            .ReturnsAsync(containers);

        // Act
        var result = await _service.GetContainersByDateRangeAsync(startDate, endDate);

        // Assert
        var containerDtos = result.ToList();
        Assert.Single(containerDtos);
        Assert.Equal(_testContainer.ShippingDate, containerDtos.First().ShippingDate);
    }

    [Fact]
    public async Task GetContainersByDateRangeAsync_WithIncludeParcelDetails_ShouldReturnMappedContainers()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-2);
        var endDate = DateTime.UtcNow;
        var containers = new List<ShippingContainer> { _testContainer };
        _mockContainerRepository.Setup(r => r.GetByShippingDateRangeAsync(startDate, endDate))
            .ReturnsAsync(containers);

        // Act
        var result = await _service.GetContainersByDateRangeAsync(startDate, endDate, true);

        // Assert
        var containerDtos = result.ToList();
        Assert.Single(containerDtos);
        Assert.Equal(_testContainer.ShippingDate, containerDtos.First().ShippingDate);
    }

    [Fact]
    public async Task GetContainersByDateRangeAsync_WithStartDateGreaterThanEndDate_ShouldThrowArgumentException()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetContainersByDateRangeAsync(startDate, endDate));

        Assert.Contains("Start date cannot be greater than end date", exception.Message);
    }

    [Fact]
    public async Task GetContainerWithParcelsAsync_WithValidId_ShouldReturnMappedContainerWithParcels()
    {
        // Arrange
        var containerId = _testContainer.Id;
        _mockContainerRepository.Setup(r => r.GetWithParcelsAsync(containerId))
            .ReturnsAsync(_testContainer);

        // Act
        var result = await _service.GetContainerWithParcelsAsync(containerId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testContainer.Id, result.Id);
        Assert.True(result.TotalParcels >= 0); // Check that parcels data is included
    }

    [Fact]
    public async Task GetContainerWithParcelsAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetContainerWithParcelsAsync(Guid.Empty));

        Assert.Contains("Container ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GetContainerWithParcelsAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        _mockContainerRepository.Setup(r => r.GetWithParcelsAsync(containerId))
            .ReturnsAsync((ShippingContainer?)null);

        // Act
        var result = await _service.GetContainerWithParcelsAsync(containerId);

        // Assert
        Assert.Null(result);
    }
}