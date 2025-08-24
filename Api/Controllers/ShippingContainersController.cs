using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
///     Controller for managing shipping containers and XML import operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ShippingContainersController : ControllerBase
{
    private readonly ILogger<ShippingContainersController> _logger;
    private readonly IParcelProcessingService _parcelProcessingService;
    private readonly IShippingContainerService _shippingContainerService;
    private readonly IXmlImportService _xmlImportService;

    /// <summary>
    ///     Initializes a new instance of the ShippingContainersController
    /// </summary>
    /// <param name="shippingContainerService">Service for container operations</param>
    /// <param name="xmlImportService">Service for XML import operations</param>
    /// <param name="parcelProcessingService">Service for parcel processing operations</param>
    /// <param name="logger">Logger instance</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    public ShippingContainersController(
        IShippingContainerService shippingContainerService,
        IXmlImportService xmlImportService,
        IParcelProcessingService parcelProcessingService,
        ILogger<ShippingContainersController> logger)
    {
        _shippingContainerService = shippingContainerService ??
                                    throw new ArgumentNullException(nameof(shippingContainerService));
        _xmlImportService = xmlImportService ?? throw new ArgumentNullException(nameof(xmlImportService));
        _parcelProcessingService =
            parcelProcessingService ?? throw new ArgumentNullException(nameof(parcelProcessingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    ///     Retrieves all shipping containers
    /// </summary>
    /// <returns>A list of shipping containers</returns>
    /// <response code="200">Returns the list of containers</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShippingContainerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ShippingContainerDto>>> GetShippingContainers()
    {
        try
        {
            var containerDtos = await _shippingContainerService.GetAllContainersAsync();
            return Ok(containerDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve containers", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific shipping container by ID
    /// </summary>
    /// <param name="id">The unique identifier of the container</param>
    /// <returns>The container with the specified ID</returns>
    /// <response code="200">Returns the requested container</response>
    /// <response code="400">Invalid container ID format</response>
    /// <response code="404">Container not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShippingContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShippingContainerDto>> GetShippingContainer(Guid id)
    {
        try
        {
            var containerDto = await _shippingContainerService.GetContainerByIdAsync(id);
            if (containerDto == null)
                return NotFound(new { error = "Container not found", message = $"No container found with ID: {id}" });

            return Ok(containerDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid container ID", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve container", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves detailed container information including all parcels
    /// </summary>
    /// <param name="id">The unique identifier of the container</param>
    /// <returns>Detailed container information with parcels</returns>
    /// <response code="200">Returns the container with parcels</response>
    /// <response code="400">Invalid container ID format</response>
    /// <response code="404">Container not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}/with-parcels")]
    [ProducesResponseType(typeof(ShippingContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShippingContainerDto>> GetShippingContainerWithParcels(Guid id)
    {
        try
        {
            var containerDto = await _shippingContainerService.GetContainerWithParcelsAsync(id);
            if (containerDto == null)
                return NotFound(new { error = "Container not found", message = $"No container found with ID: {id}" });

            return Ok(containerDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid container ID", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve container with parcels", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves containers by status
    /// </summary>
    /// <param name="status">The container status to filter by</param>
    /// <returns>A collection of containers with the specified status</returns>
    /// <response code="200">Returns the list of containers with the specified status</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<ShippingContainerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ShippingContainerDto>>> GetContainersByStatus(
        ShippingContainerStatus status)
    {
        try
        {
            var containerDtos = await _shippingContainerService.GetContainersByStatusAsync(status);
            return Ok(containerDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve containers by status", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves containers within a specific date range
    /// </summary>
    /// <param name="startDate">Start date for the range</param>
    /// <param name="endDate">End date for the range</param>
    /// <returns>A collection of containers shipped within the date range</returns>
    /// <response code="200">Returns the list of containers in the date range</response>
    /// <response code="400">Invalid date parameters</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("by-date-range")]
    [ProducesResponseType(typeof(IEnumerable<ShippingContainerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ShippingContainerDto>>> GetContainersByDateRange(
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var containerDtos = await _shippingContainerService.GetContainersByDateRangeAsync(startDate, endDate);
            return Ok(containerDtos);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid date parameters", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to retrieve containers by date range", message = ex.Message });
        }
    }

    /// <summary>
    ///     Imports a shipping container from XML file
    /// </summary>
    /// <param name="file">The XML file containing container data</param>
    /// <returns>The imported container information</returns>
    /// <response code="200">Container imported successfully</response>
    /// <response code="400">Invalid file or XML format</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ShippingContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShippingContainerDto>> ImportFromXmlFile(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "Invalid file", message = "No file provided or file is empty" });

        if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { error = "Invalid file format", message = "Only XML files are supported" });

        try
        {
            await using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var xmlContent = await reader.ReadToEndAsync();
            var containerDto = await _xmlImportService.ImportContainerFromXmlAsync(xmlContent);
            return Ok(containerDto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid XML format", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Import failed", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during XML import from file: {FileName}", file.FileName);
            return StatusCode(500, new { error = "Internal server error", message = "An unexpected error occurred" });
        }
    }

    /// <summary>
    ///     Processes all parcels in a container according to configured business rules
    /// </summary>
    /// <param name="id">The unique identifier of the container to process</param>
    /// <returns>Processing results for the container</returns>
    /// <response code="200">Container processed successfully</response>
    /// <response code="400">Invalid container ID format</response>
    /// <response code="404">Container not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessContainer(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest(new { error = "Invalid container ID", message = "Container ID cannot be empty" });

        try
        {
            var result = await _parcelProcessingService.ProcessContainerAsync(id);
            return Ok(new { message = "Container processed successfully", result });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = "Container not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Failed to process container", message = ex.Message });
        }
    }
}