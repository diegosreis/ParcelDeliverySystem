using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using static Domain.Constants.DefaultBusinessRuleValues;

namespace Application.Services;

/// <summary>
///     Service implementation for managing shipping containers in the delivery system.
///     Encapsulates business logic and coordinates between controllers and repositories.
/// </summary>
public class ShippingContainerService : IShippingContainerService
{
    private readonly IShippingContainerRepository _containerRepository;
    private readonly ILogger<ShippingContainerService> _logger;

    /// <summary>
    ///     Initializes a new instance of the ShippingContainerService
    /// </summary>
    /// <param name="containerRepository">Repository for container operations</param>
    /// <param name="logger">Logger for service operations</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public ShippingContainerService(IShippingContainerRepository containerRepository,
        ILogger<ShippingContainerService> logger)
    {
        _containerRepository = containerRepository ?? throw new ArgumentNullException(nameof(containerRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ShippingContainerDto>> GetAllContainersAsync()
    {
        _logger.LogInformation("Retrieving all shipping containers");

        var containers = await _containerRepository.GetAllAsync();
        return containers.Select(MapToContainerDto);
    }

    /// <inheritdoc />
    public async Task<ShippingContainerDto?> GetContainerByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve container with empty ID");
            throw new ArgumentException("Container ID cannot be empty", nameof(id));
        }

        _logger.LogInformation("Retrieving container with ID: {ContainerId}", id);

        var container = await _containerRepository.GetByIdAsync(id);
        return container != null ? MapToContainerDto(container) : null;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ShippingContainerDto>> GetContainersByStatusAsync(ShippingContainerStatus status)
    {
        _logger.LogInformation("Retrieving containers with status: {Status}", status);

        var containers = await _containerRepository.GetByStatusAsync(status);
        return containers.Select(MapToContainerDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ShippingContainerDto>> GetContainersByDateRangeAsync(DateTime startDate,
        DateTime endDate)
    {
        if (startDate > endDate)
            throw new ArgumentException("Start date cannot be greater than end date", nameof(startDate));

        _logger.LogInformation("Retrieving containers shipped between {StartDate} and {EndDate}", startDate, endDate);

        var containers = await _containerRepository.GetByShippingDateRangeAsync(startDate, endDate);
        return containers.Select(MapToContainerDto);
    }

    /// <inheritdoc />
    public async Task<ShippingContainerDto?> GetContainerWithParcelsAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve container with parcels using empty ID");
            throw new ArgumentException("Container ID cannot be empty", nameof(id));
        }

        _logger.LogInformation("Retrieving detailed container information with parcels for ID: {ContainerId}", id);

        var container = await _containerRepository.GetWithParcelsAsync(id);
        return container != null ? MapToContainerDto(container) : null;
    }

    /// <summary>
    ///     Maps a ShippingContainer entity to a ShippingContainerDto
    /// </summary>
    /// <param name="container">The container entity to map</param>
    /// <returns>The mapped container DTO</returns>
    private static ShippingContainerDto MapToContainerDto(ShippingContainer container)
    {
        return new ShippingContainerDto(
            container.Id,
            container.ContainerId, // Use the actual ContainerId property
            container.ShippingDate,
            container.Status,
            container.Parcels.Count,
            container.Parcels.Sum(p => p.Weight),
            container.Parcels.Sum(p => p.Value),
            container.Parcels.Count(p => p.Value > InsuranceValueThreshold),
            container.CreatedAt,
            container.UpdatedAt
        );
    }

    /// <summary>
    ///     Maps a ShippingContainer entity to a ShippingContainerDto with full parcel details
    /// </summary>
    /// <param name="container">The container entity to map</param>
    /// <returns>The mapped container DTO with parcels</returns>
    private static ShippingContainerDto MapToContainerDtoWithParcels(ShippingContainer container)
    {
        var parcelDtos = container.Parcels.Select(parcel => new ParcelDto(
            parcel.Id,
            new CustomerDto(
                parcel.Recipient.Id,
                parcel.Recipient.Name,
                new AddressDto(
                    parcel.Recipient.Address.Id,
                    parcel.Recipient.Address.Street,
                    parcel.Recipient.Address.Number,
                    parcel.Recipient.Address.Complement,
                    parcel.Recipient.Address.Neighborhood,
                    parcel.Recipient.Address.City,
                    parcel.Recipient.Address.State,
                    parcel.Recipient.Address.ZipCode,
                    parcel.Recipient.Address.Country,
                    parcel.Recipient.Address.CreatedAt,
                    parcel.Recipient.Address.UpdatedAt
                ),
                parcel.Recipient.CreatedAt,
                parcel.Recipient.UpdatedAt
            ),
            parcel.Weight,
            parcel.Value,
            parcel.Status,
            parcel.AssignedDepartments.Select(d => new DepartmentDto(
                d.Id,
                d.Name,
                d.Description,
                d.IsActive,
                d.CreatedAt,
                d.UpdatedAt
            )),
            parcel.CreatedAt,
            parcel.UpdatedAt
        ));

        return new ShippingContainerDto(
            container.Id,
            container.Id.ToString(), // ContainerId - using ID as string
            container.ShippingDate,
            container.Status,
            container.TotalParcels,
            0m, // TotalWeight - calculated value
            0m, // TotalValue - calculated value
            0, // ParcelsRequiringInsurance - calculated value
            container.CreatedAt,
            container.UpdatedAt
        );
    }
}