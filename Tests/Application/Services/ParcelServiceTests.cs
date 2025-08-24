using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class ParcelServiceTests
{
    private readonly ParcelService _service;
    private readonly Mock<IParcelRepository> _mockParcelRepository;
    private readonly Mock<ILogger<ParcelService>> _mockLogger;
    private readonly Parcel _testParcel;

    public ParcelServiceTests()
    {
        _mockParcelRepository = new Mock<IParcelRepository>();
        _mockLogger = new Mock<ILogger<ParcelService>>();
        _service = new ParcelService(_mockParcelRepository.Object, _mockLogger.Object);

        var testAddress = new Address(
            "Marijkestraat",
            "28",
            "",
            "Center",
            "Bosschenhoofd",
            "NB",
            "4744AT",
            "Netherlands"
        );
        var testCustomer = new Customer("João Silva", testAddress);
        _testParcel = new Parcel(testCustomer, 0.5m, 50m);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ParcelService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new ParcelService(_mockParcelRepository.Object, null!));
    }

    [Fact]
    public async Task GetAllParcelsAsync_ShouldReturnMappedParcels()
    {
        // Arrange
        var parcels = new List<Parcel> { _testParcel };
        _mockParcelRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(parcels);

        // Act
        var result = await _service.GetAllParcelsAsync();

        // Assert
        var parcelDtos = result.ToList();
        Assert.Single(parcelDtos);
        Assert.Equal(_testParcel.Weight, parcelDtos.First().Weight);
        Assert.Equal(_testParcel.Value, parcelDtos.First().Value);
    }

    [Fact]
    public async Task GetParcelByIdAsync_WithValidId_ShouldReturnMappedParcel()
    {
        // Arrange
        var parcelId = _testParcel.Id;
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync(_testParcel);

        // Act
        var result = await _service.GetParcelByIdAsync(parcelId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(_testParcel.Weight, result.Weight);
        Assert.Equal(_testParcel.Id, result.Id);
    }

    [Fact]
    public async Task GetParcelByIdAsync_WithEmptyId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetParcelByIdAsync(Guid.Empty));
        
        Assert.Contains("Parcel ID cannot be empty", exception.Message);
    }

    [Fact]
    public async Task GetParcelByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync((Parcel?)null);

        // Act
        var result = await _service.GetParcelByIdAsync(parcelId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetParcelsByStatusAsync_ShouldReturnMappedParcels()
    {
        // Arrange
        var status = ParcelStatus.Pending;
        var parcels = new List<Parcel> { _testParcel };
        _mockParcelRepository.Setup(r => r.GetByStatusAsync(status))
            .ReturnsAsync(parcels);

        // Act
        var result = await _service.GetParcelsByStatusAsync(status);

        // Assert
        var parcelDtos = result.ToList();
        Assert.Single(parcelDtos);
        Assert.Equal(_testParcel.Status, parcelDtos.First().Status);
    }

    [Fact]
    public async Task GetParcelsRequiringInsuranceAsync_ShouldReturnMappedParcels()
    {
        // Arrange
        var highValueParcel = new Parcel(_testParcel.Recipient, 1m, 1500m); // High value parcel
        var parcels = new List<Parcel> { highValueParcel };
        _mockParcelRepository.Setup(r => r.GetRequiringInsuranceAsync())
            .ReturnsAsync(parcels);

        // Act
        var result = await _service.GetParcelsRequiringInsuranceAsync();

        // Assert
        var parcelDtos = result.ToList();
        Assert.Single(parcelDtos);
        Assert.Equal(highValueParcel.Value, parcelDtos.First().Value);
    }

    [Fact]
    public async Task GetParcelsByWeightRangeAsync_WithValidRange_ShouldReturnMappedParcels()
    {
        // Arrange
        const decimal minWeight = 0.1m;
        const decimal maxWeight = 1.0m;
        var parcels = new List<Parcel> { _testParcel };
        _mockParcelRepository.Setup(r => r.GetByWeightRangeAsync(minWeight, maxWeight))
            .ReturnsAsync(parcels);

        // Act
        var result = await _service.GetParcelsByWeightRangeAsync(minWeight, maxWeight);

        // Assert
        var parcelDtos = result.ToList();
        Assert.Single(parcelDtos);
        Assert.Equal(_testParcel.Weight, parcelDtos.First().Weight);
    }

    [Fact]
    public async Task GetParcelsByWeightRangeAsync_WithNegativeMinWeight_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetParcelsByWeightRangeAsync(-1m, 10m));
        
        Assert.Contains("Minimum weight cannot be negative", exception.Message);
    }

    [Fact]
    public async Task GetParcelsByWeightRangeAsync_WithMaxWeightLessThanMinWeight_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetParcelsByWeightRangeAsync(10m, 5m));
        
        Assert.Contains("Maximum weight cannot be less than minimum weight", exception.Message);
    }

    [Fact]
    public async Task GetParcelsByContainerAsync_WithValidContainerId_ShouldReturnMappedParcels()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        var parcels = new List<Parcel> { _testParcel };
        _mockParcelRepository.Setup(r => r.GetByContainerIdAsync(containerId))
            .ReturnsAsync(parcels);

        // Act
        var result = await _service.GetParcelsByContainerAsync(containerId);

        // Assert
        var parcelDtos = result.ToList();
        Assert.Single(parcelDtos);
        Assert.Equal(_testParcel.Weight, parcelDtos.First().Weight);
    }

    [Fact]
    public async Task GetParcelsByContainerAsync_WithEmptyContainerId_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _service.GetParcelsByContainerAsync(Guid.Empty));
        
        Assert.Contains("Container ID cannot be empty", exception.Message);
    }
}
