using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Admin/HR (fallback policy). Employees use /api/my/loans.
[ApiController]
[Route("api/[controller]")]
public class LoansController : ControllerBase
{
    private readonly ILoanService _service;

    public LoansController(ILoanService service)
    {
        _service = service;
    }

    // GET: api/loans?status=Pending&employeeId=1
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? status, [FromQuery] int? employeeId)
        => Ok(ApiResponse<List<LoanResponseDto>>.SuccessResponse(await _service.GetAsync(status, employeeId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<LoanResponseDto>.SuccessResponse(await _service.GetByIdAsync(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] LoanCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ApiResponse<LoanResponseDto>.SuccessResponse(created, "Loan request created."));
    }

    [HttpPost("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
        => Ok(ApiResponse<LoanResponseDto>.SuccessResponse(await _service.ApproveAsync(id), "Loan approved."));

    [HttpPost("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] ReasonDto dto)
        => Ok(ApiResponse<LoanResponseDto>.SuccessResponse(await _service.RejectAsync(id, dto), "Loan rejected."));

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
        => Ok(ApiResponse<LoanResponseDto>.SuccessResponse(await _service.CancelAsync(id), "Loan cancelled."));
}
