using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
///     Controller for managing shipping containers and XML import operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ShippingContainersController(
    IShippingContainerRepository shippingContainerRepository,
    IXmlImportService xmlImportService,
    IParcelProcessingService parcelProcessingService)
    : ControllerBase
{
    private readonly IParcelProcessingService _parcelProcessingService =
        parcelProcessingService ?? throw new ArgumentNullException(nameof(parcelProcessingService));

    private readonly IShippingContainerRepository _shippingContainerRepository = shippingContainerRepository ??
        throw new ArgumentNullException(nameof(shippingContainerRepository));

    private readonly IXmlImportService _xmlImportService =
        xmlImportService ?? throw new ArgumentNullException(nameof(xmlImportService));

    /// <summary>
    ///     Retrieves all shipping containers
    /// </summary>
    /// <returns>A list of shipping containers</returns>
    /// <response code="200">Returns the list of containers</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ShippingContainerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ShippingContainerDto>>> GetShippingContainers()
    {
        try
        {
            var containers = await _shippingContainerRepository.GetAllAsync();
            var containerDtos = containers.Select(MapToContainerDto);
            return Ok(containerDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a specific shipping container by its internal ID
    /// </summary>
    /// <param name="id">The internal GUID of the container</param>
    /// <returns>The shipping container details</returns>
    /// <response code="200">Returns the container</response>
    /// <response code="404">Container not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ShippingContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShippingContainerDto>> GetContainer(Guid id)
    {
        try
        {
            var container = await _shippingContainerRepository.GetByIdAsync(id);
            if (container == null)
                return NotFound(new { error = "Container not found" });

            return Ok(MapToContainerDto(container));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Retrieves a shipping container by its business identifier
    /// </summary>
    /// <param name="containerId">The business container ID</param>
    /// <returns>The shipping container details</returns>
    /// <response code="200">Returns the container</response>
    /// <response code="404">Container not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("by-id/{containerId}")]
    [ProducesResponseType(typeof(ShippingContainerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShippingContainerDto>> GetContainerById(string containerId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(containerId))
                return BadRequest(new { error = "Container ID cannot be empty" });

            var container = await _shippingContainerRepository.GetByContainerIdAsync(containerId);
            if (container == null)
                return NotFound(new { error = "Container not found" });

            return Ok(MapToContainerDto(container));
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Imports a shipping container from an uploaded XML file
    /// </summary>
    /// <param name="file">The XML file containing container data</param>
    /// <returns>The imported container with its parcels</returns>
    /// <response code="201">Container imported successfully</response>
    /// <response code="400">Invalid file or XML format</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("import")]
    [ProducesResponseType(typeof(ShippingContainerWithParcelsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ShippingContainerWithParcelsDto>> ImportFromXmlFile(IFormFile? file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Only XML files are supported" });

            // Read file content safely
            using var reader = new StreamReader(file.OpenReadStream());
            var xmlContent = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(xmlContent))
                return BadRequest(new { error = "XML file is empty" });

            var container = await _xmlImportService.ImportContainerFromXmlAsync(xmlContent);
            return CreatedAtAction(nameof(GetContainer), new { id = container.Id }, container);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = "Error processing XML", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Validates XML content without importing
    /// </summary>
    /// <param name="file">The XML file to validate</param>
    /// <returns>Validation result</returns>
    /// <response code="200">XML is valid</response>
    /// <response code="400">Invalid XML</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> ValidateXml(IFormFile? file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            if (!file.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Only XML files are supported" });

            using var reader = new StreamReader(file.OpenReadStream());
            var xmlContent = await reader.ReadToEndAsync();

            var isValid = await _xmlImportService.ValidateXmlContentAsync(xmlContent);

            return Ok(new
            {
                isValid,
                message = isValid ? "XML is valid" : "XML format is invalid",
                fileName = file.FileName,
                fileSize = file.Length
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Processes all parcels in a container, assigning them to appropriate departments
    /// </summary>
    /// <param name="id">The container ID</param>
    /// <returns>List of processed parcels</returns>
    /// <response code="200">Container processed successfully</response>
    /// <response code="404">Container not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(typeof(IEnumerable<ParcelDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ParcelDto>>> ProcessContainer(Guid id)
    {
        try
        {
            var processedParcels = await _parcelProcessingService.ProcessContainerAsync(id);
            return Ok(processedParcels);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = "Container not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    /// <summary>
    ///     Maps ShippingContainer entity to DTO
    /// </summary>
    private static ShippingContainerDto MapToContainerDto(ShippingContainer shippingContainer)
    {
        return new ShippingContainerDto(
            shippingContainer.Id,
            shippingContainer.ContainerId,
            shippingContainer.ShippingDate,
            shippingContainer.Status,
            shippingContainer.TotalParcels,
            shippingContainer.TotalWeight,
            shippingContainer.TotalValue,
            shippingContainer.ParcelsRequiringInsurance,
            shippingContainer.CreatedAt,
            shippingContainer.UpdatedAt
        );
    }
}