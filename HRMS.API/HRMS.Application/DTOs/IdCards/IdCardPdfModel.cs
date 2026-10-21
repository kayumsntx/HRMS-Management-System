namespace HRMS.Application.DTOs.IdCards;

public class IdCardPdfModel
{
    public string CompanyName { get; set; } = "";
    public string CardNo { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string EmployeeCode { get; set; } = default!;
    public string? DesignationName { get; set; }
    public string? DepartmentName { get; set; }
    public string? BloodGroup { get; set; }
    public DateTime DateOfJoining { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? MobileNumber { get; set; }
    public string? EmergencyNumber { get; set; }
    public byte[]? PhotoBytes { get; set; }
    public string QrText { get; set; } = default!;
}
