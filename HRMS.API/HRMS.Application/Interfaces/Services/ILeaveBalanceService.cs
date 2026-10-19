using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Interfaces.Services;

public interface ILeaveBalanceService
{
    Task<List<LeaveBalanceResponseDto>> GetByEmployeeIdAsync(int employeeId, int? year);
    Task<LeaveBalanceResponseDto> GetByIdAsync(int id);
    Task<LeaveBalanceResponseDto> CreateAsync(LeaveBalanceCreateDto dto);
    Task<LeaveBalanceResponseDto> UpdateAsync(int id, LeaveBalanceUpdateDto dto);
    Task DeleteAsync(int id);
}
