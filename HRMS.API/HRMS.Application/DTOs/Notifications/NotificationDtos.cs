namespace HRMS.Application.DTOs.Notifications;

public class NotificationSendDto
{
    public int? UserId { get; set; }                       // null = all active users
    public string? NotificationType { get; set; }          // General, Leave, Attendance, Payroll, Approval
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? LinkUrl { get; set; }
}

public class NotificationResponseDto
{
    public int Id { get; set; }
    public string NotificationType { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
