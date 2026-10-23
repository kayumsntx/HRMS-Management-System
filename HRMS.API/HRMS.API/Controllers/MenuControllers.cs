using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Security;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenusController : CrudControllerBase<MenuCreateDto, MenuUpdateDto, MenuResponseDto>
{
    public MenusController(IMenuService service) : base(service, "Menu") { }
}

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class MenuPermissionsController : ControllerBase
{
    private readonly IMenuPermissionService _service;

    public MenuPermissionsController(IMenuPermissionService service)
    {
        _service = service;
    }

    [HttpGet("role/{roleId:int}")]
    public async Task<IActionResult> GetByRole(int roleId)
        => Ok(ApiResponse<List<MenuPermissionResponseDto>>.SuccessResponse(await _service.GetByRoleAsync(roleId)));

    [HttpPut("role/{roleId:int}")]
    public async Task<IActionResult> Save(int roleId, [FromBody] MenuPermissionSaveDto dto)
        => Ok(ApiResponse<List<MenuPermissionResponseDto>>.SuccessResponse(await _service.SaveForRoleAsync(roleId, dto), "Menu permissions saved."));
}

// The sidebar of the logged-in user.
[Authorize]
[ApiController]
[Route("api/my/menus")]
public class MyMenusController : ControllerBase
{
    private readonly IMenuPermissionService _service;
    private readonly ICurrentUserService _currentUser;

    public MyMenusController(IMenuPermissionService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("Invalid token.");
        return Ok(ApiResponse<List<MyMenuDto>>.SuccessResponse(await _service.GetMyMenusAsync(userId)));
    }
}
