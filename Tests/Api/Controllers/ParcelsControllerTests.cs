using Api.Controllers;
using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Api.Controllers;

public class ParcelsControllerTests
{
    private readonly ParcelsController _controller;
    private readonly Mock<IParcelService> _mockParcelService;
    private readonly Mock<IParcelProcessingService> _mockParcelProcessingService;
    private readonly Mock<IDepartmentRuleService> _mockDepartmentRuleService;
    private readonly ParcelDto _testParcelDto;

    public ParcelsControllerTests()
    {
        _mockParcelService = new Mock<IParcelService>();
        _mockParcelProcessingService = new Mock<IParcelProcessingService>();
        _mockDepartmentRuleService = new Mock<IDepartmentRuleService>();
        _controller = new ParcelsController(
            _mockParcelService.Object,
            _mockParcelProcessingService.Object,
            _mockDepartmentRuleService.Object);

        _testParcelDto = new ParcelDto(
            Guid.NewGuid(),
            new CustomerDto(
                Guid.NewGuid(),
                "João Silva",
                new AddressDto(
                    Guid.NewGuid(),
                    "Marijkestraat",
                    "28",
                    "",
                    "Center",
                    "Bosschenhoofd",
                    "NB",
                    "4744AT",
                    "Netherlands",
                    DateTime.UtcNow,
                    null
                ),
                DateTime.UtcNow,
                null
            ),
            0.5m,
            50m,
            ParcelStatus.Pending,
            new List<DepartmentDto>(),
            DateTime.UtcNow,
            null
        );
    }

