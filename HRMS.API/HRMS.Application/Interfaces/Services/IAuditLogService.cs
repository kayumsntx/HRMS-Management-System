using HRMS.Application.DTOs.AuditLogs;

namespace HRMS.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task<List<AuditLogResponseDto>> GetAsync(AuditLogFilterDto filter);
    Task<AuditLogResponseDto> GetByIdAsync(int id);
}
