namespace HRMS.Application.DTOs.Team;

public class TeamMemberDto
{
    public int EmployeeId { get; set; }
    public string EmployeeCode { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? DesignationName { get; set; }
    public string? DepartmentName { get; set; }
    public string? MobileNumber { get; set; }
}
