using HRMS.Application.DTOs.Notifications;

namespace HRMS.Application.Interfaces.Services;

public interface INotificationService
{
    Task<List<NotificationResponseDto>> GetMyAsync(int userId, bool unreadOnly);
    Task<int> GetUnreadCountAsync(int userId);
    Task MarkAsReadAsync(int userId, int id);
    Task<int> MarkAllAsReadAsync(int userId);
    Task DeleteAsync(int userId, int id);
    Task<int> SendAsync(NotificationSendDto dto);   // returns number of recipients
}
