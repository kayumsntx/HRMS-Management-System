namespace HRMS.Application.DTOs.Users;

public class UserLoginResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new();
}
