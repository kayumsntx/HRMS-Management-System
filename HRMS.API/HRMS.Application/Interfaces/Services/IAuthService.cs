using HRMS.Application.DTOs.Users;

namespace HRMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto, string? ipAddress, string? userAgent);
    Task<UserLoginResponseDto> RefreshAsync(string refreshToken, string? ipAddress);
    Task LogoutAsync(int userId, string? refreshToken);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
