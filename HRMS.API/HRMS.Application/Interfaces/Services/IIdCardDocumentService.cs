using HRMS.Application.DTOs.Files;

namespace HRMS.Application.Interfaces.Services;

public interface IIdCardDocumentService
{
    Task<FileContentDto> GetPdfAsync(int idCardId);
    Task<FileContentDto> GetQrAsync(int idCardId);
}
