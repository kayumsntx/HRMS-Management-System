using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.EmployeeStatuses;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeStatusesController : ControllerBase
    {
        private readonly IEmployeeStatusService _employeeStatusService;

        public EmployeeStatusesController(IEmployeeStatusService employeeStatusService)
        {
            _employeeStatusService = employeeStatusService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var statuses = await _employeeStatusService.GetAllAsync();
            return Ok(ApiResponse<List<EmployeeStatusResponseDto>>.SuccessResponse(statuses));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var status = await _employeeStatusService.GetByIdAsync(id);
            return Ok(ApiResponse<EmployeeStatusResponseDto>.SuccessResponse(status));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeStatusCreateDto dto)
        {
            var created = await _employeeStatusService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<EmployeeStatusResponseDto>.SuccessResponse(created, "Employee Status created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] EmployeeStatusUpdateDto dto)
        {
            var updated = await _employeeStatusService.UpdateAsync(id, dto);
            return Ok(ApiResponse<EmployeeStatusResponseDto>.SuccessResponse(updated, "Employee Status updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _employeeStatusService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Employee Status deleted successfully."));
        }
    }
}
