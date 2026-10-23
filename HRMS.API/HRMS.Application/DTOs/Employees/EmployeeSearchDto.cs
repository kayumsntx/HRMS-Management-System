namespace HRMS.Application.DTOs.Employees;

public class EmployeeSearchDto
{
    public string? Search { get; set; }            // name, employee code or mobile number
    public int? DepartmentId { get; set; }
    public int? DesignationId { get; set; }
    public int? EmployeeStatusId { get; set; }
    public bool? IsActive { get; set; } = true;    // null = active and inactive
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
