namespace HRMS.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? EmployeeId { get; }   // the employee linked to the logged-in user (null for pure admin accounts)
    string? UserName { get; }
    string? IpAddress { get; }
    bool IsAuthenticated { get; }
}
