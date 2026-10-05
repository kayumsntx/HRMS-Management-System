using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.EmployeeEducations;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeEducationsController : ControllerBase
    {
        private readonly IEmployeeEducationService _service;

        public EmployeeEducationsController(IEmployeeEducationService service)
        {
            _service = service;
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(int employeeId)
        {
            var result = await _service.GetByEmployeeIdAsync(employeeId);
            return Ok(ApiResponse<List<EmployeeEducationResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<EmployeeEducationResponseDto>.SuccessResponse(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeEducationCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<EmployeeEducationResponseDto>.SuccessResponse(created, "Education record created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeEducationCreateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<EmployeeEducationResponseDto>.SuccessResponse(updated, "Education record updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Education record deleted successfully."));
        }
    }
}
