using HRMS.Application.DTOs.Dashboard;

namespace HRMS.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(DateTime? date);
}
