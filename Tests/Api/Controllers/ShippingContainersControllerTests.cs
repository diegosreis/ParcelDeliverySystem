using Api.Controllers;
using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.Api.Controllers;

public class ShippingContainersControllerTests
{
    private readonly ShippingContainersController _controller;
    private readonly Mock<IParcelProcessingService> _mockParcelProcessingService;
    private readonly Mock<IShippingContainerRepository> _mockShippingContainerRepository;
    private readonly Mock<IXmlImportService> _mockXmlImportService;

    private readonly ShippingContainer _testContainer;
    private readonly ShippingContainerDto _testContainerDto;
    private readonly ShippingContainerWithParcelsDto _testContainerWithParcelsDto;

    public ShippingContainersControllerTests()
    {
        _mockShippingContainerRepository = new Mock<IShippingContainerRepository>();
        _mockXmlImportService = new Mock<IXmlImportService>();
        _mockParcelProcessingService = new Mock<IParcelProcessingService>();

        _controller = new ShippingContainersController(
            _mockShippingContainerRepository.Object,
            _mockXmlImportService.Object,
            _mockParcelProcessingService.Object);

        // Setup test data
        _testContainer = new ShippingContainer("CONTAINER123", DateTime.UtcNow);
        _testContainerDto = new ShippingContainerDto(
            _testContainer.Id,
            _testContainer.ContainerId,
            _testContainer.ShippingDate,
            _testContainer.Status,
            _testContainer.TotalParcels,
            _testContainer.TotalWeight,
            _testContainer.TotalValue,
            _testContainer.ParcelsRequiringInsurance,
            _testContainer.CreatedAt,
            _testContainer.UpdatedAt);

        _testContainerWithParcelsDto = new ShippingContainerWithParcelsDto(
            _testContainer.Id,
            _testContainer.ContainerId,
            _testContainer.ShippingDate,
            _testContainer.Status,
            new List<ParcelDto>(),
            _testContainer.CreatedAt,
            _testContainer.UpdatedAt);
    }

    private static IFormFile CreateMockFormFile(string fileName, string content)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(content);
        writer.Flush();
        stream.Position = 0;

        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(stream.Length);
        file.Setup(f => f.OpenReadStream()).Returns(stream);

