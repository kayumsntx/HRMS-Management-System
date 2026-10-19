using HRMS.Application.DTOs.Users;

namespace HRMS.Application.Interfaces.Services;

public interface IUserService
{
    Task<List<UserResponseDto>> GetAllAsync();
    Task<UserResponseDto> GetByIdAsync(int id);
    Task<UserResponseDto> CreateAsync(UserCreateDto dto);
    Task<UserResponseDto> UpdateAsync(int id, UserUpdateDto dto);
    Task DeleteAsync(int id);
}
