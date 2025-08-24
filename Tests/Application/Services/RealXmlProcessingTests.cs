using System.Xml.Serialization;
using Application.Models;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class RealXmlProcessingTests
{
    private readonly Mock<IDepartmentRepository> _mockDepartmentRepository;
    private readonly Mock<ILogger<XmlImportService>> _mockLogger;
    private readonly Mock<IParcelRepository> _mockParcelRepository;
    private readonly Mock<IShippingContainerRepository> _mockShippingContainerRepository;
    private readonly XmlImportService _xmlImportService;

    public RealXmlProcessingTests()
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
    public async Task ProcessRealXmlFile_ShouldParseCorrectly()
    {
        // Arrange
        var xmlFilePath = "/home/diego/RiderProjects/ParcelDeliverySystem/Container_68465468.xml";

        if (!File.Exists(xmlFilePath))
        {
            Assert.Fail("Test XML file not found");
            return;
        }

        var xmlContent = await File.ReadAllTextAsync(xmlFilePath);

        _mockShippingContainerRepository.Setup(x => x.GetByContainerIdAsync("68465468"))
            .ReturnsAsync((ShippingContainer)null!);
        _mockShippingContainerRepository.Setup(x => x.AddAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync((ShippingContainer container) => container);
        _mockShippingContainerRepository.Setup(x => x.UpdateAsync(It.IsAny<ShippingContainer>()))
            .ReturnsAsync((ShippingContainer container) => container);
        _mockParcelRepository.Setup(x => x.AddAsync(It.IsAny<Parcel>()))
            .ReturnsAsync((Parcel parcel) => parcel);

        // Act & Assert - Test XML parsing first
        var serializer = new XmlSerializer(typeof(ContainerXml));
        ContainerXml containerXml;

        using (var reader = new StringReader(xmlContent))
        {
            containerXml = (ContainerXml)serializer.Deserialize(reader)!;
        }

        // Verify parsing results
        Assert.NotNull(containerXml);
        Assert.Equal("68465468", containerXml.Id);
        Assert.Equal(new DateTime(2016, 7, 22), containerXml.ShippingDate.Date);
        Assert.True(containerXml.Parcels.Count >= 4); // Should have at least 4 parcels from our sample

        // Verify first parcel
        var firstParcel = containerXml.Parcels.First();
        Assert.Equal("Vinny Gankema", firstParcel.Recipient.Name);
        Assert.Equal("Marijkestraat", firstParcel.Recipient.Address.Street);
        Assert.Equal("28", firstParcel.Recipient.Address.HouseNumber);
        Assert.Equal("4744AT", firstParcel.Recipient.Address.PostalCode);
        Assert.Equal("Bosschenhoofd", firstParcel.Recipient.Address.City);
        Assert.Equal(0.02m, firstParcel.Weight);
        Assert.Equal(0.0m, firstParcel.Value);

        // Test service integration
        var result = await _xmlImportService.ImportContainerFromXmlAsync(xmlContent);

        Assert.NotNull(result);
        Assert.Equal("68465468", result.ContainerId);
    }

    [Fact]
    public void TestXmlDeserialization_DirectTest()
    {
        // Arrange
        const string sampleXml = """
                                 <?xml version="1.0"?>
                                 <Container xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
                                   xmlns:xsd="http://www.w3.org/2001/XMLSchema">
                                   <Id>68465468</Id>
                                   <ShippingDate>2016-07-22T00:00:00+02:00</ShippingDate>
                                   <parcels>
                                     <Parcel>
                                       <Receipient>
                                         <Name>Vinny Gankema</Name>
                                         <Address>
                                           <Street>Marijkestraat</Street>
                                           <HouseNumber>28</HouseNumber>
                                           <PostalCode>4744AT</PostalCode>
                                           <City>Bosschenhoofd</City>
                                         </Address>
                                       </Receipient>
                                       <Weight>0.02</Weight>
                                       <Value>0.0</Value>
                                     </Parcel>
                                   </parcels>
                                 </Container>
                                 """;

        // Act
        var serializer = new XmlSerializer(typeof(ContainerXml));
        ContainerXml result;

        using (var reader = new StringReader(sampleXml))
        {
            result = (ContainerXml)serializer.Deserialize(reader)!;
        }

        // Assert
        Assert.NotNull(result);
        Assert.Equal("68465468", result.Id);
        Assert.Single(result.Parcels);
        Assert.Equal("Vinny Gankema", result.Parcels[0].Recipient.Name);
    }
}