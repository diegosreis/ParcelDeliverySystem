using Api.Controllers;
using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Api.Controllers;

public class ShippingContainersControllerTests
{
    private readonly ShippingContainersController _controller;
    private readonly Mock<IParcelProcessingService> _mockParcelProcessingService;
    private readonly Mock<IShippingContainerService> _mockShippingContainerService;
    private readonly Mock<IXmlImportService> _mockXmlImportService;
    private readonly ShippingContainerDto _testContainerDto;

    public ShippingContainersControllerTests()
    {
        _mockShippingContainerService = new Mock<IShippingContainerService>();
        _mockXmlImportService = new Mock<IXmlImportService>();
        _mockParcelProcessingService = new Mock<IParcelProcessingService>();
        var mockLogger = new Mock<ILogger<ShippingContainersController>>();
        _controller = new ShippingContainersController(
            _mockShippingContainerService.Object,
            _mockXmlImportService.Object,
            _mockParcelProcessingService.Object,
            mockLogger.Object);

        _testContainerDto = new ShippingContainerDto(
            Guid.NewGuid(),
            "Container-001", // ContainerId as string
            DateTime.UtcNow.AddDays(-1),
            ShippingContainerStatus.Processing,
            10,
            25.5m, // TotalWeight
            1500m, // TotalValue
            2, // ParcelsRequiringInsurance
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow
        );
    }

