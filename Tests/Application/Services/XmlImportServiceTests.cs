using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class XmlImportServiceTests
{
    private const string ValidXmlContent = """
                                           <?xml version="1.0" encoding="UTF-8"?>
                                           <Container>
                                               <Id>Container_68465468</Id>
                                               <ShippingDate>2019-03-07T00:00:00</ShippingDate>
                                               <parcels>
                                                   <Parcel>
                                                       <Receipient>
                                                           <Name>John Doe</Name>
                                                           <Address>
                                                               <Street>Main Street</Street>
                                                               <HouseNumber>123</HouseNumber>
                                                               <PostalCode>1234AB</PostalCode>
                                                               <City>Amsterdam</City>
                                                           </Address>
                                                       </Receipient>
                                                       <Weight>5.5</Weight>
                                                       <Value>100.0</Value>
                                                   </Parcel>
                                               </parcels>
                                           </Container>
                                           """;

    private const string InvalidXmlContent = """
                                             <?xml version="1.0" encoding="UTF-8"?>
                                             <Container>
                                                 <Id></Id>
                                                 <ShippingDate>2019-03-07T00:00:00</ShippingDate>
                                                 <parcels>
                                                 </parcels>
                                             </Container>
                                             """;

    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly Mock<ILogger<XmlImportService>> _mockLogger;
    private readonly Mock<IParcelRepository> _mockParcelRepository;
    private readonly Mock<IShippingContainerRepository> _mockShippingContainerRepository;
    private readonly XmlImportService _xmlImportService;

    public XmlImportServiceTests()
    {
        _mockShippingContainerRepository = new Mock<IShippingContainerRepository>();
        _mockParcelRepository = new Mock<IParcelRepository>();
        _mockDepartmentRepository = new Mock<IDepartmentRepository>();
        _mockLogger = new Mock<ILogger<XmlImportService>>();

        _xmlImportService = new XmlImportService(
            _mockShippingContainerRepository.Object,
            _mockParcelRepository.Object,
            _mockDepartmentRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithNullContainerRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new XmlImportService(null!, _mockParcelRepository.Object, _mockDepartmentRepository.Object,
                _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullParcelRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new XmlImportService(_mockShippingContainerRepository.Object, null!, _mockDepartmentRepository.Object,
                _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullDepartmentRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new XmlImportService(_mockShippingContainerRepository.Object, _mockParcelRepository.Object, null!,
                _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new XmlImportService(_mockShippingContainerRepository.Object, _mockParcelRepository.Object,
                _mockDepartmentRepository.Object, null!));
    }

    [Fact]
    public async Task ValidateXmlContentAsync_WithValidXml_ShouldReturnTrue()
    {
        // Act
        var result = await _xmlImportService.ValidateXmlContentAsync(ValidXmlContent);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ValidateXmlContentAsync_WithInvalidXml_ShouldReturnFalse()
    {
        // Act
        var result = await _xmlImportService.ValidateXmlContentAsync(InvalidXmlContent);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateXmlContentAsync_WithEmptyContent_ShouldReturnFalse()
    {
        // Act
        var result = await _xmlImportService.ValidateXmlContentAsync(string.Empty);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateXmlContentAsync_WithNullContent_ShouldReturnFalse()
    {
        // Act
        var result = await _xmlImportService.ValidateXmlContentAsync(null!);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ValidateXmlContentAsync_WithWhitespaceContent_ShouldReturnFalse()
    {
        // Act
        var result = await _xmlImportService.ValidateXmlContentAsync("   ");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WithValidXml_ShouldReturnContainerDto()
    {
        // Arrange
        var testContainer = new ShippingContainer("Container_68465468", new DateTime(2019, 3, 7));

        _mockShippingContainerRepository.Setup(x => x.GetByContainerIdAsync("Container_68465468"))
            .ReturnsAsync((ShippingContainer)null!);
        _mockShippingContainerRepository.Setup(x => x.AddAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync(testContainer);
        _mockShippingContainerRepository.Setup(x => x.UpdateAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync(testContainer);
        _mockParcelRepository.Setup(x => x.AddAsync(It.IsAny<Parcel>()))
            .ReturnsAsync((Parcel parcel) => parcel);

        // Act
        var result = await _xmlImportService.ImportContainerFromXmlAsync(ValidXmlContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Container_68465468", result.ContainerId);
        Assert.Equal(new DateTime(2019, 3, 7), result.ShippingDate);
        Assert.Equal(ShippingContainerStatus.Pending, result.Status);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WithEmptyContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _xmlImportService.ImportContainerFromXmlAsync(string.Empty));
        Assert.Contains("XML content cannot be empty", exception.Message);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WithNullContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _xmlImportService.ImportContainerFromXmlAsync(null!));
        Assert.Contains("XML content cannot be empty", exception.Message);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WithInvalidXml_ShouldThrowInvalidOperationException()
    {
        // Arrange
        const string malformedXml = """
                                    <?xml version="1.0" encoding="UTF-8"?>
                                    <Container>
                                        <Id>Container_123</Id>
                                        <ShippingDate>invalid-date</ShippingDate>
                                        <parcels>
                                            <Parcel>
                                                <Receipient>
                                    """;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _xmlImportService.ImportContainerFromXmlAsync(malformedXml));
        Assert.Contains("Error processing XML", exception.Message);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WithExistingContainer_ShouldReturnExistingContainer()
    {
        // Arrange
        var existingContainer = new ShippingContainer("Container_68465468", new DateTime(2019, 3, 7));

        // Add the same parcel that exists in the XML to ensure integrity validation passes
        var existingParcel = new Parcel(
            new Customer("John Doe",
                new Address("Main Street", "123", "", "Default", "Amsterdam", "NL", "1234AB", "Netherlands")),
            5.5m, 100.0m);
        existingContainer.AddParcel(existingParcel);

        _mockShippingContainerRepository.Setup(x => x.GetByContainerIdAsync("Container_68465468"))
            .ReturnsAsync(existingContainer);

        // Act
        var result = await _xmlImportService.ImportContainerFromXmlAsync(ValidXmlContent);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Container_68465468", result.ContainerId);
        Assert.Single(result.Parcels); // Should return the existing parcel
        // Verify that AddAsync was NOT called since container already exists
        _mockShippingContainerRepository.Verify(x => x.AddAsync(It.IsAny<ShippingContainer>()), Times.Never);
    }

    [Fact]
    public async Task ImportContainerFromFileAsync_WithValidFile_ShouldReturnContainerDto()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, ValidXmlContent);

        var testContainer = new ShippingContainer("Container_68465468", new DateTime(2019, 3, 7));

        _mockShippingContainerRepository.Setup(x => x.GetByContainerIdAsync("Container_68465468"))
            .ReturnsAsync((ShippingContainer)null!);
        _mockShippingContainerRepository.Setup(x => x.AddAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync(testContainer);
        _mockShippingContainerRepository.Setup(x => x.UpdateAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync(testContainer);
        _mockParcelRepository.Setup(x => x.AddAsync(It.IsAny<Parcel>()))
            .ReturnsAsync((Parcel parcel) => parcel);

        try
        {
            // Act
            var result = await _xmlImportService.ImportContainerFromFileAsync(tempFilePath);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Container_68465468", result.ContainerId);
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
        }
    }

    [Fact]
    public async Task ImportContainerFromFileAsync_WithEmptyFilePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _xmlImportService.ImportContainerFromFileAsync(string.Empty));
        Assert.Contains("File path cannot be empty", exception.Message);
    }

    [Fact]
    public async Task ImportContainerFromFileAsync_WithNullFilePath_ShouldThrowArgumentException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _xmlImportService.ImportContainerFromFileAsync(null!));
        Assert.Contains("File path cannot be empty", exception.Message);
    }

    [Fact]
    public async Task ImportContainerFromFileAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = "non-existent-file.xml";

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _xmlImportService.ImportContainerFromFileAsync(nonExistentPath));
        Assert.Contains($"File not found: {nonExistentPath}", exception.Message);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WhenRepositoryThrowsException_ShouldThrowInvalidOperationException()
    {
        // Arrange
        _mockShippingContainerRepository.Setup(x => x.GetByContainerIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _xmlImportService.ImportContainerFromXmlAsync(ValidXmlContent));
        Assert.Contains("Failed to import container from XML", exception.Message);
    }
}