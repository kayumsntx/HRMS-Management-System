using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/[controller]")]
public class SalaryController : ControllerBase
{
    private readonly ISalaryService _salaryService;

    public SalaryController(ISalaryService salaryService)
    {
        _salaryService = salaryService;
    }

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployeeId(int employeeId)
    {
        var result = await _salaryService.GetByEmployeeIdAsync(employeeId);
        return Ok(ApiResponse<List<EmployeeSalaryResponseDto>>.SuccessResponse(result));
    }

    // GET: api/salary/employee/1/current?asOfDate=2026-10-31
    [HttpGet("employee/{employeeId:int}/current")]
    public async Task<IActionResult> GetCurrent(int employeeId, [FromQuery] DateTime? asOfDate)
    {
        var result = await _salaryService.GetCurrentAsync(employeeId, asOfDate);
        return Ok(ApiResponse<EmployeeSalaryResponseDto>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _salaryService.GetByIdAsync(id);
        return Ok(ApiResponse<EmployeeSalaryResponseDto>.SuccessResponse(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeSalaryCreateDto dto)
    {
        var created = await _salaryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            ApiResponse<EmployeeSalaryResponseDto>.SuccessResponse(created, "Salary created successfully."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] EmployeeSalaryCreateDto dto)
    {
        var updated = await _salaryService.UpdateAsync(id, dto);
        return Ok(ApiResponse<EmployeeSalaryResponseDto>.SuccessResponse(updated, "Salary updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _salaryService.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Salary deleted successfully."));
    }
}