        return file.Object;
    }

    [Fact]
    public void Constructor_WithNullShippingContainerRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ShippingContainersController(null!, _mockXmlImportService.Object, _mockParcelProcessingService.Object));
    }

    [Fact]
    public void Constructor_WithNullXmlImportService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ShippingContainersController(_mockShippingContainerRepository.Object, null!,
                _mockParcelProcessingService.Object));
    }

    [Fact]
    public void Constructor_WithNullParcelProcessingService_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ShippingContainersController(_mockShippingContainerRepository.Object, _mockXmlImportService.Object,
                null!));
    }

    [Fact]
    public async Task GetShippingContainers_WhenContainersExist_ShouldReturnOkWithContainers()
    {
        // Arrange
        var containers = new List<ShippingContainer> { _testContainer };
        _mockShippingContainerRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(containers);

        // Act
        var result = await _controller.GetShippingContainers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainers = Assert.IsAssignableFrom<IEnumerable<ShippingContainerDto>>(okResult.Value);
        Assert.Single(returnedContainers);
    }

    [Fact]
    public async Task GetShippingContainers_WhenNoContainers_ShouldReturnOkWithEmptyList()
    {
        // Arrange
        _mockShippingContainerRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<ShippingContainer>());

        // Act
        var result = await _controller.GetShippingContainers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainers = Assert.IsAssignableFrom<IEnumerable<ShippingContainerDto>>(okResult.Value);
        Assert.Empty(returnedContainers);
    }

    [Fact]
    public async Task GetShippingContainers_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockShippingContainerRepository.Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetShippingContainers();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetContainer_WithValidId_ShouldReturnOkWithContainer()
    {
        // Arrange
        _mockShippingContainerRepository.Setup(r => r.GetByIdAsync(_testContainer.Id))
            .ReturnsAsync(_testContainer);

        // Act
        var result = await _controller.GetContainer(_testContainer.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainer = Assert.IsType<ShippingContainerDto>(okResult.Value);
        Assert.Equal(_testContainer.Id, returnedContainer.Id);
    }

    [Fact]
    public async Task GetContainer_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var invalidId = Guid.NewGuid();
        _mockShippingContainerRepository.Setup(r => r.GetByIdAsync(invalidId))
            .ReturnsAsync((ShippingContainer)null!);

        // Act
        var result = await _controller.GetContainer(invalidId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetContainer_WhenRepositoryThrowsException_ShouldReturn500()
    {
        // Arrange
        _mockShippingContainerRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetContainer(Guid.NewGuid());

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetContainerById_WithValidContainerId_ShouldReturnOkWithContainer()
    {
        // Arrange
        const string containerId = "CONTAINER123";
        _mockShippingContainerRepository.Setup(r => r.GetByContainerIdAsync(containerId))
            .ReturnsAsync(_testContainer);

        // Act
        var result = await _controller.GetContainerById(containerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedContainer = Assert.IsType<ShippingContainerDto>(okResult.Value);
        Assert.Equal(_testContainer.ContainerId, returnedContainer.ContainerId);
    }

    [Fact]
    public async Task GetContainerById_WithEmptyContainerId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetContainerById(string.Empty);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetContainerById_WithNullContainerId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetContainerById(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetContainerById_WithWhitespaceContainerId_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.GetContainerById("   ");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetContainerById_WithNonExistentContainerId_ShouldReturnNotFound()
    {
        // Arrange
        const string containerId = "NONEXISTENT";
        _mockShippingContainerRepository.Setup(r => r.GetByContainerIdAsync(containerId))
            .ReturnsAsync((ShippingContainer)null!);

        // Act
        var result = await _controller.GetContainerById(containerId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ImportFromXmlFile_WithValidXmlFile_ShouldReturnCreatedAtAction()
    {
        // Arrange
        const string xmlContent = """
                                  <?xml version="1.0"?>
                                  <Container>
                                      <Id>CONTAINER123</Id>
                                      <ShippingDate>2023-01-01T00:00:00</ShippingDate>
                                      <parcels>
                                          <Parcel>
                                              <Receipient>
                                                  <Name>Test User</Name>
                                                  <Address>
                                                      <Street>Test Street</Street>
                                                      <HouseNumber>123</HouseNumber>
                                                      <PostalCode>12345</PostalCode>
                                                      <City>Test City</City>
                                                  </Address>
                                              </Receipient>
                                              <Weight>5.0</Weight>
                                              <Value>100.0</Value>
                                          </Parcel>
                                      </parcels>
                                  </Container>
                                  """;

        var file = CreateMockFormFile("test.xml", xmlContent);
        _mockXmlImportService.Setup(s => s.ImportContainerFromXmlAsync(xmlContent))
            .ReturnsAsync(_testContainerWithParcelsDto);

        // Act
        var result = await _controller.ImportFromXmlFile(file);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.GetContainer), createdResult.ActionName);
        Assert.Equal(_testContainerWithParcelsDto, createdResult.Value);
    }

    [Fact]
    public async Task ImportFromXmlFile_WithNullFile_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.ImportFromXmlFile(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = badRequestResult.Value;
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ImportFromXmlFile_WithEmptyFile_ShouldReturnBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("test.xml", string.Empty);

        // Act
        var result = await _controller.ImportFromXmlFile(file);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ImportFromXmlFile_WithNonXmlFile_ShouldReturnBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("test.txt", "some content");

        // Act
        var result = await _controller.ImportFromXmlFile(file);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var error = badRequestResult.Value;
        Assert.NotNull(error);
    }

    [Fact]
    public async Task ImportFromXmlFile_WhenXmlImportServiceThrowsInvalidOperationException_ShouldReturnBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("test.xml", "<invalid>xml</invalid>");
        _mockXmlImportService.Setup(s => s.ImportContainerFromXmlAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Invalid XML format"));

        // Act
        var result = await _controller.ImportFromXmlFile(file);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ImportFromXmlFile_WhenXmlImportServiceThrowsGenericException_ShouldReturn500()
    {
        // Arrange
        var file = CreateMockFormFile("test.xml", "<valid>xml</valid>");
        _mockXmlImportService.Setup(s => s.ImportContainerFromXmlAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.ImportFromXmlFile(file);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task ValidateXml_WithValidXmlFile_ShouldReturnOkWithValidationResult()
    {
        // Arrange
        var file = CreateMockFormFile("test.xml", "<valid>xml</valid>");
        _mockXmlImportService.Setup(s => s.ValidateXmlContentAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateXml(file);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ValidateXml_WithInvalidXmlFile_ShouldReturnOkWithInvalidResult()
    {
        // Arrange
        var file = CreateMockFormFile("test.xml", "<invalid>xml");
        _mockXmlImportService.Setup(s => s.ValidateXmlContentAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateXml(file);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task ValidateXml_WithNullFile_ShouldReturnBadRequest()
    {
        // Act
        var result = await _controller.ValidateXml(null!);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ValidateXml_WithNonXmlFile_ShouldReturnBadRequest()
    {
        // Arrange
        var file = CreateMockFormFile("test.pdf", "pdf content");

        // Act
        var result = await _controller.ValidateXml(file);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ProcessContainer_WithValidContainerId_ShouldReturnOkWithProcessedParcels()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        var processedParcels = new List<ParcelDto>();
        _mockParcelProcessingService.Setup(s => s.ProcessContainerAsync(containerId))
            .ReturnsAsync(processedParcels);

        // Act
        var result = await _controller.ProcessContainer(containerId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(processedParcels, okResult.Value);
    }

    [Fact]
    public async Task ProcessContainer_WithInvalidContainerId_ShouldReturnNotFound()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        _mockParcelProcessingService.Setup(s => s.ProcessContainerAsync(containerId))
            .ThrowsAsync(new ArgumentException("Container not found"));

        // Act
        var result = await _controller.ProcessContainer(containerId);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task ProcessContainer_WhenServiceThrowsGenericException_ShouldReturn500()
    {
        // Arrange
        var containerId = Guid.NewGuid();
        _mockParcelProcessingService.Setup(s => s.ProcessContainerAsync(containerId))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.ProcessContainer(containerId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}