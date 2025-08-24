using System.Xml;
using System.Xml.Serialization;
using Application.DTOs;
using Application.Models;
using Domain.Constants;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
///     Service responsible for importing shipping containers from XML files
/// </summary>
/// <param name="shippingContainerRepository"></param>
/// <param name="parcelRepository"></param>
/// <param name="departmentRepository"></param>
/// <param name="logger"></param>
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

    /// <summary>
    ///     Validates the XML content and returns true if it is valid, false otherwise
    /// </summary>
    /// <param name="xmlContent"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Imports a shipping container from XML content
    /// </summary>
    /// <param name="xmlContent"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
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
                container.ContainerId, container.Parcels.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import container from XML");
            throw new InvalidOperationException($"Failed to import container from XML: {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Imports a shipping container from a file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
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
        // Check if container already exists with comprehensive validation
        var existingContainer = await _shippingContainerRepository.GetByContainerIdAsync(containerXml.Id);
        if (existingContainer != null)
        {
            _logger.LogInformation(
                "Container with ID {ContainerId} already exists. Validating integrity...",
                containerXml.Id);

            // Validate if existing container matches the import data
            var integrityResult = await ValidateContainerIntegrityAsync(existingContainer, containerXml);

            if (integrityResult.IsValid)
            {
                _logger.LogInformation(
                    "Container {ContainerId} integrity validated successfully. Returning existing container with {ParcelCount} parcels.",
                    containerXml.Id, existingContainer.Parcels.Count);
                return existingContainer;
            }

            _logger.LogWarning(
                "Container {ContainerId} exists but has integrity issues: {Issues}. Creating new version.",
                containerXml.Id, string.Join(", ", integrityResult.Issues));

            // For safety, we could either:
            // 1. Throw an exception (strict mode)
            // 2. Update the existing container (merge mode) 
            // 3. Create with different ID (versioning mode)

            // Using strict mode for data integrity
            throw new InvalidOperationException(
                $"Container {containerXml.Id} already exists but with different data. " +
                $"Issues found: {string.Join(", ", integrityResult.Issues)}");
        }

        _logger.LogInformation("Creating new container with ID {ContainerId}", containerXml.Id);

        // Create container with transaction-like behavior
        var container = new ShippingContainer(containerXml.Id, containerXml.ShippingDate);
        container = await _shippingContainerRepository.AddAsync(container);

        try
        {
            // Process parcels with duplicate detection
            var processedParcels = await ProcessParcelsWithDuplicateDetectionAsync(containerXml.Parcels);

            foreach (var parcel in processedParcels) container.AddParcel(parcel);

            // Update container with parcels
            await _shippingContainerRepository.UpdateAsync(container);

            _logger.LogInformation(
                "Successfully created container {ContainerId} with {ParcelCount} unique parcels",
                container.ContainerId, container.Parcels.Count);

            return container;
        }
        catch (Exception ex)
        {
            // Rollback: remove the container if parcel processing fails
            _logger.LogError(ex, "Failed to process parcels for container {ContainerId}. Rolling back.",
                container.ContainerId);

            try
            {
                await _shippingContainerRepository.DeleteAsync(container.Id);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to rollback container {ContainerId}", container.ContainerId);
            }

            throw;
        }
    }

    private async Task<ContainerIntegrityResult> ValidateContainerIntegrityAsync(
        ShippingContainer existingContainer,
        ContainerXml containerXml)
    {
        var issues = new List<string>();

        // Validate shipping date
        if (existingContainer.ShippingDate != containerXml.ShippingDate)
            issues.Add(
                $"Shipping date mismatch: existing={existingContainer.ShippingDate:yyyy-MM-dd}, new={containerXml.ShippingDate:yyyy-MM-dd}");

        // Validate parcel count
        if (existingContainer.Parcels.Count != containerXml.Parcels.Count)
            issues.Add(
                $"Parcel count mismatch: existing={existingContainer.Parcels.Count}, new={containerXml.Parcels.Count}");
        else
            // Deep validation of parcels if counts match
            await ValidateParcelIntegrityAsync(existingContainer.Parcels, containerXml.Parcels, issues);

        return new ContainerIntegrityResult(issues.Count == 0, issues);
    }

    private async Task ValidateParcelIntegrityAsync(
        IEnumerable<Parcel> existingParcels,
        IEnumerable<ParcelXml> xmlParcels,
        List<string> issues)
    {
        await Task.CompletedTask;

        var existingParcelsList = existingParcels.ToList();
        var xmlParcelsList = xmlParcels.ToList();

        for (var i = 0; i < existingParcelsList.Count; i++)
        {
            var existing = existingParcelsList[i];
            var xml = xmlParcelsList[i];

            if (Math.Abs(existing.Weight - xml.Weight) > 0.01m)
                issues.Add($"Parcel {i + 1} weight mismatch: existing={existing.Weight}, new={xml.Weight}");

            if (Math.Abs(existing.Value - xml.Value) > 0.01m)
                issues.Add($"Parcel {i + 1} value mismatch: existing={existing.Value}, new={xml.Value}");

            if (existing.Recipient.Name != xml.Recipient.Name)
                issues.Add(
                    $"Parcel {i + 1} recipient name mismatch: existing={existing.Recipient.Name}, new={xml.Recipient.Name}");
        }
    }

    private async Task<List<Parcel>> ProcessParcelsWithDuplicateDetectionAsync(IEnumerable<ParcelXml> parcelXmls)
    {
        var processedParcels = new List<Parcel>();
        var parcelSignatures = new HashSet<string>();
        var duplicateCount = 0;

        foreach (var parcelXml in parcelXmls)
        {
            // Create a signature for duplicate detection
            var signature = CreateParcelSignature(parcelXml);

            if (parcelSignatures.Contains(signature))
            {
                duplicateCount++;
                _logger.LogWarning(
                    "Duplicate parcel detected and skipped: {RecipientName}, Weight: {Weight}, Value: {Value}",
                    parcelXml.Recipient.Name, parcelXml.Weight, parcelXml.Value);
                continue;
            }

            parcelSignatures.Add(signature);
            var parcel = await CreateParcelFromXmlAsync(parcelXml);
            processedParcels.Add(parcel);
        }

        if (duplicateCount > 0)
            _logger.LogInformation(
                "Processed {ProcessedCount} unique parcels, skipped {DuplicateCount} duplicates",
                processedParcels.Count, duplicateCount);

        return processedParcels;
    }

    private static string CreateParcelSignature(ParcelXml parcelXml)
    {
        // Create a deterministic signature for duplicate detection
        return $"{parcelXml.Recipient.Name}|{parcelXml.Weight}|{parcelXml.Value}|" +
               $"{parcelXml.Recipient.Address.Street}|{parcelXml.Recipient.Address.HouseNumber}|" +
               $"{parcelXml.Recipient.Address.PostalCode}|{parcelXml.Recipient.Address.City}";
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

        // Assign departments to parcel
        foreach (var department in departments)
            parcel.AssignDepartment(department);

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