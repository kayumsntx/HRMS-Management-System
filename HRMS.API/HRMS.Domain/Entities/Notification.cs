using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

// One row per recipient user.
public class Notification : BaseEntity
{
    public int UserId { get; set; }
    public string NotificationType { get; set; } = "General";
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
}
