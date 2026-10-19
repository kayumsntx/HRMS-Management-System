using HRMS.Application.DTOs.IdCards;

namespace HRMS.Application.Interfaces.Services;

public interface IIdCardService
{
    Task<List<IdCardResponseDto>> GetAllAsync(string? status);
    Task<List<IdCardResponseDto>> GetByEmployeeIdAsync(int employeeId);
    Task<IdCardResponseDto> GetByIdAsync(int id);
    Task<IdCardPreviewDto> GetPreviewAsync(int id);
    Task<IdCardResponseDto> IssueAsync(IdCardIssueDto dto);
    Task<IdCardResponseDto> ReissueAsync(IdCardReissueDto dto);
    Task<IdCardResponseDto> ChangeStatusAsync(int id, IdCardStatusDto dto);
    Task<IdCardResponseDto> RegisterPrintAsync(int id);
}
