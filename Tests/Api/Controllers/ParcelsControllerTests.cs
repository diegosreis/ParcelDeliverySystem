using Api.Controllers;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Api.Controllers;

public class ParcelsControllerTests
{
    private readonly ParcelsController _controller;
    private readonly Mock<IDepartmentRuleService> _mockDepartmentRuleService;
    private readonly Mock<IParcelProcessingService> _mockParcelProcessingService;
    private readonly Mock<IParcelRepository> _mockParcelRepository;
    private readonly Address _testAddress;
    private readonly Customer _testCustomer;
    private readonly Department _testDepartment;

    private readonly Parcel _testParcel;
    private readonly ParcelDto _testParcelDto;

    public ParcelsControllerTests()
    {
        _mockParcelRepository = new Mock<IParcelRepository>();
        _mockParcelProcessingService = new Mock<IParcelProcessingService>();
        _mockDepartmentRuleService = new Mock<IDepartmentRuleService>();

        _controller = new ParcelsController(
            _mockParcelRepository.Object,
            _mockParcelProcessingService.Object,
            _mockDepartmentRuleService.Object);

        // Setup test data
        _testAddress = new Address("Marijkestraat", "28", "", "Center", "Bosschenhoofd", "NB", "4744AT", "Netherlands");
        _testCustomer = new Customer("Vinny Gankema", _testAddress);
        _testDepartment = new Department("Mail", "Handles lightweight parcels");
        _testParcel = new Parcel(_testCustomer, 0.5m, 100m);

        var customerDto = new CustomerDto(
            _testCustomer.Id,
            _testCustomer.Name,
            new AddressDto(
                _testAddress.Id,
                _testAddress.Street,
                _testAddress.Number,
                _testAddress.Complement,
                _testAddress.Neighborhood,
                _testAddress.City,
                _testAddress.State,
                _testAddress.ZipCode,
                _testAddress.Country,
                _testAddress.CreatedAt,
                _testAddress.UpdatedAt),
            _testCustomer.CreatedAt,
            _testCustomer.UpdatedAt);

        _testParcelDto = new ParcelDto(
            _testParcel.Id,
            customerDto,
            _testParcel.Weight,
            _testParcel.Value,
            _testParcel.Status,
            new List<DepartmentDto>(),
            _testParcel.CreatedAt,
            _testParcel.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithNullParcelRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ParcelsController(
            null!,
            _mockParcelProcessingService.Object,
            _mockDepartmentRuleService.Object));
    }

