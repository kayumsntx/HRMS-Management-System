using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Interfaces.Services;

public interface ILeaveService
{
    Task<List<LeaveResponseDto>> GetAllAsync(string? status);
    Task<List<LeaveResponseDto>> GetByEmployeeIdAsync(int employeeId);
    Task<LeaveResponseDto> GetByIdAsync(int id);
    Task<LeaveResponseDto> ApplyAsync(LeaveApplyDto dto);
    Task<LeaveResponseDto> ApproveAsync(int id);
    Task<LeaveResponseDto> RejectAsync(int id, LeaveRejectDto dto);
    Task<LeaveResponseDto> CancelAsync(int id);
}
