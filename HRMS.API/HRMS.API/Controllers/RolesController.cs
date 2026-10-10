using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Roles;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleService.GetAllAsync();
            return Ok(ApiResponse<List<RoleResponseDto>>.SuccessResponse(roles));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _roleService.GetByIdAsync(id);
            return Ok(ApiResponse<RoleResponseDto>.SuccessResponse(role));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleCreateDto dto)
        {
            var created = await _roleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<RoleResponseDto>.SuccessResponse(created, "Role created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleUpdateDto dto)
        {
            var updated = await _roleService.UpdateAsync(id, dto);
            return Ok(ApiResponse<RoleResponseDto>.SuccessResponse(updated, "Role updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _roleService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Role deleted successfully."));
        }
    }
}
