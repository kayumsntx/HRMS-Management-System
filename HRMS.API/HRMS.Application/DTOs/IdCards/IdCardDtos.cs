namespace HRMS.Application.DTOs.IdCards;

public class IdCardIssueDto
{
    public int EmployeeId { get; set; }
    public DateTime? ExpiryDate { get; set; }   // optional; default = 3 years from today
}

public class IdCardReissueDto
{
    public int EmployeeId { get; set; }
    public string Reason { get; set; } = default!;
    public bool IsLost { get; set; }            // true = old card becomes "Lost", false = "Cancelled"
    public DateTime? ExpiryDate { get; set; }
}

public class IdCardStatusDto
{
    public string Status { get; set; } = default!;   // Lost, Cancelled, Expired
}

public class IdCardResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string CardNo { get; set; } = default!;
    public string IssueType { get; set; } = default!;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
    public string? ReIssueReason { get; set; }
    public string? QrCodeData { get; set; }
    public int PrintCount { get; set; }
    public string CardStatus { get; set; } = default!;
    public int? IssuedBy { get; set; }
}

public class IdCardPreviewDto
{
    public int IdCardId { get; set; }
    public string CardNo { get; set; } = default!;
    public string? QrCodeData { get; set; }
    public string CardStatus { get; set; } = default!;
    public DateTime IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }

    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? DesignationName { get; set; }
    public string? DepartmentName { get; set; }
    public string? CompanyName { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime DateOfJoining { get; set; }
    public string? MobileNumber { get; set; }
    public string? FamilyMobileNumber { get; set; }
    public string? ProfileImage { get; set; }
}