    [Fact]
    public void Constructor_WithNullShippingContainerService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShippingContainersController(
            null!,
            _mockXmlImportService.Object,
            _mockParcelProcessingService.Object,
            Mock.Of<ILogger<ShippingContainersController>>()));
    }

    [Fact]
    public void Constructor_WithNullXmlImportService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShippingContainersController(
            _mockShippingContainerService.Object,
            null!,
            _mockParcelProcessingService.Object,
            Mock.Of<ILogger<ShippingContainersController>>()));
    }

    [Fact]
    public void Constructor_WithNullParcelProcessingService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new ShippingContainersController(
            _mockShippingContainerService.Object,
            _mockXmlImportService.Object,
            null!,
            Mock.Of<ILogger<ShippingContainersController>>()));
    }

    [Fact]
    public async Task GetShippingContainers_WhenContainersExist_ShouldReturnOkWithContainers()
    {
        // Arrange
        var containers = new List<ShippingContainerDto> { _testContainerDto };
        _mockShippingContainerService.Setup(s => s.GetAllContainersAsync())
            .ReturnsAsync(containers);

        // Act
        var result = await _controller.GetShippingContainers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainers = Assert.IsAssignableFrom<IEnumerable<ShippingContainerDto>>(okResult.Value).ToList();
        Assert.Single(returnedContainers);
        Assert.Equal(_testContainerDto.Id, returnedContainers.First().Id);
    }

    [Fact]
    public async Task GetShippingContainers_WhenServiceThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockShippingContainerService.Setup(s => s.GetAllContainersAsync())
            .ThrowsAsync(new Exception("Service error"));

        // Act
        var result = await _controller.GetShippingContainers();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetShippingContainer_WithValidId_ShouldReturnOkWithContainer()
    {
        // Arrange
        var containerId = _testContainerDto.Id;
        _mockShippingContainerService.Setup(s => s.GetContainerByIdAsync(containerId))
            .ReturnsAsync(_testContainerDto);

        // Act
        var result = await _controller.GetShippingContainer(containerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainer = Assert.IsType<ShippingContainerDto>(okResult.Value);
        Assert.Equal(_testContainerDto.Id, returnedContainer.Id);
    }

    [Fact]
    public async Task GetShippingContainer_WithEmptyId_ShouldReturnBadRequest()
    {
        // Arrange
        _mockShippingContainerService.Setup(s => s.GetContainerByIdAsync(Guid.Empty))
            .ThrowsAsync(new ArgumentException("Container ID cannot be empty"));

        // Act
        var result = await _controller.GetShippingContainer(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetShippingContainer_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        _mockShippingContainerService.Setup(s => s.GetContainerByIdAsync(containerId))
            .ReturnsAsync((ShippingContainerDto?)null);

        // Act
        var result = await _controller.GetShippingContainer(containerId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetShippingContainerWithParcels_WithValidId_ShouldReturnOkWithContainerAndParcels()
    {
        // Arrange
        var containerId = _testContainerDto.Id;
        var containerWithParcels = new ShippingContainerDto(
            containerId,
            "Container-002", // ContainerId as string
            _testContainerDto.ShippingDate,
            _testContainerDto.Status,
            _testContainerDto.TotalParcels,
            _testContainerDto.TotalWeight,
            _testContainerDto.TotalValue,
            _testContainerDto.ParcelsRequiringInsurance,
            _testContainerDto.CreatedAt,
            _testContainerDto.UpdatedAt
        );

        _mockShippingContainerService.Setup(s => s.GetContainerWithParcelsAsync(containerId))
            .ReturnsAsync(containerWithParcels);

        // Act
        var result = await _controller.GetShippingContainerWithParcels(containerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainer = Assert.IsType<ShippingContainerDto>(okResult.Value);
        Assert.Equal(containerId, returnedContainer.Id);
    }

    [Fact]
    public async Task GetContainersByStatus_ShouldReturnOkWithFilteredContainers()
    {
        // Arrange
        var status = ShippingContainerStatus.Processing;
        var containers = new List<ShippingContainerDto> { _testContainerDto };
        _mockShippingContainerService.Setup(s => s.GetContainersByStatusAsync(status))
            .ReturnsAsync(containers);

        // Act
        var result = await _controller.GetContainersByStatus(status);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainers = Assert.IsAssignableFrom<IEnumerable<ShippingContainerDto>>(okResult.Value).ToList();
        Assert.Single(returnedContainers);
        Assert.Equal(status, returnedContainers.First().Status);
    }

    [Fact]
    public async Task GetContainersByDateRange_WithValidRange_ShouldReturnOkWithContainers()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-2);
        var endDate = DateTime.UtcNow;
        var containers = new List<ShippingContainerDto> { _testContainerDto };
        _mockShippingContainerService.Setup(s => s.GetContainersByDateRangeAsync(startDate, endDate))
            .ReturnsAsync(containers);

        // Act
        var result = await _controller.GetContainersByDateRange(startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainers = Assert.IsAssignableFrom<IEnumerable<ShippingContainerDto>>(okResult.Value).ToList();
        Assert.Single(returnedContainers);
    }

    [Fact]
    public async Task GetContainersByDateRange_WithInvalidRange_ShouldReturnBadRequest()
    {
        // Arrange
        var startDate = DateTime.UtcNow;
        var endDate = DateTime.UtcNow.AddDays(-1);
        _mockShippingContainerService.Setup(s => s.GetContainersByDateRangeAsync(startDate, endDate))
            .ThrowsAsync(new ArgumentException("Start date cannot be greater than end date"));

        // Act
        var result = await _controller.GetContainersByDateRange(startDate, endDate);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ImportFromXmlFile_WithValidFile_ShouldReturnOkWithImportedContainer()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "<?xml version=\"1.0\"?><Container></Container>";
        var fileName = "test.xml";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.Length).Returns(ms.Length);

        _mockXmlImportService.Setup(s => s.ImportContainerFromXmlAsync(It.IsAny<string>()))
            .ReturnsAsync(_testContainerDto);

        // Act
        var result = await _controller.ImportFromXmlFile(mockFile.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainer = Assert.IsType<ShippingContainerDto>(okResult.Value);
        Assert.Equal(_testContainerDto.Id, returnedContainer.Id);
    }

    [Fact]
    public async Task ImportFromXmlFile_WithNullFile_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.ImportFromXmlFile(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ImportFromXmlFile_WithEmptyFile_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(_ => _.Length).Returns(0);

        // Act
        var result = await _controller.ImportFromXmlFile(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ImportFromXmlFile_WithNonXmlFile_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(_ => _.FileName).Returns("test.txt");
        mockFile.Setup(_ => _.Length).Returns(100);

        // Act
        var result = await _controller.ImportFromXmlFile(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ImportFromXmlFile_WhenImportServiceThrowsArgumentException_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = new Mock<IFormFile>();
        var content = "invalid xml";
        var fileName = "test.xml";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        mockFile.Setup(_ => _.OpenReadStream()).Returns(ms);
        mockFile.Setup(_ => _.FileName).Returns(fileName);
        mockFile.Setup(_ => _.Length).Returns(ms.Length);

        _mockXmlImportService.Setup(s => s.ImportContainerFromXmlAsync(It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Invalid XML format"));

        // Act
        var result = await _controller.ImportFromXmlFile(mockFile.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ProcessContainer_WithValidId_ShouldReturnOkWithResult()
    {
        // Arrange
        var containerId = _testContainerDto.Id;
        var processingResult = new List<ParcelDto>();

        _mockParcelProcessingService.Setup(s => s.ProcessContainerAsync(containerId))
            .ReturnsAsync(processingResult);

        // Act
        var result = await _controller.ProcessContainer(containerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ProcessContainer_WithEmptyId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.ProcessContainer(Guid.Empty);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ProcessContainer_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        _mockParcelProcessingService.Setup(s => s.ProcessContainerAsync(containerId))
            .ThrowsAsync(new ArgumentException("Container not found"));

        // Act
        var result = await _controller.ProcessContainer(containerId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }
}