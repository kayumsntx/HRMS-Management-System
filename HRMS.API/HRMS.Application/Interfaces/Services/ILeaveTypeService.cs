using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Interfaces.Services;

public interface ILeaveTypeService
{
    Task<List<LeaveTypeResponseDto>> GetAllAsync();
    Task<LeaveTypeResponseDto> GetByIdAsync(int id);
    Task<LeaveTypeResponseDto> CreateAsync(LeaveTypeCreateDto dto);
    Task<LeaveTypeResponseDto> UpdateAsync(int id, LeaveTypeUpdateDto dto);
    Task DeleteAsync(int id);
}
