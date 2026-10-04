using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Departments;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var departments = await _departmentService.GetAllAsync();
            return Ok(ApiResponse<List<DepartmentResponseDto>>.SuccessResponse(departments));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var department = await _departmentService.GetByIdAsync(id);
            return Ok(ApiResponse<DepartmentResponseDto>.SuccessResponse(department));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartmentCreateDto dto)
        {
            var created = await _departmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<DepartmentResponseDto>.SuccessResponse(created, "Department created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DepartmentUpdateDto dto)
        {
            var updated = await _departmentService.UpdateAsync(id, dto);
            return Ok(ApiResponse<DepartmentResponseDto>.SuccessResponse(updated, "Department updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _departmentService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Department deleted successfully."));
        }
    }
}
