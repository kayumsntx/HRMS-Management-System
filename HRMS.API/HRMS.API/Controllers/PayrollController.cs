using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService _payrollService;

    public PayrollController(IPayrollService payrollService)
    {
        _payrollService = payrollService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _payrollService.GetAllAsync();
        return Ok(ApiResponse<List<PayrollResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _payrollService.GetByIdAsync(id);
        return Ok(ApiResponse<PayrollResponseDto>.SuccessResponse(result));
    }

    [HttpGet("{id:int}/details")]
    public async Task<IActionResult> GetDetails(int id)
    {
        var result = await _payrollService.GetDetailsAsync(id);
        return Ok(ApiResponse<List<PayrollDetailResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}/employee/{employeeId:int}/payslip")]
    public async Task<IActionResult> GetPayslip(int id, int employeeId)
    {
        var result = await _payrollService.GetPayslipAsync(id, employeeId);
        return Ok(ApiResponse<PayslipDto>.SuccessResponse(result));
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] PayrollProcessDto dto)
    {
        var result = await _payrollService.ProcessAsync(dto);
        return Ok(ApiResponse<PayrollProcessResultDto>.SuccessResponse(result, "Payroll processed successfully."));
    }

    [HttpPut("details/{detailId:int}")]
    public async Task<IActionResult> AdjustDetail(int detailId, [FromBody] PayrollDetailAdjustDto dto)
    {
        var result = await _payrollService.AdjustDetailAsync(detailId, dto);
        return Ok(ApiResponse<PayrollDetailResponseDto>.SuccessResponse(result, "Payroll detail adjusted successfully."));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _payrollService.ApproveAsync(id);
        return Ok(ApiResponse<PayrollResponseDto>.SuccessResponse(result, "Payroll approved successfully."));
    }

    [HttpPost("{id:int}/pay")]
    public async Task<IActionResult> MarkAsPaid(int id)
    {
        var result = await _payrollService.MarkAsPaidAsync(id);
        return Ok(ApiResponse<PayrollResponseDto>.SuccessResponse(result, "Payroll marked as paid."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _payrollService.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", "Payroll deleted successfully."));
    }
}
