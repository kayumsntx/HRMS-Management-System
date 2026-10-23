using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Common;

namespace HRMS.Application.Interfaces.Services;

public interface IAttendanceLockService
{
    Task<List<AttendanceLockResponseDto>> GetAsync(int? year);
    Task<AttendanceLockResponseDto> ApproveAsync(AttendanceLockDto dto);
    Task<AttendanceLockResponseDto> ReopenAsync(int id, ReasonDto dto);

    // Throws when any day of the range belongs to an approved (locked) month.
    Task EnsureOpenAsync(DateTime from, DateTime to);
}

public interface IShortLeaveService
{
    Task<List<ShortLeaveResponseDto>> GetAsync(string? status, int? employeeId, DateTime? fromDate, DateTime? toDate);
    Task<ShortLeaveResponseDto> GetByIdAsync(int id);
    Task<ShortLeaveResponseDto> CreateAsync(ShortLeaveCreateDto dto);
    Task<ShortLeaveResponseDto> ApproveAsync(int id);
    Task<ShortLeaveResponseDto> RejectAsync(int id, ReasonDto dto);
    Task<ShortLeaveResponseDto> CancelAsync(int id);
}
