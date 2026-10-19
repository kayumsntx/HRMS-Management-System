using HRMS.Application.DTOs.Attendance;

namespace HRMS.Application.Interfaces.Services;

public interface IAttendanceService
{
    Task<AttendanceProcessResultDto> ProcessAsync(AttendanceProcessDto dto);
    Task<List<AttendanceResponseDto>> GetAsync(AttendanceFilterDto filter);
    Task<AttendanceResponseDto> GetByIdAsync(int id);
    Task<AttendanceResponseDto> CorrectAsync(int id, AttendanceCorrectionDto dto);
    Task<AttendanceSummaryDto> GetMonthlySummaryAsync(int employeeId, int year, int month);
}
