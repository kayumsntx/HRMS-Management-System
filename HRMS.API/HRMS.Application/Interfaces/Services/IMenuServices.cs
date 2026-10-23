using HRMS.Application.DTOs.Security;

namespace HRMS.Application.Interfaces.Services;

public interface IMenuService : ICrudService<MenuCreateDto, MenuUpdateDto, MenuResponseDto> { }

public interface IMenuPermissionService
{
    Task<List<MenuPermissionResponseDto>> GetByRoleAsync(int roleId);
    Task<List<MenuPermissionResponseDto>> SaveForRoleAsync(int roleId, MenuPermissionSaveDto dto);
    Task<List<MyMenuDto>> GetMyMenusAsync(int userId);
}
