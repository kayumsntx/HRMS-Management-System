using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.UserRoles;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserRolesController : ControllerBase
{
    private readonly IUserRoleService _service;

    public UserRolesController(IUserRoleService service)
    {
        _service = service;
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUserId(int userId)
    {
        var result = await _service.GetByUserIdAsync(userId);
        return Ok(ApiResponse<List<UserRoleResponseDto>>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Assign([FromBody] UserRoleAssignDto dto)
    {
        var result = await _service.AssignAsync(dto);
        return Ok(ApiResponse<UserRoleResponseDto>.SuccessResponse(result, "Role assigned successfully."));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Remove(int id)
    {
        await _service.RemoveAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Role removed from user successfully."));
    }
}
