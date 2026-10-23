namespace HRMS.Application.DTOs.Users;

public class UserLoginResponseDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = default!;
    public string? Email { get; set; }
    public string Token { get; set; } = default!;            // short-lived access token
    public DateTime ExpiresAt { get; set; }
    public string RefreshToken { get; set; } = default!;     // long-lived, single use
    public DateTime RefreshTokenExpiresAt { get; set; }
    public List<string> Roles { get; set; } = new();
}
