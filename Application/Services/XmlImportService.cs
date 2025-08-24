using System.Xml;
using System.Xml.Serialization;
using Application.DTOs;
using Application.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class XmlImportService(
    IShippingContainerRepository shippingContainerRepository,
    IParcelRepository parcelRepository,
    IDepartmentRepository departmentRepository,
    ILogger<XmlImportService> logger)
    : IXmlImportService
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    private readonly ILogger<XmlImportService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IParcelRepository _parcelRepository =
        parcelRepository ?? throw new ArgumentNullException(nameof(parcelRepository));

    private readonly IShippingContainerRepository _shippingContainerRepository =
        shippingContainerRepository ?? throw new ArgumentNullException(nameof(shippingContainerRepository));

    public Task<bool> ValidateXmlContentAsync(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            _logger.LogWarning("XML content validation failed: content is null or empty");
            return Task.FromResult(false);
        }

        try
        {
            _logger.LogDebug("Starting XML content validation");
            var containerXml = DeserializeXml(xmlContent);
            var isValid = !string.IsNullOrWhiteSpace(containerXml.Id) && containerXml.Parcels.Count != 0;

            _logger.LogInformation(
                "XML content validation completed. Valid: {IsValid}, ContainerId: {ContainerId}, ParcelCount: {ParcelCount}",
                isValid, containerXml.Id, containerXml.Parcels.Count);

            return Task.FromResult(isValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "XML content validation failed due to parsing error");
            return Task.FromResult(false);
        }
    }

    public async Task<ShippingContainerWithParcelsDto> ImportContainerFromXmlAsync(string xmlContent)
    {
        if (string.IsNullOrWhiteSpace(xmlContent))
        {
            _logger.LogError("Import failed: XML content cannot be empty");
            throw new ArgumentException("XML content cannot be empty", nameof(xmlContent));
        }

        try
        {
            _logger.LogInformation("Starting container import from XML");

            var containerXml = DeserializeXml(xmlContent);
            _logger.LogDebug("XML deserialized successfully. ContainerId: {ContainerId}, ParcelCount: {ParcelCount}",
                containerXml.Id, containerXml.Parcels.Count);

            var container = await CreateContainerFromXmlAsync(containerXml);
            var result = MapToShippingContainerWithParcelsDto(container);

            _logger.LogInformation(
                "Container import completed successfully. ContainerId: {ContainerId}, ImportedParcels: {ParcelCount}",
                container.Id, container.Parcels.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import container from XML");
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
        parcel = await _parcelRepository.AddAsync(parcel);

        // Determine and assign appropriate departments based on business rules
        await AssignDepartmentsToParcelAsync(parcel);

        return parcel;
    }

    private async Task AssignDepartmentsToParcelAsync(Parcel parcel)
    {
        var departments = new List<Department>();

        switch (parcel.Weight)
        {
            // Weight-based department assignment
            case <= 1:
            {
                var mailDept = await _departmentRepository.GetByNameAsync(DefaultDepartmentNames.Mail);
                if (mailDept != null) departments.Add(mailDept);
                break;
            }
            case <= 10:
            {
                var regularDept = await _departmentRepository.GetByNameAsync(DefaultDepartmentNames.Regular);
                if (regularDept != null) departments.Add(regularDept);
                break;
            }
            default:
            {
                var heavyDept = await _departmentRepository.GetByNameAsync(DefaultDepartmentNames.Heavy);
                if (heavyDept != null) departments.Add(heavyDept);
                break;
            }
        }

        // Value-based department assignment (Insurance)
        if (parcel.Value > 1000)
        {
            var insuranceDept = await _departmentRepository.GetByNameAsync(DefaultDepartmentNames.Insurance);
            if (insuranceDept != null) departments.Add(insuranceDept);
        }

        // Assign departments to parcel (using correct method name)
        foreach (var department in departments) parcel.AssignDepartment(department);

        _logger.LogInformation(
            "Assigned parcel {ParcelId} to departments: {Departments}",
            parcel.Id,
            string.Join(", ", departments.Select(d => d.Name))
        );
    }

    private static ShippingContainerWithParcelsDto MapToShippingContainerWithParcelsDto(ShippingContainer container)
    {
        return new ShippingContainerWithParcelsDto(
            container.Id,
            container.ContainerId,
            container.ShippingDate,
            container.Status,
            container.Parcels.Select(MapToParcelDto),
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