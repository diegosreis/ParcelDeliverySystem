using Application.DTOs;
using Application.Services;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class ParcelProcessingServiceTests
{
    private readonly Department _insuranceDepartment;
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly Mock<IDepartmentRuleService> _mockDepartmentRuleService;
    private readonly Mock<ILogger<ParcelProcessingService>> _mockLogger;
    private readonly Mock<IParcelRepository> _mockParcelRepository;
    private readonly Mock<IShippingContainerRepository> _mockShippingContainerRepository;
    private readonly ParcelProcessingService _service;
    private readonly Customer _testCustomer;
    private readonly Department _testDepartment;

    public ParcelProcessingServiceTests()
    {
        _mockParcelRepository = new Mock<IParcelRepository>();
        _mockShippingContainerRepository = new Mock<IShippingContainerRepository>();
        _mockDepartmentRepository = new Mock<IDepartmentRepository>();
        _mockDepartmentRuleService = new Mock<IDepartmentRuleService>();
        _mockLogger = new Mock<ILogger<ParcelProcessingService>>();

        _service = new ParcelProcessingService(
            _mockParcelRepository.Object,
            _mockShippingContainerRepository.Object,
            _mockDepartmentRepository.Object,
            _mockDepartmentRuleService.Object,
            _mockLogger.Object);

        // Setup test data
        var testAddress = new Address(
            "Test Street",
            "123",
            "",
            "Center",
            "Test City",
            "TS",
            "1234AB",
            "Netherlands"
        );

        _testCustomer = new Customer("Test Customer", testAddress);
        _testDepartment = new Department("Mail", "Mail department");
        _insuranceDepartment = new Department(DefaultDepartmentNames.Insurance, "Insurance department");
    }

    [Fact]
    public async Task ProcessParcelAsync_WithValidId_ShouldProcessParcel()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 0.5m, 100m);
        var departmentDto = new DepartmentDto(
            _testDepartment.Id,
            _testDepartment.Name,
            _testDepartment.Description,
            _testDepartment.IsActive,
            _testDepartment.CreatedAt,
            _testDepartment.UpdatedAt);

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcel.Id))
            .ReturnsAsync(parcel);
        _mockParcelRepository.Setup(r => r.UpdateAsync(It.IsAny<Parcel>()))
            .ReturnsAsync(parcel);
        _mockDepartmentRuleService.Setup(s => s.DetermineRequiredDepartmentsAsync(parcel.Id))
            .ReturnsAsync(new List<DepartmentDto> { departmentDto });
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(_testDepartment.Id))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _service.ProcessParcelAsync(parcel.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parcel.Id, result.Id);
        Assert.Equal(ParcelStatus.AssignedToDepartment, result.Status);
        _mockParcelRepository.Verify(r => r.UpdateAsync(It.IsAny<Parcel>()), Times.Once);
    }

    [Fact]
    public async Task ProcessParcelAsync_WithHighValueParcel_ShouldRequireInsuranceApproval()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 0.5m, 1500m);

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcel.Id))
            .ReturnsAsync(parcel);
        _mockParcelRepository.Setup(r => r.UpdateAsync(It.IsAny<Parcel>()))
            .ReturnsAsync(parcel);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync(DefaultDepartmentNames.Insurance))
            .ReturnsAsync(_insuranceDepartment);

        // Act
        var result = await _service.ProcessParcelAsync(parcel.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ParcelStatus.InsuranceApprovalRequired, result.Status);
        _mockDepartmentRepository.Verify(r => r.GetByNameAsync(DefaultDepartmentNames.Insurance), Times.Once);
        _mockParcelRepository.Verify(r => r.UpdateAsync(It.IsAny<Parcel>()), Times.Once);
    }

    [Fact]
    public async Task ProcessParcelAsync_WithNonExistentParcel_ShouldThrowArgumentException()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync((Parcel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessParcelAsync(parcelId));
        Assert.Contains($"Parcel with ID {parcelId} not found", exception.Message);
    }

    [Fact]
    public async Task ProcessContainerAsync_WithValidContainer_ShouldProcessAllParcels()
    {
        // Arrange
        var parcel1 = new Parcel(_testCustomer, 0.5m, 100m);
        var parcel2 = new Parcel(_testCustomer, 2m, 200m);
        var container = new ShippingContainer("123456", DateTime.UtcNow);
        container.AddParcel(parcel1);
        container.AddParcel(parcel2);

        var departmentDto = new DepartmentDto(
            _testDepartment.Id,
            _testDepartment.Name,
            _testDepartment.Description,
            _testDepartment.IsActive,
            _testDepartment.CreatedAt,
            _testDepartment.UpdatedAt);

        _mockShippingContainerRepository.Setup(r => r.GetByIdAsync(container.Id))
            .ReturnsAsync(container);
        _mockParcelRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => container.Parcels.FirstOrDefault(p => p.Id == id));
        _mockParcelRepository.Setup(r => r.UpdateAsync(It.IsAny<Parcel>()))
            .ReturnsAsync((Parcel p) => p);
        _mockDepartmentRuleService.Setup(s => s.DetermineRequiredDepartmentsAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new List<DepartmentDto> { departmentDto });
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(_testDepartment.Id))
            .ReturnsAsync(_testDepartment);

        // Act
        var result = await _service.ProcessContainerAsync(container.Id);

        // Assert
        var processedParcels = result.ToList();
        Assert.Equal(2, processedParcels.Count);
        Assert.All(processedParcels, p => Assert.Equal(ParcelStatus.AssignedToDepartment, p.Status));
    }

    [Fact]
    public async Task ProcessContainerAsync_WithNonExistentContainer_ShouldThrowArgumentException()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        _mockShippingContainerRepository.Setup(r => r.GetByIdAsync(containerId))
            .ReturnsAsync((ShippingContainer?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => _service.ProcessContainerAsync(containerId));
        Assert.Contains($"Container with ID {containerId} not found", exception.Message);
    }

    [Fact]
    public async Task AssignDepartmentAsync_WithValidIds_ShouldAssignDepartment()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 0.5m, 100m);

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcel.Id))
            .ReturnsAsync(parcel);
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(_testDepartment.Id))
            .ReturnsAsync(_testDepartment);
        _mockParcelRepository.Setup(r => r.UpdateAsync(It.IsAny<Parcel>()))
            .ReturnsAsync(parcel);

        // Act
        var result = await _service.AssignDepartmentAsync(parcel.Id, _testDepartment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parcel.Id, result.Id);
        _mockParcelRepository.Verify(r => r.UpdateAsync(It.IsAny<Parcel>()), Times.Once);
    }

    [Fact]
    public async Task AssignDepartmentAsync_WithNonExistentParcel_ShouldThrowArgumentException()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync((Parcel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AssignDepartmentAsync(parcelId, _testDepartment.Id));
        Assert.Contains($"Parcel with ID {parcelId} not found", exception.Message);
    }

    [Fact]
    public async Task AssignDepartmentAsync_WithNonExistentDepartment_ShouldThrowArgumentException()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 0.5m, 100m);
        var departmentId = Guid.NewGuid();

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcel.Id))
            .ReturnsAsync(parcel);
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(departmentId))
            .ReturnsAsync((Department?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AssignDepartmentAsync(parcel.Id, departmentId));
        Assert.Contains($"Department with ID {departmentId} not found", exception.Message);
    }

    [Fact]
    public async Task RemoveDepartmentAsync_WithValidIds_ShouldRemoveDepartment()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 0.5m, 100m);
        parcel.AssignDepartment(_testDepartment);

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcel.Id))
            .ReturnsAsync(parcel);
        _mockDepartmentRepository.Setup(r => r.GetByIdAsync(_testDepartment.Id))
            .ReturnsAsync(_testDepartment);
        _mockParcelRepository.Setup(r => r.UpdateAsync(It.IsAny<Parcel>()))
            .ReturnsAsync(parcel);

        // Act
        var result = await _service.RemoveDepartmentAsync(parcel.Id, _testDepartment.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(parcel.Id, result.Id);
        _mockParcelRepository.Verify(r => r.UpdateAsync(It.IsAny<Parcel>()), Times.Once);
    }

    [Fact]
    public async Task UpdateParcelStatusAsync_WithValidData_ShouldUpdateStatus()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 0.5m, 100m);
        var newStatus = ParcelStatus.Delivered;

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcel.Id))
            .ReturnsAsync(parcel);
        _mockParcelRepository.Setup(r => r.UpdateAsync(It.IsAny<Parcel>()))
            .ReturnsAsync(parcel);

        // Act
        var result = await _service.UpdateParcelStatusAsync(parcel.Id, newStatus);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newStatus, result.Status);
        _mockParcelRepository.Verify(r => r.UpdateAsync(It.IsAny<Parcel>()), Times.Once);
    }

    [Fact]
    public async Task UpdateParcelStatusAsync_WithNonExistentParcel_ShouldThrowArgumentException()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync((Parcel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateParcelStatusAsync(parcelId, ParcelStatus.Delivered));
        Assert.Contains($"Parcel with ID {parcelId} not found", exception.Message);
    }

    [Fact]
    public async Task GetAssignedDepartmentsAsync_WithValidParcel_ShouldReturnDepartments()
    {
        // Arrange
        var parcel = new Parcel(_testCustomer, 0.5m, 100m);
        parcel.AssignDepartment(_testDepartment);

        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcel.Id))
            .ReturnsAsync(parcel);

        // Act
        var result = await _service.GetAssignedDepartmentsAsync(parcel.Id);

        // Assert
        var departments = result.ToList();
        Assert.Single(departments);
        Assert.Equal(_testDepartment.Id, departments.First().Id);
        Assert.Equal(_testDepartment.Name, departments.First().Name);
    }

    [Fact]
    public async Task GetAssignedDepartmentsAsync_WithNonExistentParcel_ShouldThrowArgumentException()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync((Parcel?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GetAssignedDepartmentsAsync(parcelId));
        Assert.Contains($"Parcel with ID {parcelId} not found", exception.Message);
    }
}