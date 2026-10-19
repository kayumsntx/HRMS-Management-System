namespace HRMS.Application.DTOs.Users;

public class UserCreateDto
{
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public string Password { get; set; } = default!;
    public int? EmployeeId { get; set; }
}
