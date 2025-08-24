using Application.DTOs;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
///     Controller for managing departments in the parcel delivery system.
///     Supports adding and removing departments as business requirements evolve.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentRepository _departmentRepository;

    /// <summary>
    ///     Initializes a new instance of the DepartmentsController
    /// </summary>
    /// <param name="departmentRepository">Repository for department operations</param>
    /// <exception cref="ArgumentNullException">Thrown when departmentRepository is null</exception>
    public DepartmentsController(IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository ?? throw new ArgumentNullException(nameof(departmentRepository));
    }

    /// <summary>
    ///     Retrieves all departments in the system
    /// </summary>
    /// <returns>A collection of all departments</returns>
    /// <response code="200">Returns the list of departments</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
    {
        try
        {
            var departments = await _departmentRepository.GetAllAsync();
            var departmentDtos = departments.Select(MapToDepartmentDto);
            return Ok(departmentDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve departments", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves all active departments
    /// </summary>
    /// <returns>A collection of active departments</returns>
    /// <response code="200">Returns the list of active departments</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetActiveDepartments()
    {
        try
        {
            var departments = await _departmentRepository.GetActiveDepartmentsAsync();
            var departmentDtos = departments.Select(MapToDepartmentDto);
            return Ok(departmentDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve active departments", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves all inactive departments
    /// </summary>
    /// <returns>A collection of inactive departments</returns>
    /// <response code="200">Returns the list of inactive departments</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("inactive")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetInactiveDepartments()
    {
        try
        {
            var departments = await _departmentRepository.GetInactiveDepartmentsAsync();
            var departmentDtos = departments.Select(MapToDepartmentDto);
            return Ok(departmentDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve inactive departments", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific department by ID
    /// </summary>
    /// <param name="id">The unique identifier of the department</param>
    /// <returns>The department with the specified ID</returns>
    /// <response code="200">Returns the requested department</response>
    /// <response code="400">Invalid department ID format</response>
    /// <response code="404">Department not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DepartmentDto>> GetDepartment(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid department ID", message = "Department ID cannot be empty" });

        try
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return NotFound(new { error = "Department not found", message = $"No department found with ID: {id}" });

            return Ok(MapToDepartmentDto(department));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve department", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a department by name
    /// </summary>
    /// <param name="name">The name of the department to find</param>
    /// <returns>The department with the specified name</returns>
    /// <response code="200">Returns the requested department</response>
    /// <response code="400">Invalid department name</response>
    /// <response code="404">Department not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("by-name/{name}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DepartmentDto>> GetDepartmentByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Invalid department name", message = "Department name cannot be empty" });

        try
        {
            var department = await _departmentRepository.GetByNameAsync(name);
            if (department == null)
                return NotFound(new
                    { error = "Department not found", message = $"No department found with name: {name}" });

            return Ok(MapToDepartmentDto(department));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve department", message = ex.Message });
        }
    }

    /// <summary>
    ///     Maps a Department entity to a DepartmentDto
    /// </summary>
    /// <param name="department">The department entity to map</param>
    /// <returns>A DepartmentDto representing the department</returns>
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