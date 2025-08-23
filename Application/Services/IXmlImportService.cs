using Application.DTOs;

namespace Application.Services;

public interface IXmlImportService
{
    Task<ShippingContainerWithParcelsDto> ImportContainerFromXmlAsync(string xmlContent);
    Task<ShippingContainerWithParcelsDto> ImportContainerFromFileAsync(string filePath);
    Task<bool> ValidateXmlContentAsync(string xmlContent);
}