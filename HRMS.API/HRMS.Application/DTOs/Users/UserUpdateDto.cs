namespace HRMS.Application.DTOs.Users;

public class UserUpdateDto
{
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public int? EmployeeId { get; set; }
    public bool IsActive { get; set; }
}
