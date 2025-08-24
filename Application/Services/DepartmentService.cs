using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary>
///     Service implementation for managing departments in the parcel delivery system.
///     Encapsulates business logic and coordinates between controllers and repositories.
/// </summary>
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<DepartmentService> _logger;

    /// <summary>
    ///     Initializes a new instance of the DepartmentService
    /// </summary>
    /// <param name="departmentRepository">Repository for department operations</param>
    /// <param name="logger">Logger for service operations</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public DepartmentService(IDepartmentRepository departmentRepository, ILogger<DepartmentService> logger)
    {
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
    {
        _logger.LogInformation("Retrieving all departments");

        var departments = await _departmentRepository.GetAllAsync();
        return departments.Select(MapToDepartmentDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DepartmentDto>> GetActiveDepartmentsAsync()
    {
        _logger.LogInformation("Retrieving active departments");

        var departments = await _departmentRepository.GetActiveDepartmentsAsync();
        return departments.Select(MapToDepartmentDto);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<DepartmentDto>> GetInactiveDepartmentsAsync()
    {
        _logger.LogInformation("Retrieving inactive departments");

        var departments = await _departmentRepository.GetInactiveDepartmentsAsync();
        return departments.Select(MapToDepartmentDto);
    }

    /// <inheritdoc />
    public async Task<DepartmentDto?> GetDepartmentByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Attempted to retrieve department with empty ID");
            throw new ArgumentException("Department ID cannot be empty", nameof(id));
        }

        _logger.LogInformation("Retrieving department with ID: {DepartmentId}", id);

        var department = await _departmentRepository.GetByIdAsync(id);
        return department != null ? MapToDepartmentDto(department) : null;
    }

    /// <inheritdoc />
    public async Task<DepartmentDto?> GetDepartmentByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Attempted to retrieve department with empty or null name");
            throw new ArgumentException("Department name cannot be empty or null", nameof(name));
        }

        _logger.LogInformation("Retrieving department with name: {DepartmentName}", name);

        var department = await _departmentRepository.GetByNameAsync(name);
        return department != null ? MapToDepartmentDto(department) : null;
    }

    /// <inheritdoc />
    public async Task<DepartmentDto> CreateDepartmentAsync(CreateDepartmentDto createDepartmentDto)
    {
        ArgumentNullException.ThrowIfNull(createDepartmentDto);

        if (string.IsNullOrWhiteSpace(createDepartmentDto.Name))
            throw new ArgumentException("Department name is required", nameof(createDepartmentDto));

        _logger.LogInformation("Creating new department: {DepartmentName}", createDepartmentDto.Name);

        // Check if department with same name already exists
        var existingDepartment = await _departmentRepository.GetByNameAsync(createDepartmentDto.Name);
        if (existingDepartment != null)
        {
            _logger.LogWarning("Attempted to create department with existing name: {DepartmentName}",
                createDepartmentDto.Name);
            throw new InvalidOperationException($"A department with name '{createDepartmentDto.Name}' already exists");
        }

        // Create new department
        var department = new Department(createDepartmentDto.Name, createDepartmentDto.Description ?? "");
        var createdDepartment = await _departmentRepository.AddAsync(department);

        _logger.LogInformation("Successfully created department: {DepartmentName} with ID: {DepartmentId}",
            createdDepartment.Name, createdDepartment.Id);

        return MapToDepartmentDto(createdDepartment);
    }

    /// <inheritdoc />
    public async Task<DepartmentDto> UpdateDepartmentAsync(Guid id, UpdateDepartmentDto updateDepartmentDto)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Department ID cannot be empty", nameof(id));

        ArgumentNullException.ThrowIfNull(updateDepartmentDto);

        if (string.IsNullOrWhiteSpace(updateDepartmentDto.Name))
            throw new ArgumentException("Department name is required", nameof(updateDepartmentDto));

        _logger.LogInformation("Updating department with ID: {DepartmentId}", id);

        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            _logger.LogWarning("Attempted to update non-existent department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"No department found with ID: {id}");
        }

        // Check if another department with the same name exists (excluding current department)
        var existingDepartment = await _departmentRepository.GetByNameAsync(updateDepartmentDto.Name);
        if (existingDepartment != null && existingDepartment.Id != id)
        {
            _logger.LogWarning("Attempted to update department to existing name: {DepartmentName}",
                updateDepartmentDto.Name);
            throw new InvalidOperationException(
                $"Another department with name '{updateDepartmentDto.Name}' already exists");
        }

        // Update department
        department.Update(updateDepartmentDto.Name, updateDepartmentDto.Description ?? "");
        var updatedDepartment = await _departmentRepository.UpdateAsync(department);

        _logger.LogInformation("Successfully updated department with ID: {DepartmentId}", id);

        return MapToDepartmentDto(updatedDepartment);
    }

    /// <inheritdoc />
    public async Task<DepartmentDto> ActivateDepartmentAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Department ID cannot be empty", nameof(id));

        _logger.LogInformation("Activating department with ID: {DepartmentId}", id);

        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            _logger.LogWarning("Attempted to activate non-existent department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"No department found with ID: {id}");
        }

        department.Activate();
        var updatedDepartment = await _departmentRepository.UpdateAsync(department);

        _logger.LogInformation("Successfully activated department with ID: {DepartmentId}", id);

        return MapToDepartmentDto(updatedDepartment);
    }

    /// <inheritdoc />
    public async Task<DepartmentDto> DeactivateDepartmentAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Department ID cannot be empty", nameof(id));

        _logger.LogInformation("Deactivating department with ID: {DepartmentId}", id);

        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            _logger.LogWarning("Attempted to deactivate non-existent department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"No department found with ID: {id}");
        }

        department.Deactivate();
        var updatedDepartment = await _departmentRepository.UpdateAsync(department);

        _logger.LogInformation("Successfully deactivated department with ID: {DepartmentId}", id);

        return MapToDepartmentDto(updatedDepartment);
    }

    /// <inheritdoc />
    public async Task DeleteDepartmentAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Department ID cannot be empty", nameof(id));

        _logger.LogInformation("Deleting department with ID: {DepartmentId}", id);

        var department = await _departmentRepository.GetByIdAsync(id);
        if (department == null)
        {
            _logger.LogWarning("Attempted to delete non-existent department with ID: {DepartmentId}", id);
            throw new InvalidOperationException($"No department found with ID: {id}");
        }

        await _departmentRepository.DeleteAsync(id);

        _logger.LogInformation("Successfully deleted department with ID: {DepartmentId}", id);
    }

    /// <summary>
    ///     Maps a Department entity to a DepartmentDto
    /// </summary>
    /// <param name="department">The department entity to map</param>
    /// <returns>The mapped department DTO</returns>
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