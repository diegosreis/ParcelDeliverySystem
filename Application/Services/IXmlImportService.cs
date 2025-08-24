using Application.DTOs;

namespace Application.Services;

/// <summary>
///     Service for importing shipping containers from XML files
/// </summary>
public interface IXmlImportService
{
    /// <summary>
    ///     Imports a shipping container from XML content
    /// </summary>
    /// <param name="xmlContent">Raw XML content containing container data</param>
    /// <returns>Imported container with all parcels and assignments</returns>
    /// <exception cref="ArgumentException">Thrown when XML content is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when XML format is invalid or processing fails</exception>
    Task<ShippingContainerDto> ImportContainerFromXmlAsync(string xmlContent);

    /// <summary>
    ///     Imports a shipping container from an XML file
    /// </summary>
    /// <param name="filePath">Path to the XML file containing container data</param>
    /// <returns>Imported container with all parcels and assignments</returns>
    /// <exception cref="ArgumentException">Thrown when file path is null or empty</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when XML format is invalid or processing fails</exception>
    Task<ShippingContainerDto> ImportContainerFromXmlFileAsync(string filePath);
}