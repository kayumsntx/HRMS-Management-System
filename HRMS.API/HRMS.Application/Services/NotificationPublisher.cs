using HRMS.Application.Common.Interfaces;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class NotificationPublisher : INotificationPublisher
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationPublisher(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task NotifyUserAsync(int userId, NotificationCategory category, string title, string message, string? linkUrl = null)
        => NotifyUsersAsync(new[] { userId }, category, title, message, linkUrl);

    public async Task NotifyUsersAsync(IEnumerable<int> userIds, NotificationCategory category, string title, string message, string? linkUrl = null)
    {
        var ids = userIds.Distinct().ToList();
        if (ids.Count == 0)
            return;

        var notifications = ids.Select(id => new Notification
        {
            UserId = id,
            NotificationType = category.ToString(),
            Title = Truncate(title, 200),
            Message = Truncate(message, 1000),
            LinkUrl = linkUrl == null ? null : Truncate(linkUrl, 300)
        }).ToList();

        await _unitOfWork.Repository<Notification>().AddRangeAsync(notifications);
    }

    public async Task NotifyEmployeeAsync(int employeeId, NotificationCategory category, string title, string message, string? linkUrl = null)
    {
        var userIds = (await _unitOfWork.Repository<User>()
                .FindAsync(u => u.IsActive && u.EmployeeId == employeeId))
            .Select(u => u.Id)
            .ToList();

        await NotifyUsersAsync(userIds, category, title, message, linkUrl);
    }

    public async Task NotifyRolesAsync(IEnumerable<string> roleNames, NotificationCategory category, string title, string message,
        string? linkUrl = null, int? excludeUserId = null)
    {
        var names = roleNames.ToList();

        var roleIds = (await _unitOfWork.Repository<Role>()
                .FindAsync(r => r.IsActive && names.Contains(r.RoleName)))
            .Select(r => r.Id)
            .ToList();

        if (roleIds.Count == 0)
            return;

        var userIdsInRoles = (await _unitOfWork.Repository<UserRole>()
                .FindAsync(ur => roleIds.Contains(ur.RoleId)))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToList();

        if (userIdsInRoles.Count == 0)
            return;

        var activeUserIds = (await _unitOfWork.Repository<User>()
                .FindAsync(u => u.IsActive && userIdsInRoles.Contains(u.Id)))
            .Select(u => u.Id)
            .Where(id => !excludeUserId.HasValue || id != excludeUserId.Value)
            .ToList();

        await NotifyUsersAsync(activeUserIds, category, title, message, linkUrl);
    }

    private static string Truncate(string value, int max) => value.Length <= max ? value : value.Substring(0, max);
}
