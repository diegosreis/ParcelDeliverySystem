using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class XmlImportServiceDuplicationTests
{
    private const string ValidXml = """
                                    <?xml version="1.0"?>
                                    <Container xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                      xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                                      <Id>68465468</Id>
                                      <ShippingDate>2016-07-22T00:00:00+02:00</ShippingDate>
                                      <parcels>
                                        <Parcel>
                                          <Receipient>
                                            <Name>John Doe</Name>
                                            <Address>
                                              <Street>Main Street</Street>
                                              <HouseNumber>123</HouseNumber>
                                              <PostalCode>1234AB</PostalCode>
                                              <City>Test City</City>
                                            </Address>
                                          </Receipient>
                                          <Weight>1.5</Weight>
                                          <Value>100.0</Value>
                                        </Parcel>
                                      </parcels>
                                    </Container>
                                    """;

    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly Mock<IDepartmentRuleService> _mockDepartmentRuleService;
    private readonly Mock<ILogger<XmlImportService>> _mockLogger;
    private readonly Mock<IParcelRepository> _mockParcelRepository;
    private readonly Mock<IShippingContainerRepository> _mockShippingContainerRepository;
    private readonly XmlImportService _service;

    public XmlImportServiceDuplicationTests()
    {
        _mockShippingContainerRepository = new Mock<IShippingContainerRepository>();
        _mockParcelRepository = new Mock<IParcelRepository>();
        _mockDepartmentRepository = new Mock<IDepartmentRepository>();
        _mockDepartmentRuleService = new Mock<IDepartmentRuleService>();
        _mockLogger = new Mock<ILogger<XmlImportService>>();

        _service = new XmlImportService(
            _mockShippingContainerRepository.Object,
            _mockParcelRepository.Object,
            _mockDepartmentRepository.Object,
            _mockDepartmentRuleService.Object,
            _mockLogger.Object);

        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        _mockShippingContainerRepository
            .Setup(r => r.GetByContainerIdAsync(It.IsAny<string>()))
            .ReturnsAsync((ShippingContainer?)null);

        _mockShippingContainerRepository
            .Setup(r => r.AddAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync((ShippingContainer container) => container);

        _mockShippingContainerRepository
            .Setup(r => r.UpdateAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync((ShippingContainer container) => container);

        _mockParcelRepository
            .Setup(r => r.AddAsync(It.IsAny<Parcel>()))
            .ReturnsAsync((Parcel parcel) => parcel);

        // Setup departments
        var mailDept = new Department("Mail", "Mail department");
        var regularDept = new Department("Regular", "Regular department");
        var heavyDept = new Department("Heavy", "Heavy department");
        var insuranceDept = new Department("Insurance", "Insurance department");

        _mockDepartmentRepository.Setup(r => r.GetByNameAsync("Mail")).ReturnsAsync(mailDept);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync("Regular")).ReturnsAsync(regularDept);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync("Heavy")).ReturnsAsync(heavyDept);
        _mockDepartmentRepository.Setup(r => r.GetByNameAsync("Insurance")).ReturnsAsync(insuranceDept);

        // Setup department rule service to return empty list for tests
        _mockDepartmentRuleService.Setup(x => x.DetermineDepartmentsAsync(It.IsAny<Parcel>()))
            .ReturnsAsync([]);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WhenContainerExists_ShouldReturnExistingContainer()
    {
        // Arrange
        var existingContainer = new ShippingContainer("68465468", new DateTime(2016, 7, 22));
        var existingParcel = new Parcel(
            new Customer("John Doe",
                new Address("Main Street", "123", "", "Default", "Test City", "NL", "1234AB", "Netherlands")),
            1.5m, 100.0m);
        existingContainer.AddParcel(existingParcel);

        _mockShippingContainerRepository
            .Setup(r => r.GetByContainerIdAsync("68465468"))
            .ReturnsAsync(existingContainer);

        // Act
        var result = await _service.ImportContainerFromXmlAsync(ValidXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("68465468", result.ContainerId);
        Assert.Equal(1, result.TotalParcels);

        // Verify that no new container was created
        _mockShippingContainerRepository.Verify(r => r.AddAsync(It.IsAny<ShippingContainer>()), Times.Never);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WhenContainerExistsWithDifferentData_ShouldThrowException()
    {
        // Arrange
        var existingContainer = new ShippingContainer("68465468", new DateTime(2020, 1, 1)); // Different date
        var existingParcel = new Parcel(
            new Customer("John Doe",
                new Address("Main Street", "123", "", "Default", "Test City", "NL", "1234AB", "Netherlands")),
            1.5m, 100.0m);
        existingContainer.AddParcel(existingParcel);

        _mockShippingContainerRepository
            .Setup(r => r.GetByContainerIdAsync("68465468"))
            .ReturnsAsync(existingContainer);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ImportContainerFromXmlAsync(ValidXml));

        Assert.Contains("already exists but with different data", exception.Message);
        Assert.Contains("Shipping date mismatch", exception.Message);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WithDuplicateParcels_ShouldSkipDuplicates()
    {
        // Arrange
        const string xmlWithDuplicates = """
                                         <?xml version="1.0"?>
                                         <Container xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                           xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                                           <Id>68465468</Id>
                                           <ShippingDate>2016-07-22T00:00:00+02:00</ShippingDate>
                                           <parcels>
                                             <Parcel>
                                               <Receipient>
                                                 <Name>John Doe</Name>
                                                 <Address>
                                                   <Street>Main Street</Street>
                                                   <HouseNumber>123</HouseNumber>
                                                   <PostalCode>1234AB</PostalCode>
                                                   <City>Test City</City>
                                                 </Address>
                                               </Receipient>
                                               <Weight>1.5</Weight>
                                               <Value>100.0</Value>
                                             </Parcel>
                                             <Parcel>
                                               <Receipient>
                                                 <Name>John Doe</Name>
                                                 <Address>
                                                   <Street>Main Street</Street>
                                                   <HouseNumber>123</HouseNumber>
                                                   <PostalCode>1234AB</PostalCode>
                                                   <City>Test City</City>
                                                 </Address>
                                               </Receipient>
                                               <Weight>1.5</Weight>
                                               <Value>100.0</Value>
                                             </Parcel>
                                             <Parcel>
                                               <Receipient>
                                                 <Name>Jane Smith</Name>
                                                 <Address>
                                                   <Street>Second Street</Street>
                                                   <HouseNumber>456</HouseNumber>
                                                   <PostalCode>5678CD</PostalCode>
                                                   <City>Another City</City>
                                                 </Address>
                                               </Receipient>
                                               <Weight>2.0</Weight>
                                               <Value>200.0</Value>
                                             </Parcel>
                                           </parcels>
                                         </Container>
                                         """;

        // Act
        var result = await _service.ImportContainerFromXmlAsync(xmlWithDuplicates);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("68465468", result.ContainerId);
        Assert.Equal(2, result.TotalParcels);

        // Verify that only 2 parcels were added to repository
        _mockParcelRepository.Verify(r => r.AddAsync(It.IsAny<Parcel>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WhenParcelProcessingFails_ShouldRollbackContainer()
    {
        // Arrange
        var addedContainer = new ShippingContainer("68465468", new DateTime(2016, 7, 22));

        _mockShippingContainerRepository
            .Setup(r => r.AddAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync(addedContainer);

        _mockParcelRepository
            .Setup(r => r.AddAsync(It.IsAny<Parcel>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ImportContainerFromXmlAsync(ValidXml));

        Assert.Contains("Failed to import container from XML", exception.Message);

        // Verify rollback was attempted
        _mockShippingContainerRepository.Verify(r => r.DeleteAsync(addedContainer.Id), Times.Once);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_WhenContainerExistsWithSameParcelCount_ShouldValidateParcelDetails()
    {
        // Arrange
        var existingContainer = new ShippingContainer("68465468", new DateTime(2016, 7, 22));
        var existingParcel = new Parcel(
            new Customer("Different Name", // Different name should cause validation failure
                new Address("Main Street", "123", "", "Default", "Test City", "NL", "1234AB", "Netherlands")),
            1.5m, 100.0m);
        existingContainer.AddParcel(existingParcel);

        _mockShippingContainerRepository
            .Setup(r => r.GetByContainerIdAsync("68465468"))
            .ReturnsAsync(existingContainer);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.ImportContainerFromXmlAsync(ValidXml));

        Assert.Contains("already exists but with different data", exception.Message);
        Assert.Contains("recipient name mismatch", exception.Message);
    }

    [Fact]
    public async Task ImportContainerFromXmlAsync_NewContainer_ShouldCreateSuccessfully()
    {
        // Act
        var result = await _service.ImportContainerFromXmlAsync(ValidXml);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("68465468", result.ContainerId);
        Assert.Equal(1, result.TotalParcels);

        // Verify container and parcel were created
        _mockShippingContainerRepository.Verify(r => r.AddAsync(It.IsAny<ShippingContainer>()), Times.Once);
        _mockShippingContainerRepository.Verify(r => r.UpdateAsync(It.IsAny<ShippingContainer>()), Times.Once);
        _mockParcelRepository.Verify(r => r.AddAsync(It.IsAny<Parcel>()), Times.Once);
    }
}