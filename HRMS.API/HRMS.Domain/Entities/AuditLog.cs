using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string ActionType { get; set; } = default!;   // Create / Update / Delete / Bulk
    public string TableName { get; set; } = default!;
    public string? RecordId { get; set; }
    public string? OldValues { get; set; }               // JSON
    public string? NewValues { get; set; }               // JSON
    public string? IpAddress { get; set; }
    public DateTime ActivityDate { get; set; } = DateTime.UtcNow;
}
