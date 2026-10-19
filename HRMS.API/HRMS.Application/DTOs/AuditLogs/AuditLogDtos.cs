namespace HRMS.Application.DTOs.AuditLogs;

public class AuditLogFilterDto
{
    public int? UserId { get; set; }
    public string? TableName { get; set; }
    public string? ActionType { get; set; }   // Create / Update / Delete / Bulk
    public string? RecordId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class AuditLogResponseDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserName { get; set; }
    public string ActionType { get; set; } = default!;
    public string TableName { get; set; } = default!;
    public string? RecordId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ActivityDate { get; set; }   // UTC
}
