using HRMS.Application.DTOs.ShiftAssignments;

namespace HRMS.Application.Interfaces.Services;

public interface IShiftAssignmentService
{
    Task<List<ShiftAssignmentResponseDto>> GetByEmployeeIdAsync(int employeeId);
    Task<ShiftAssignmentResponseDto> GetByIdAsync(int id);
    Task<ShiftAssignmentResponseDto> CreateAsync(ShiftAssignmentCreateDto dto);
    Task<ShiftAssignmentResponseDto> UpdateAsync(int id, ShiftAssignmentCreateDto dto);
    Task DeleteAsync(int id);
}
