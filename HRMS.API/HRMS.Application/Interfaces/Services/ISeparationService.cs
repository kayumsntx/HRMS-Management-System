using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Separations;

namespace HRMS.Application.Interfaces.Services;

public interface ISeparationService
{
    Task<List<SeparationResponseDto>> GetAllAsync(string? status, string? type);
    Task<SeparationDetailDto> GetByIdAsync(int id);
    Task<List<SeparationResponseDto>> GetByEmployeeAsync(int employeeId);
    Task<SeparationDetailDto> CreateAsync(SeparationCreateDto dto);
    Task<SeparationDetailDto> CreateForSelfAsync(int employeeId, MySeparationCreateDto dto);
    Task<SeparationDetailDto> ApproveAsync(int id, SeparationApproveDto dto);
    Task<SeparationDetailDto> RejectAsync(int id, ReasonDto dto);
    Task<SeparationDetailDto> CancelAsync(int id);
    Task<SeparationDetailDto> SaveExitInterviewAsync(int id, ExitInterviewDto dto);
    Task<SeparationDetailDto> AddClearanceAsync(int id, ClearanceCreateDto dto);
    Task<SeparationDetailDto> UpdateClearanceAsync(int clearanceId, ClearanceUpdateDto dto);
    Task<SeparationDetailDto> DeleteClearanceAsync(int clearanceId);
    Task<SeparationDetailDto> SaveSettlementAsync(int id, SettlementSaveDto dto);
    Task<SeparationDetailDto> ApproveSettlementAsync(int id);
    Task<SeparationDetailDto> PaySettlementAsync(int id);
    Task<SeparationDetailDto> CompleteAsync(int id);
}
