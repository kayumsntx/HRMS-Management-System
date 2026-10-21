namespace HRMS.Application.DTOs.Separations;

public class SeparationCreateDto
{
    public int EmployeeId { get; set; }
    public string SeparationType { get; set; } = default!;   // Resignation, Termination, Retirement, Other
    public DateTime? RequestDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }
    public string Reason { get; set; } = default!;
}

public class MySeparationCreateDto
{
    public DateTime? LastWorkingDate { get; set; }
    public string Reason { get; set; } = default!;
}

public class SeparationApproveDto
{
    public DateTime LastWorkingDate { get; set; }
}

public class ExitInterviewDto
{
    public DateTime InterviewDate { get; set; }
    public int? InterviewerEmployeeId { get; set; }
    public string? Feedback { get; set; }
    public bool? WouldRejoin { get; set; }
}

public class ClearanceCreateDto
{
    public int DepartmentId { get; set; }
    public decimal DueAmount { get; set; }
    public string? Remarks { get; set; }
}

public class ClearanceUpdateDto
{
    public bool IsCleared { get; set; }
    public decimal DueAmount { get; set; }
    public string? Remarks { get; set; }
}

public class SettlementSaveDto
{
    public decimal LeaveEncashment { get; set; }
    public decimal OtherPayable { get; set; }
    public decimal OtherDeduction { get; set; }
    public DateTime? SettlementDate { get; set; }
}

public class SeparationResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? DesignationName { get; set; }
    public string? DepartmentName { get; set; }
    public string SeparationType { get; set; } = default!;
    public DateTime RequestDate { get; set; }
    public DateTime? LastWorkingDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = default!;
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class ExitInterviewResponseDto
{
    public int Id { get; set; }
    public int SeparationId { get; set; }
    public DateTime InterviewDate { get; set; }
    public int? InterviewerEmployeeId { get; set; }
    public string? InterviewerName { get; set; }
    public string? Feedback { get; set; }
    public bool? WouldRejoin { get; set; }
}

public class ClearanceResponseDto
{
    public int Id { get; set; }
    public int SeparationId { get; set; }
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsCleared { get; set; }
    public decimal DueAmount { get; set; }
    public int? ClearedBy { get; set; }
    public DateTime? ClearedDate { get; set; }
    public string? Remarks { get; set; }
}

public class SettlementResponseDto
{
    public int Id { get; set; }
    public int SeparationId { get; set; }
    public decimal PayableSalary { get; set; }
    public decimal LeaveEncashment { get; set; }
    public decimal OtherPayable { get; set; }
    public decimal OtherDeduction { get; set; }
    public decimal TotalDeduction { get; set; }
    public decimal NetPayable { get; set; }   // negative = the employee owes the company
    public DateTime? SettlementDate { get; set; }
    public string Status { get; set; } = default!;
    public int? ApprovedBy { get; set; }
    public string? Notes { get; set; }
}

public class SeparationDetailDto
{
    public SeparationResponseDto Separation { get; set; } = default!;
    public ExitInterviewResponseDto? ExitInterview { get; set; }
    public List<ClearanceResponseDto> Clearances { get; set; } = new();
    public SettlementResponseDto? Settlement { get; set; }
}
