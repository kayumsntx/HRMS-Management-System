using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Interfaces.Services;

public interface ITeamLeaveService
{
    Task<List<LeaveResponseDto>> GetTeamLeavesAsync(int managerEmployeeId, string? status);
    Task<LeaveResponseDto> ApproveAsync(int managerEmployeeId, int leaveId);
    Task<LeaveResponseDto> RejectAsync(int managerEmployeeId, int leaveId, LeaveRejectDto dto);
}