    [Fact]
    public void Constructor_WithNullParcelService_ShouldThrowArgumentNullException()
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
            _mockParcelService.Object,
            null!,
            _mockDepartmentRuleService.Object));
    }

    [Fact]
    public void Constructor_WithNullDepartmentRuleService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ParcelsController(
            _mockParcelService.Object,
            _mockParcelProcessingService.Object,
            null!));
    }

    [Fact]
    public async Task GetParcels_WhenParcelsExist_ShouldReturnOkWithParcels()
    {
        // Arrange
        var parcels = new List<ParcelDto> { _testParcelDto };
        _mockParcelService.Setup(s => s.GetAllParcelsAsync())
            .ReturnsAsync(parcels);

        // Act
        var result = await _controller.GetParcels();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Single(returnedParcels);
        Assert.Equal(_testParcelDto.Weight, returnedParcels.First().Weight);
    }

    [Fact]
    public async Task GetParcels_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockParcelService.Setup(s => s.GetAllParcelsAsync())
            .ThrowsAsync(new Exception("Service error"));

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
        var parcelId = _testParcelDto.Id;
        _mockParcelService.Setup(s => s.GetParcelByIdAsync(parcelId))
            .ReturnsAsync(_testParcelDto);

        // Act
        var result = await _controller.GetParcel(parcelId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcel = Assert.IsType<ParcelDto>(okResult.Value);
        Assert.Equal(_testParcelDto.Weight, returnedParcel.Weight);
    }

    [Fact]
    public async Task GetParcel_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        _mockParcelService.Setup(s => s.GetParcelByIdAsync(Guid.Empty))
            .ThrowsAsync(new ArgumentException("Parcel ID cannot be empty"));

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
        _mockParcelService.Setup(s => s.GetParcelByIdAsync(parcelId))
            .ReturnsAsync((ParcelDto?)null);

        // Act
        var result = await _controller.GetParcel(parcelId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetParcelsByStatus_ShouldReturnOkWithFilteredParcels()
    {
        // Arrange
        var status = ParcelStatus.Pending;
        var parcels = new List<ParcelDto> { _testParcelDto };
        _mockParcelService.Setup(s => s.GetParcelsByStatusAsync(status))
            .ReturnsAsync(parcels);

        // Act
        var result = await _controller.GetParcelsByStatus(status);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Single(returnedParcels);
        Assert.Equal(status, returnedParcels.First().Status);
    }

    [Fact]
    public async Task GetParcelsRequiringInsurance_ShouldReturnOkWithInsuranceParcels()
    {
        // Arrange
        var highValueParcel = new ParcelDto(
            Guid.NewGuid(),
            _testParcelDto.Recipient,
            1m,
            1500m, // High value
            ParcelStatus.Pending,
            new List<DepartmentDto>(),
            DateTime.UtcNow,
            null
        );
        var parcels = new List<ParcelDto> { highValueParcel };
        _mockParcelService.Setup(s => s.GetParcelsRequiringInsuranceAsync())
            .ReturnsAsync(parcels);

        // Act
        var result = await _controller.GetParcelsRequiringInsurance();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Single(returnedParcels);
        Assert.Equal(1500m, returnedParcels.First().Value);
    }

    [Fact]
    public async Task GetParcelsByWeightRange_WithValidRange_ShouldReturnOkWithParcels()
    {
        // Arrange
        const decimal minWeight = 0.1m;
        const decimal maxWeight = 1.0m;
        var parcels = new List<ParcelDto> { _testParcelDto };
        _mockParcelService.Setup(s => s.GetParcelsByWeightRangeAsync(minWeight, maxWeight))
            .ReturnsAsync(parcels);

        // Act
        var result = await _controller.GetParcelsByWeightRange(minWeight, maxWeight);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Single(returnedParcels);
    }

    [Fact]
    public async Task GetParcelsByWeightRange_WithInvalidRange_ShouldReturnBadRequest()
    {
        // Arrange
        _mockParcelService.Setup(s => s.GetParcelsByWeightRangeAsync(-1m, 10m))
            .ThrowsAsync(new ArgumentException("Minimum weight cannot be negative"));

        // Act
        var result = await _controller.GetParcelsByWeightRange(-1m, 10m);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetParcelsByContainer_WithValidContainerId_ShouldReturnOkWithParcels()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        var parcels = new List<ParcelDto> { _testParcelDto };
        _mockParcelService.Setup(s => s.GetParcelsByContainerAsync(containerId))
            .ReturnsAsync(parcels);

        // Act
        var result = await _controller.GetParcelsByContainer(containerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcels = Assert.IsAssignableFrom<IEnumerable<ParcelDto>>(okResult.Value).ToList();
        Assert.Single(returnedParcels);
    }

    [Fact]
    public async Task GetParcelsByContainer_WithEmptyContainerId_ShouldReturnBadRequest()
    {
        // Arrange
        _mockParcelService.Setup(s => s.GetParcelsByContainerAsync(Guid.Empty))
            .ThrowsAsync(new ArgumentException("Container ID cannot be empty"));

        // Act
        var result = await _controller.GetParcelsByContainer(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ProcessParcel_WithValidId_ShouldReturnOkWithProcessedParcel()
    {
        // Arrange
        var parcelId = _testParcelDto.Id;
        var processedParcel = new ParcelDto(
            parcelId,
            _testParcelDto.Recipient,
            _testParcelDto.Weight,
            _testParcelDto.Value,
            ParcelStatus.Processing,
            new List<DepartmentDto>(),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mockParcelProcessingService.Setup(s => s.ProcessParcelAsync(parcelId))
            .ReturnsAsync(processedParcel);

        // Act
        var result = await _controller.ProcessParcel(parcelId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcel = Assert.IsType<ParcelDto>(okResult.Value);
        Assert.Equal(ParcelStatus.Processing, returnedParcel.Status);
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
    public async Task UpdateStatus_WithValidData_ShouldReturnOkWithUpdatedParcel()
    {
        // Arrange
        var parcelId = _testParcelDto.Id;
        var newStatus = ParcelStatus.Processing;
        var updatedParcel = new ParcelDto(
            parcelId,
            _testParcelDto.Recipient,
            _testParcelDto.Weight,
            _testParcelDto.Value,
            newStatus,
            new List<DepartmentDto>(),
            DateTime.UtcNow,
            DateTime.UtcNow
        );

        _mockParcelProcessingService.Setup(s => s.UpdateParcelStatusAsync(parcelId, newStatus))
            .ReturnsAsync(updatedParcel);

        // Act
        var result = await _controller.UpdateStatus(parcelId, newStatus);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedParcel = Assert.IsType<ParcelDto>(okResult.Value);
        Assert.Equal(newStatus, returnedParcel.Status);
    }

    [Fact]
    public async Task UpdateStatus_WithEmptyId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.UpdateStatus(Guid.Empty, ParcelStatus.Processing);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetRequiredDepartments_WithValidId_ShouldReturnOkWithDepartments()
    {
        // Arrange
        var parcelId = _testParcelDto.Id;
        var departments = new List<DepartmentDto>
        {
            new DepartmentDto(Guid.NewGuid(), "Mail", "Mail department", true, DateTime.UtcNow, DateTime.UtcNow)
        };

        _mockDepartmentRuleService.Setup(s => s.DetermineRequiredDepartmentsAsync(parcelId))
            .ReturnsAsync(departments);

        // Act
        var result = await _controller.GetRequiredDepartments(parcelId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedDepartments = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(okResult.Value).ToList();
        Assert.Single(returnedDepartments);
        Assert.Equal("Mail", returnedDepartments.First().Name);
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
}
