using System.Security.Claims;
using HRMS.Application.Common.Interfaces;

namespace HRMS.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? UserId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? (int?)id : null;
        }
    }

    public int? EmployeeId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirst("employeeId")?.Value;
            return int.TryParse(value, out var id) ? (int?)id : null;
        }
    }

    public string? UserName => _httpContextAccessor.HttpContext?.User.Identity?.Name;

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
