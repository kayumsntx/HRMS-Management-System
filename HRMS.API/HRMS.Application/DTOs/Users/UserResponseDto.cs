namespace HRMS.Application.DTOs.Users;

public class UserResponseDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int? EmployeeId { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
