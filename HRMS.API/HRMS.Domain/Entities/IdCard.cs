using HRMS.Domain.Common;

namespace HRMS.Domain.Entities;

public class IdCard : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    public string CardNo { get; set; } = default!;
    public string IssueType { get; set; } = "Issue";     // Issue / ReIssue
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? ReIssueReason { get; set; }
    public string? QrCodeData { get; set; }
    public int PrintCount { get; set; }
    public string CardStatus { get; set; } = "Active";   // Active / Lost / Expired / Cancelled
    public int? IssuedBy { get; set; }
}
