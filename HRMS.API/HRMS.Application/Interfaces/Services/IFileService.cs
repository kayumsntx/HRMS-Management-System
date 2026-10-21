using HRMS.Application.DTOs.Files;

namespace HRMS.Application.Interfaces.Services;

public interface IFileService
{
    Task<UploadedFileDto> UploadPhotoAsync(int employeeId, string? fileName, byte[] content);
    Task<FileContentDto> GetPhotoAsync(int employeeId, int? requesterEmployeeId, bool isPrivileged);

    Task<UploadedFileDto> UploadDocumentAsync(int employeeId, string? documentType, string? remarks, string? fileName, byte[] content);
    Task<FileContentDto> GetDocumentAsync(int documentId, int? requesterEmployeeId, bool isPrivileged);
    Task DeleteDocumentAsync(int documentId);

    Task<UploadedFileDto> UploadCvAsync(int applicantId, string? fileName, byte[] content);
    Task<FileContentDto> GetCvAsync(int applicantId);
}
