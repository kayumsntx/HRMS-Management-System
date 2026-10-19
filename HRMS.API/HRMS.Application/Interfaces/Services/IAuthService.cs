using HRMS.Application.DTOs.Users;

namespace HRMS.Application.Interfaces.Services;

public interface IAuthService
{
    Task<UserLoginResponseDto> LoginAsync(UserLoginDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
