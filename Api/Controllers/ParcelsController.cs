using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
///     Controller for managing individual parcels in the delivery system.
///     Provides detailed parcel operations for monitoring and troubleshooting configurable business rules.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ParcelsController : ControllerBase
{
    private readonly IDepartmentRuleService _departmentRuleService;
    private readonly IParcelProcessingService _parcelProcessingService;
    private readonly IParcelService _parcelService;

    /// <summary>
    ///     Initializes a new instance of the ParcelsController
    /// </summary>
    /// <param name="parcelService">Service for parcel data operations</param>
    /// <param name="parcelProcessingService">Service for parcel processing operations</param>
    /// <param name="departmentRuleService">Service for department rule evaluation</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public ParcelsController(
        IParcelService parcelService,
        IParcelProcessingService parcelProcessingService,
        IDepartmentRuleService departmentRuleService)
    {
        _parcelService = parcelService ?? throw new ArgumentNullException(nameof(parcelService));
        _parcelProcessingService =
            parcelProcessingService ?? throw new ArgumentNullException(nameof(parcelProcessingService));
        _departmentRuleService =
            departmentRuleService ?? throw new ArgumentNullException(nameof(departmentRuleService));
    }

    /// <summary>
    ///     Retrieves all parcels in the system
    /// </summary>
    /// <returns>A collection of all parcels with their current status and assigned departments</returns>
    /// <response code="200">Returns the list of parcels</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ParcelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ParcelDto>>> GetParcels()
    {
        try
        {
            var parcelDtos = await _parcelService.GetAllParcelsAsync();
            return Ok(parcelDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve parcels", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific parcel by ID
    /// </summary>
    /// <param name="id">The unique identifier of the parcel</param>
    /// <returns>The parcel with detailed information including assigned departments</returns>
    /// <response code="200">Returns the requested parcel</response>
    /// <response code="400">Invalid parcel ID format</response>
    /// <response code="404">Parcel not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ParcelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParcelDto>> GetParcel(Guid id)
    {
        try
        {
            var parcelDto = await _parcelService.GetParcelByIdAsync(id);
            if (parcelDto == null)
                return NotFound(new { error = "Parcel not found", message = $"No parcel found with ID: {id}" });

            return Ok(parcelDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid parcel ID", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve parcel", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves parcels filtered by processing status
    /// </summary>
    /// <param name="status">The parcel status to filter by</param>
    /// <returns>A collection of parcels with the specified status</returns>
    /// <response code="200">Returns the list of parcels with the specified status</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<ParcelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ParcelDto>>> GetParcelsByStatus(ParcelStatus status)
    {
        try
        {
            var parcelDtos = await _parcelService.GetParcelsByStatusAsync(status);
            return Ok(parcelDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve parcels by status", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves parcels that require insurance approval (value > €1000)
    /// </summary>
    /// <returns>A collection of high-value parcels requiring insurance approval</returns>
    /// <response code="200">Returns the list of parcels requiring insurance</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("requiring-insurance")]
    [ProducesResponseType(typeof(IEnumerable<ParcelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ParcelDto>>> GetParcelsRequiringInsurance()
    {
        try
        {
            var parcelDtos = await _parcelService.GetParcelsRequiringInsuranceAsync();
            return Ok(parcelDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve parcels requiring insurance", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves parcels by weight range
    /// </summary>
    /// <param name="minWeight">Minimum weight in kg</param>
    /// <param name="maxWeight">Maximum weight in kg</param>
    /// <returns>A collection of parcels within the specified weight range</returns>
    /// <response code="200">Returns the list of parcels in the weight range</response>
    /// <response code="400">Invalid weight parameters</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("by-weight")]
    [ProducesResponseType(typeof(IEnumerable<ParcelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ParcelDto>>> GetParcelsByWeightRange([FromQuery] decimal minWeight, [FromQuery] decimal maxWeight)
    {
        try
        {
            var parcelDtos = await _parcelService.GetParcelsByWeightRangeAsync(minWeight, maxWeight);
            return Ok(parcelDtos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid weight parameters", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve parcels by weight range", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves parcels by shipping container ID
    /// </summary>
    /// <param name="containerId">The unique identifier of the shipping container</param>
    /// <returns>A collection of parcels in the specified container</returns>
    /// <response code="200">Returns the list of parcels in the container</response>
    /// <response code="400">Invalid container ID format</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("by-container/{containerId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ParcelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ParcelDto>>> GetParcelsByContainer(Guid containerId)
    {
        try
        {
            var parcelDtos = await _parcelService.GetParcelsByContainerAsync(containerId);
            return Ok(parcelDtos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid container ID", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve parcels by container", message = ex.Message });
        }
    }

    /// <summary>
    ///     Processes a parcel according to configured business rules
    /// </summary>
    /// <param name="id">The unique identifier of the parcel to process</param>
    /// <returns>The processed parcel with updated status and department assignments</returns>
    /// <response code="200">Parcel processed successfully</response>
    /// <response code="400">Invalid parcel ID format</response>
    /// <response code="404">Parcel not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(typeof(ParcelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParcelDto>> ProcessParcel(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid parcel ID", message = "Parcel ID cannot be empty" });

        try
        {
            var processedParcel = await _parcelProcessingService.ProcessParcelAsync(id);
            return Ok(processedParcel);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = "Parcel not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to process parcel", message = ex.Message });
        }
    }

    /// <summary>
    ///     Updates the processing status of a parcel
    /// </summary>
    /// <param name="id">The unique identifier of the parcel</param>
    /// <param name="status">The new status to assign to the parcel</param>
    /// <returns>The updated parcel information</returns>
    /// <response code="200">Status updated successfully</response>
    /// <response code="400">Invalid parcel ID or status</response>
    /// <response code="404">Parcel not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(ParcelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParcelDto>> UpdateStatus(Guid id, [FromBody] ParcelStatus status)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid parcel ID", message = "Parcel ID cannot be empty" });

        try
        {
            var parcel = await _parcelProcessingService.UpdateParcelStatusAsync(id, status);
            return Ok(parcel);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = "Parcel not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to update parcel status", message = ex.Message });
        }
    }

    /// <summary>
    ///     Determines which departments are required for a parcel based on current business rules
    /// </summary>
    /// <param name="id">The unique identifier of the parcel</param>
    /// <returns>A collection of departments that should handle this parcel</returns>
    /// <response code="200">Returns the required departments</response>
    /// <response code="400">Invalid parcel ID format</response>
    /// <response code="404">Parcel not found</response>
    /// <response code="500">Internal server error occurred</response>
    [HttpGet("{id:guid}/required-departments")]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetRequiredDepartments(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid parcel ID", message = "Parcel ID cannot be empty" });

        try
        {
            var departments = await _departmentRuleService.DetermineRequiredDepartmentsAsync(id);
            return Ok(departments);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = "Parcel not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to determine required departments", message = ex.Message });
        }
    }
}