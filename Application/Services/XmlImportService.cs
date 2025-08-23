using System.Xml;
using System.Xml.Serialization;
using Application.DTOs;
using Application.Models;
using Domain.Entities;
using Domain.Interfaces;

namespace Application.Services;

public class XmlImportService(
    IShippingContainerRepository shippingContainerRepository,
    IParcelRepository parcelRepository,
    IDepartmentRepository departmentRepository)
    : IXmlImportService
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    private readonly IParcelRepository _parcelRepository =
        parcelRepository ?? throw new ArgumentNullException(nameof(parcelRepository));

    private readonly IShippingContainerRepository _shippingContainerRepository =
        shippingContainerRepository ?? throw new ArgumentNullException(nameof(shippingContainerRepository));

    public Task<bool> ValidateXmlContentAsync(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
            return Task.FromResult(false);

        try
        {
            var containerXml = DeserializeXml(xmlContent);
            return Task.FromResult(!string.IsNullOrWhiteSpace(containerXml.Id) &&
                                   containerXml.Parcels.Count != 0);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public async Task<ShippingContainerWithParcelsDto> ImportContainerFromXmlAsync(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
            throw new ArgumentException("XML content cannot be empty", nameof(xmlContent));

        try
        {
            var containerXml = DeserializeXml(xmlContent);
            var container = await CreateContainerFromXmlAsync(containerXml);

            return MapToContainerWithParcelsDto(container);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to import container from XML: {ex.Message}", ex);
        }
    }

    public async Task<ShippingContainerWithParcelsDto> ImportContainerFromFileAsync(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be empty", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var xmlContent = await File.ReadAllTextAsync(filePath);
        return await ImportContainerFromXmlAsync(xmlContent);
    }

    private static ContainerXml DeserializeXml(string xmlContent)
    {
        var serializer = new XmlSerializer(typeof(ContainerXml));
        using var reader = new StringReader(xmlContent);
        try
        {
            return (ContainerXml)serializer.Deserialize(reader)!;
        }
        catch (Exception ex)
        {
            throw new XmlException("Error processing XML", ex);
        }
    }

    private async Task<ShippingContainer> CreateContainerFromXmlAsync(ContainerXml containerXml)
    {
        // Check if container already exists
        var existingContainer = await _shippingContainerRepository.GetByContainerIdAsync(containerXml.Id);
        if (existingContainer != null)
            throw new InvalidOperationException($"Container with ID {containerXml.Id} already exists");

        // Create container
        var container = new ShippingContainer(containerXml.Id, containerXml.ShippingDate);
        container = await _shippingContainerRepository.AddAsync(container);

        // Process parcels
        foreach (var parcelXml in containerXml.Parcels)
        {
            var parcel = await CreateParcelFromXmlAsync(parcelXml);
            container.AddParcel(parcel);
        }

        // Update container with parcels
        await _shippingContainerRepository.UpdateAsync(container);

        return container;
    }

    private async Task<Parcel> CreateParcelFromXmlAsync(ParcelXml parcelXml)
    {
        var address = new Address(
            parcelXml.Recipient.Address.Street,
            parcelXml.Recipient.Address.HouseNumber,
            string.Empty,
            "Default",
            parcelXml.Recipient.Address.City,
            "NL",
            parcelXml.Recipient.Address.PostalCode,
            "Netherlands"
        );

        // Create customer
        var customer = new Customer(parcelXml.Recipient.Name, address);

        // Create parcel
        var parcel = new Parcel(customer, parcelXml.Weight, parcelXml.Value);

        return await _parcelRepository.AddAsync(parcel);
    }

    private static ShippingContainerWithParcelsDto MapToContainerWithParcelsDto(ShippingContainer container)
    {
        return new ShippingContainerWithParcelsDto(
            container.Id,
            container.ContainerId,
            container.ShippingDate,
            container.Status,
            container.Parcels.Select(MapToParcelDto).ToList(),
            container.TotalParcels,
            container.TotalWeight,
            container.TotalValue,
            container.ParcelsRequiringInsurance,
            container.CreatedAt,
            container.UpdatedAt
        );
    }

    private static ParcelDto MapToParcelDto(Parcel parcel)
    {
        return new ParcelDto(
            parcel.Id,
            MapToCustomerDto(parcel.Recipient),
            parcel.Weight,
            parcel.Value,
            parcel.Status,
            parcel.AssignedDepartments.Select(MapToDepartmentDto),
            parcel.CreatedAt,
            parcel.UpdatedAt
        );
    }

    private static CustomerDto MapToCustomerDto(Customer customer)
    {
        return new CustomerDto(
            customer.Id,
            customer.Name,
            MapToAddressDto(customer.Address),
            customer.CreatedAt,
            customer.UpdatedAt
        );
    }

    private static AddressDto MapToAddressDto(Address address)
    {
        return new AddressDto(
            address.Id,
            address.Street,
            address.Number,
            address.Complement,
            address.Neighborhood,
            address.City,
            address.State,
            address.ZipCode,
            address.Country,
            address.CreatedAt,
            address.UpdatedAt
        );
    }

    private static DepartmentDto MapToDepartmentDto(Department department)
    {
        return new DepartmentDto(
            department.Id,
            department.Name,
            department.Description,
            department.IsActive,
            department.CreatedAt,
            department.UpdatedAt
        );
    }
}