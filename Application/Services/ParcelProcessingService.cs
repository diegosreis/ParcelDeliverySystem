using Application.DTOs;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;

namespace Application.Services;

public class ParcelProcessingService(
    IParcelRepository parcelRepository,
    IShippingContainerRepository shippingContainerRepository,
    IDepartmentRepository departmentRepository,
    IDepartmentRuleService departmentRuleService)
    : IParcelProcessingService
{
    private readonly IDepartmentRepository _departmentRepository =
        departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));

    private readonly IDepartmentRuleService _departmentRuleService =
        departmentRuleService ?? throw new ArgumentNullException(nameof(departmentRuleService));

    private readonly IParcelRepository _parcelRepository =
        parcelRepository ?? throw new ArgumentNullException(nameof(parcelRepository));

    private readonly IShippingContainerRepository _shippingContainerRepository =
        shippingContainerRepository ?? throw new ArgumentNullException(nameof(shippingContainerRepository));

    public async Task<ParcelDto> ProcessParcelAsync(Guid parcelId)
    {
        var parcel = await _parcelRepository.GetByIdAsync(parcelId);
        if (parcel == null)
            throw new ArgumentException($"Parcel with ID {parcelId} not found");

        // Update status to processing
        parcel.UpdateStatus(ParcelStatus.Processing);

        // Check if insurance approval is required
        if (parcel.RequiresInsuranceApproval)
        {
            parcel.UpdateStatus(ParcelStatus.InsuranceApprovalRequired);
            var insuranceDept = await _departmentRepository.GetByNameAsync(DepartmentNames.Insurance);
            if (insuranceDept != null) parcel.AssignDepartment(insuranceDept);
        }
        else
        {
            // Assign departments based on rules using DepartmentRuleService
            await AssignDepartmentsByRulesAsync(parcel);
            parcel.UpdateStatus(ParcelStatus.AssignedToDepartment);
        }

        var updatedParcel = await _parcelRepository.UpdateAsync(parcel);
        return MapToParcelDto(updatedParcel);
    }

    public async Task<IEnumerable<ParcelDto>> ProcessContainerAsync(Guid containerId)
    {
        var container = await _shippingContainerRepository.GetByIdAsync(containerId);
        if (container == null)
            throw new ArgumentException($"Container with ID {containerId} not found");

        var processedParcels = new List<ParcelDto>();

        foreach (var parcel in container.Parcels)
        {
            var processedParcel = await ProcessParcelAsync(parcel.Id);
            processedParcels.Add(processedParcel);
        }

        return processedParcels;
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