    [Fact]
    public void Constructor_WithNullParcelProcessingService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ParcelsController(
            _mockParcelRepository.Object,
            null!,
            _mockDepartmentRuleService.Object));
    }

    [Fact]
    public void Constructor_WithNullDepartmentRuleService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ParcelsController(
            _mockParcelRepository.Object,
            _mockParcelProcessingService.Object,
            null!));
    }

    [Fact]
    public async Task GetParcels_WhenParcelsExist_ShouldReturnOkWithParcels()
    {
        // Arrange
        var parcels = new List<Parcel> { _testParcel };
        _mockParcelRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(parcels);

        // Act
        var result = await _controller.GetParcels();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Single(returnedParcels);
        Assert.Equal(_testParcel.Weight, returnedParcels.First().Weight);
    }

    [Fact]
    public async Task GetParcels_WhenNoParcels_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        _mockParcelRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Parcel>());

        // Act
        var result = await _controller.GetParcels();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Empty(returnedParcels);
    }

    [Fact]
    public async Task GetParcels_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockParcelRepository.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetParcels();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetParcel_WithValidId_ShouldReturnOkWithParcel()
    {
        // Arrange
        var parcelId = _testParcel.Id;
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync(_testParcel);

        // Act
        var result = await _controller.GetParcel(parcelId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcel = Assert.IsType<ParcelDto>(okResult.Value);
        Assert.Equal(_testParcel.Weight, returnedParcel.Weight);
    }

    [Fact]
    public async Task GetParcel_WithEmptyId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetParcel(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetParcel_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ReturnsAsync((Parcel?)null);

        // Act
        var result = await _controller.GetParcel(parcelId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetParcel_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelRepository.Setup(r => r.GetByIdAsync(parcelId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetParcel(parcelId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetParcelsByStatus_WithValidStatus_ShouldReturnOkWithParcels()
    {
        // Arrange
        var status = ParcelStatus.Processing;
        var parcels = new List<Parcel> { _testParcel };
        _mockParcelRepository.Setup(r => r.GetByStatusAsync(status))
            .ReturnsAsync(parcels);

        // Act
        var result = await _controller.GetParcelsByStatus(status);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Single(returnedParcels);
    }

    [Fact]
    public async Task GetParcelsByStatus_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        var status = ParcelStatus.Processing;
        _mockParcelRepository.Setup(r => r.GetByStatusAsync(status))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetParcelsByStatus(status);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetParcelsRequiringInsurance_WhenParcelsExist_ShouldReturnOkWithParcels()
    {
        // Arrange
        var highValueParcel = new Parcel(_testCustomer, 1.5m, 1500m);
        var parcels = new List<Parcel> { highValueParcel };
        _mockParcelRepository.Setup(r => r.GetRequiringInsuranceAsync())
            .ReturnsAsync(parcels);

        // Act
        var result = await _controller.GetParcelsRequiringInsurance();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Single(returnedParcels);
        Assert.True(returnedParcels.First().Value > 1000);
    }

    [Fact]
    public async Task GetParcelsRequiringInsurance_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockParcelRepository.Setup(r => r.GetRequiringInsuranceAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetParcelsRequiringInsurance();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task ProcessParcel_WithValidId_ShouldReturnOkWithProcessedParcel()
    {
        // Arrange
        var parcelId = _testParcel.Id;
        _mockParcelProcessingService.Setup(s => s.ProcessParcelAsync(parcelId))
            .ReturnsAsync(_testParcelDto);

        // Act
        var result = await _controller.ProcessParcel(parcelId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcel = Assert.IsType<ParcelDto>(okResult.Value);
        Assert.Equal(_testParcel.Weight, returnedParcel.Weight);
    }

    [Fact]
    public async Task ProcessParcel_WithEmptyId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.ProcessParcel(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ProcessParcel_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelProcessingService.Setup(s => s.ProcessParcelAsync(parcelId))
            .ThrowsAsync(new ArgumentException("Parcel not found"));

        // Act
        var result = await _controller.ProcessParcel(parcelId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task ProcessParcel_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockParcelProcessingService.Setup(s => s.ProcessParcelAsync(parcelId))
            .ThrowsAsync(new Exception("Processing error"));

        // Act
        var result = await _controller.ProcessParcel(parcelId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithValidData_ShouldReturnOkWithUpdatedParcel()
    {
        // Arrange
        var parcelId = _testParcel.Id;
        var newStatus = ParcelStatus.Processed;
        _mockParcelProcessingService.Setup(s => s.UpdateParcelStatusAsync(parcelId, newStatus))
            .ReturnsAsync(_testParcelDto);

        // Act
        var result = await _controller.UpdateStatus(parcelId, newStatus);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcel = Assert.IsType<ParcelDto>(okResult.Value);
        Assert.Equal(_testParcel.Weight, returnedParcel.Weight);
    }

    [Fact]
    public async Task UpdateStatus_WithEmptyId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.UpdateStatus(Guid.Empty, ParcelStatus.Processed);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        var status = ParcelStatus.Processed;
        _mockParcelProcessingService.Setup(s => s.UpdateParcelStatusAsync(parcelId, status))
            .ThrowsAsync(new ArgumentException("Parcel not found"));

        // Act
        var result = await _controller.UpdateStatus(parcelId, status);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        var status = ParcelStatus.Processed;
        _mockParcelProcessingService.Setup(s => s.UpdateParcelStatusAsync(parcelId, status))
            .ThrowsAsync(new Exception("Update error"));

        // Act
        var result = await _controller.UpdateStatus(parcelId, status);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetRequiredDepartments_WithValidId_ShouldReturnOkWithDepartments()
    {
        // Arrange
        var parcelId = _testParcel.Id;
        var departmentDto = new DepartmentDto(
            _testDepartment.Id,
            _testDepartment.Name,
            _testDepartment.Description,
            _testDepartment.IsActive,
            _testDepartment.CreatedAt,
            _testDepartment.UpdatedAt);
        var departments = new List<DepartmentDto> { departmentDto };

        _mockDepartmentRuleService.Setup(s => s.DetermineRequiredDepartmentsAsync(parcelId))
            .ReturnsAsync(departments);

        // Act
        var result = await _controller.GetRequiredDepartments(parcelId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartments = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value).ToList();
        Assert.Single(returnedDepartments);
        Assert.Equal(_testDepartment.Name, returnedDepartments.First().Name);
    }

    [Fact]
    public async Task GetRequiredDepartments_WithEmptyId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetRequiredDepartments(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetRequiredDepartments_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockDepartmentRuleService.Setup(s => s.DetermineRequiredDepartmentsAsync(parcelId))
            .ThrowsAsync(new ArgumentException("Parcel not found"));

        // Act
        var result = await _controller.GetRequiredDepartments(parcelId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetRequiredDepartments_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        var parcelId = Guid.NewGuid();
        _mockDepartmentRuleService.Setup(s => s.DetermineRequiredDepartmentsAsync(parcelId))
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetRequiredDepartments(parcelId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }
}