using Application.DTOs;
using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
///     Service implementation for managing parcels in the delivery system.
///     Encapsulates business logic and coordinates between controllers and repositories.
/// </summary>
public class ParcelService : IParcelService
{
    private readonly IParcelRepository _parcelRepository;
    private readonly ILogger<ParcelService> _logger;

    /// <summary>
    ///     Initializes a new instance of the ParcelService
    /// </summary>
    /// <param name="parcelRepository">Repository for parcel operations</param>
    /// <param name="logger">Logger for service operations</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public ParcelService(IParcelRepository parcelRepository, ILogger<ParcelService> logger)
    {
        _parcelRepository = parcelRepository ?? throw new ArgumentNullException(nameof(parcelRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ParcelDto>> GetAllParcelsAsync()
    {
        _logger.LogInformation("Retrieving all parcels");
        
        var parcels = await _parcelRepository.GetAllAsync();
        return parcels.Select(MapToParcelDto);
    }

    /// <inheritdoc />
    public async Task<ParcelDto?> GetParcelByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve parcel with empty ID");
            throw new ArgumentException("Parcel ID cannot be empty", nameof(id));
        }

        _logger.LogInformation("Retrieving parcel with ID: {ParcelId}", id);
        
        var parcel = await _parcelRepository.GetByIdAsync(id);
        return parcel != null ? MapToParcelDto(parcel) : null;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ParcelDto>> GetParcelsByStatusAsync(ParcelStatus status)
    {
        _logger.LogInformation("Retrieving parcels with status: {Status}", status);
        
        var parcels = await _parcelRepository.GetByStatusAsync(status);
        return parcels.Select(MapToParcelDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ParcelDto>> GetParcelsRequiringInsuranceAsync()
    {
        _logger.LogInformation("Retrieving parcels requiring insurance approval");
        
        var parcels = await _parcelRepository.GetRequiringInsuranceAsync();
        return parcels.Select(MapToParcelDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ParcelDto>> GetParcelsByWeightRangeAsync(decimal minWeight, decimal maxWeight)
    {
        if (minWeight < 0)
            throw new ArgumentException("Minimum weight cannot be negative", nameof(minWeight));
        
        if (maxWeight < minWeight)
            throw new ArgumentException("Maximum weight cannot be less than minimum weight", nameof(maxWeight));

        _logger.LogInformation("Retrieving parcels with weight between {MinWeight}kg and {MaxWeight}kg", minWeight, maxWeight);
        
        var parcels = await _parcelRepository.GetByWeightRangeAsync(minWeight, maxWeight);
        return parcels.Select(MapToParcelDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ParcelDto>> GetParcelsByContainerAsync(Guid containerId)
    {
        if (containerId == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve parcels with empty container ID");
            throw new ArgumentException("Container ID cannot be empty", nameof(containerId));
        }

        _logger.LogInformation("Retrieving parcels for container: {ContainerId}", containerId);
        
        var parcels = await _parcelRepository.GetByContainerIdAsync(containerId);
        return parcels.Select(MapToParcelDto);
    }

    /// <summary>
    ///     Maps a Parcel entity to a ParcelDto
    /// </summary>
    /// <param name="parcel">The parcel entity to map</param>
    /// <returns>The mapped parcel DTO</returns>
    private static ParcelDto MapToParcelDto(Parcel parcel)
    {
        return new ParcelDto(
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
        );
    }
}
