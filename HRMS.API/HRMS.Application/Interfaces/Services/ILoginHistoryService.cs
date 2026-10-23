using HRMS.Application.DTOs.Security;

namespace HRMS.Application.Interfaces.Services;

public interface ILoginHistoryService
{
    Task<List<LoginHistoryResponseDto>> GetAsync(LoginHistoryFilterDto filter);
}
