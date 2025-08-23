using Application.DTOs;

namespace Application.Services;

public interface IXmlImportService
{
    Task<ContainerWithParcelsDto> ImportContainerFromXmlAsync(string xmlContent);
    Task<ContainerWithParcelsDto> ImportContainerFromFileAsync(string filePath);
    Task<bool> ValidateXmlContentAsync(string xmlContent);
}