using HRMS.Domain.Enums;

namespace HRMS.Application.Common.Interfaces;

// IMPORTANT: these methods only ADD notifications to the unit of work.
// The caller must call IUnitOfWork.SaveChangesAsync() afterwards
// (so a notification is saved in the same transaction as the business change).
public interface INotificationPublisher
{
    Task NotifyUserAsync(int userId, NotificationCategory category, string title, string message, string? linkUrl = null);

    Task NotifyUsersAsync(IEnumerable<int> userIds, NotificationCategory category, string title, string message, string? linkUrl = null);

    // Notifies the user account(s) linked to this employee.
    Task NotifyEmployeeAsync(int employeeId, NotificationCategory category, string title, string message, string? linkUrl = null);

    // Notifies every active user holding one of the given roles.
    Task NotifyRolesAsync(IEnumerable<string> roleNames, NotificationCategory category, string title, string message,
        string? linkUrl = null, int? excludeUserId = null);
}
