using HRMS.Application.DTOs.UserRoles;

namespace HRMS.Application.Interfaces.Services;

public interface IUserRoleService
{
    Task<List<UserRoleResponseDto>> GetByUserIdAsync(int userId);
    Task<UserRoleResponseDto> AssignAsync(UserRoleAssignDto dto);
    Task RemoveAsync(int id);
}
