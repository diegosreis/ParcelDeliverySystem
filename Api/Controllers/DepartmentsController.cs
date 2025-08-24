using Application.DTOs;
using Application.Services;
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
    private readonly IDepartmentService _departmentService;

    /// <summary>
    ///     Initializes a new instance of the DepartmentsController
    /// </summary>
    /// <param name="departmentService">Service for department operations</param>
    /// <exception cref="ArgumentNullException">Thrown when departmentService is null</exception>
    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService ?? throw new ArgumentNullException(nameof(departmentService));
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
            var departmentDtos = await _departmentService.GetAllDepartmentsAsync();
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
            var departmentDtos = await _departmentService.GetActiveDepartmentsAsync();
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
            var departmentDtos = await _departmentService.GetInactiveDepartmentsAsync();
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
        try
        {
            var departmentDto = await _departmentService.GetDepartmentByIdAsync(id);
            if (departmentDto == null)
                return NotFound(new { error = "Department not found", message = $"No department found with ID: {id}" });

            return Ok(departmentDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid department ID", message = ex.Message });
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
        try
        {
            var departmentDto = await _departmentService.GetDepartmentByNameAsync(name);
            if (departmentDto == null)
                return NotFound(new { error = "Department not found", message = $"No department found with name: {name}" });

            return Ok(departmentDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid department name", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve department", message = ex.Message });
        }
    }

    /// <summary>
    ///     Creates a new department in the system
    /// </summary>
    /// <param name="createDepartmentDto">The department data to create</param>
    /// <returns>The created department</returns>
    /// <response code="201">Department created successfully</response>
    /// <response code="400">Invalid department data</response>
    /// <response code="409">Department with the same name already exists</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DepartmentDto>> CreateDepartment([FromBody] CreateDepartmentDto createDepartmentDto)
    {
        try
        {
            var departmentDto = await _departmentService.CreateDepartmentAsync(createDepartmentDto);
            return CreatedAtAction(nameof(GetDepartment), new { id = departmentDto.Id }, departmentDto);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = "Invalid department data", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid department data", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = "Department already exists", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to create department", message = ex.Message });
        }
    }

    /// <summary>
    ///     Updates an existing department
    /// </summary>
    /// <param name="id">The unique identifier of the department to update</param>
    /// <param name="updateDepartmentDto">The updated department data</param>
    /// <returns>The updated department</returns>
    /// <response code="200">Department updated successfully</response>
    /// <response code="400">Invalid department data or ID</response>
    /// <response code="404">Department not found</response>
    /// <response code="409">Department name conflict</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DepartmentDto>> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentDto updateDepartmentDto)
    {
        try
        {
            var departmentDto = await _departmentService.UpdateDepartmentAsync(id, updateDepartmentDto);
            return Ok(departmentDto);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = "Invalid department data", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid department data", message = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No department found"))
        {
            return NotFound(new { error = "Department not found", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = "Department name conflict", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to update department", message = ex.Message });
        }
    }

    /// <summary>
    ///     Activates a department (makes it available for parcel processing)
    /// </summary>
    /// <param name="id">The unique identifier of the department to activate</param>
    /// <returns>The activated department</returns>
    /// <response code="200">Department activated successfully</response>
    /// <response code="400">Invalid department ID</response>
    /// <response code="404">Department not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DepartmentDto>> ActivateDepartment(Guid id)
    {
        try
        {
            var departmentDto = await _departmentService.ActivateDepartmentAsync(id);
            return Ok(departmentDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid department ID", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = "Department not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to activate department", message = ex.Message });
        }
    }

    /// <summary>
    ///     Deactivates a department (removes it from parcel processing without deleting)
    /// </summary>
    /// <param name="id">The unique identifier of the department to deactivate</param>
    /// <returns>The deactivated department</returns>
    /// <response code="200">Department deactivated successfully</response>
    /// <response code="400">Invalid department ID</response>
    /// <response code="404">Department not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPatch("{id:guid}/deactivate")]
    [ProducesResponseType(typeof(DepartmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DepartmentDto>> DeactivateDepartment(Guid id)
    {
        try
        {
            var departmentDto = await _departmentService.DeactivateDepartmentAsync(id);
            return Ok(departmentDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid department ID", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = "Department not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to deactivate department", message = ex.Message });
        }
    }

    /// <summary>
    ///     Deletes a department from the system
    /// </summary>
    /// <param name="id">The unique identifier of the department to delete</param>
    /// <returns>No content if successful</returns>
    /// <response code="204">Department deleted successfully</response>
    /// <response code="400">Invalid department ID</response>
    /// <response code="404">Department not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        try
        {
            await _departmentService.DeleteDepartmentAsync(id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid department ID", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { error = "Department not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to delete department", message = ex.Message });
        }
    }
}