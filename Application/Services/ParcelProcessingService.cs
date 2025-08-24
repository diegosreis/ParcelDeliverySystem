using Application.DTOs;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ParcelProcessingService(
    IParcelRepository parcelRepository,
    IShippingContainerRepository shippingContainerRepository,
    IDepartmentRepository departmentRepository,
    IDepartmentRuleService departmentRuleService,
    ILogger<ParcelProcessingService> logger)
    : IParcelProcessingService
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    private readonly IDepartmentRuleService _departmentRuleService =
        departmentRuleService ?? throw new ArgumentNullException(nameof(departmentRuleService));

    private readonly ILogger<ParcelProcessingService> _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IParcelRepository _parcelRepository =
        parcelRepository ?? throw new ArgumentNullException(nameof(parcelRepository));

    private readonly IShippingContainerRepository _shippingContainerRepository =
        shippingContainerRepository ?? throw new ArgumentNullException(nameof(shippingContainerRepository));

    public async Task<ParcelDto> ProcessParcelAsync(Guid parcelId)
    {
        _logger.LogInformation("Starting parcel processing for ParcelId: {ParcelId}", parcelId);

        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        if (parcel == null)
        {
            _logger.LogError("Parcel processing failed: Parcel with ID {ParcelId} not found", parcelId);
            throw new ArgumentException($"Parcel with ID {parcelId} not found");
        }

        try
        {
            _logger.LogDebug("Updating parcel status to Processing for ParcelId: {ParcelId}", parcelId);
            parcel.UpdateStatus(ParcelStatus.Processing);

            if (parcel.RequiresInsuranceApproval)
            {
                _logger.LogInformation("Parcel requires insurance approval. ParcelId: {ParcelId}, Value: {Value}",
                    parcelId, parcel.Value);

                parcel.UpdateStatus(ParcelStatus.InsuranceApprovalRequired);
                var insuranceDept = await _departmentRepository.GetByNameAsync(DefaultDepartmentNames.Insurance);
                if (insuranceDept != null)
                {
                    parcel.AssignDepartment(insuranceDept);
                    _logger.LogInformation("Assigned insurance department to parcel. ParcelId: {ParcelId}", parcelId);
                }
            }
            else
            {
                _logger.LogDebug("Assigning departments based on business rules for ParcelId: {ParcelId}", parcelId);
                await AssignDepartmentsByRulesAsync(parcel);
                parcel.UpdateStatus(ParcelStatus.AssignedToDepartment);
                _logger.LogInformation("Parcel assigned to departments successfully. ParcelId: {ParcelId}", parcelId);
            }

            var updatedParcel = await _parcelRepository.UpdateAsync(parcel);
            _logger.LogInformation(
                "Parcel processing completed successfully. ParcelId: {ParcelId}, FinalStatus: {Status}",
                parcelId, updatedParcel.Status);

            return MapToParcelDto(updatedParcel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process parcel. ParcelId: {ParcelId}", parcelId);
            throw;
        }
    }

    public async Task<IEnumerable<ParcelDto>> ProcessContainerAsync(Guid containerId)
    {
        _logger.LogInformation("Starting container processing for ContainerId: {ContainerId}", containerId);

        var container = await _shippingContainerRepository.GetByIdAsync(containerId);
        if (container == null)
        {
            _logger.LogError("Container processing failed: Container with ID {ContainerId} not found", containerId);
            throw new ArgumentException($"Container with ID {containerId} not found");
        }

        try
        {
            _logger.LogInformation("Processing {ParcelCount} parcels in container. ContainerId: {ContainerId}",
                container.Parcels.Count, containerId);

            var processedParcels = await Task.WhenAll(
                container.Parcels.Select(async parcel => await ProcessParcelAsync(parcel.Id))
            );

            _logger.LogInformation(
                "Container processing completed successfully. ContainerId: {ContainerId}, ProcessedParcels: {Count}",
                containerId, processedParcels.Length);

            return processedParcels;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process container. ContainerId: {ContainerId}", containerId);
            throw;
        }
    }

    public async Task<ParcelDto> AssignDepartmentAsync(Guid parcelId, Guid departmentId)
    {
        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        if (parcel == null)
            throw new ArgumentException($"Parcel with ID {parcelId} not found");

        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null)
            throw new ArgumentException($"Department with ID {departmentId} not found");

        parcel.AssignDepartment(department);
        var updatedParcel = await _parcelRepository.UpdateAsync(parcel);

        return MapToParcelDto(updatedParcel);
    }

    public async Task<ParcelDto> RemoveDepartmentAsync(Guid parcelId, Guid departmentId)
    {
        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        if (parcel == null)
            throw new ArgumentException($"Parcel with ID {parcelId} not found");

        var department = await _departmentRepository.GetByIdAsync(departmentId);
        if (department == null)
            throw new ArgumentException($"Department with ID {departmentId} not found");

        parcel.RemoveDepartment(department);
        var updatedParcel = await _parcelRepository.UpdateAsync(parcel);

        return MapToParcelDto(updatedParcel);
    }

    public async Task<ParcelDto> UpdateParcelStatusAsync(Guid parcelId, ParcelStatus status)
    {
        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        if (parcel == null)
            throw new ArgumentException($"Parcel with ID {parcelId} not found");

        parcel.UpdateStatus(status);
        var updatedParcel = await _parcelRepository.UpdateAsync(parcel);

        return MapToParcelDto(updatedParcel);
    }

    public async Task<IEnumerable<DepartmentDto>> GetAssignedDepartmentsAsync(Guid parcelId)
    {
        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        return parcel == null
            ? throw new ArgumentException($"Parcel with ID {parcelId} not found")
            : parcel.AssignedDepartments.Select(MapToDepartmentDto);
    }

    private async Task AssignDepartmentsByRulesAsync(Parcel parcel)
    {
        // Clear existing departments
        var existingDepartments = parcel.AssignedDepartments.ToList();
        foreach (var dept in existingDepartments) parcel.RemoveDepartment(dept);

        // Use DepartmentRuleService to determine required departments
        var requiredDepartments = await _departmentRuleService.DetermineRequiredDepartmentsAsync(parcel.Id);

        foreach (var deptDto in requiredDepartments)
        {
            var department = await _departmentRepository.GetByIdAsync(deptDto.Id);
            if (department != null)
                parcel.AssignDepartment(department);
        }
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