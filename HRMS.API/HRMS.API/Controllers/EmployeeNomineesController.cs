using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.EmployeeNominees;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeNomineesController : ControllerBase
    {
        private readonly IEmployeeNomineeService _service;

        public EmployeeNomineesController(IEmployeeNomineeService service)
        {
            _service = service;
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(int employeeId)
        {
            var result = await _service.GetByEmployeeIdAsync(employeeId);
            return Ok(ApiResponse<List<EmployeeNomineeResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<EmployeeNomineeResponseDto>.SuccessResponse(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeNomineeCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<EmployeeNomineeResponseDto>.SuccessResponse(created, "Nominee created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeNomineeCreateDto dto)
        {
            var updated = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<EmployeeNomineeResponseDto>.SuccessResponse(updated, "Nominee updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Nominee deleted successfully."));
        }
    }
}
