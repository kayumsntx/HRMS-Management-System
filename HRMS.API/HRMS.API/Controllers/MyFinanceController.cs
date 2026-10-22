using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Self-service loans: works ONLY on the logged-in user's own employee record.
[Authorize]
[ApiController]
[Route("api/my/loans")]
public class MyFinanceController : ControllerBase
{
    private readonly ILoanService _loans;
    private readonly ICurrentUserService _currentUser;

    public MyFinanceController(ILoanService loans, ICurrentUserService currentUser)
    {
        _loans = loans;
        _currentUser = currentUser;
    }

    private int MyEmployeeId => _currentUser.EmployeeId
        ?? throw new BadRequestException("Your user account is not linked to an employee record. Please contact HR.");

    [HttpGet]
    public async Task<IActionResult> MyLoans()
        => Ok(ApiResponse<List<LoanResponseDto>>.SuccessResponse(await _loans.GetAsync(null, MyEmployeeId)));

    [HttpPost]
    public async Task<IActionResult> Apply([FromBody] MyLoanCreateDto dto)
    {
        var created = await _loans.CreateAsync(new LoanCreateDto
        {
            EmployeeId = MyEmployeeId,   // forced: only for myself
            LoanAmount = dto.LoanAmount,
            TotalInstallments = dto.TotalInstallments,
            StartDate = dto.StartDate,
            Purpose = dto.Purpose
        });

        return Ok(ApiResponse<LoanResponseDto>.SuccessResponse(created, "Loan request submitted."));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var loan = await _loans.GetByIdAsync(id);
        if (loan.EmployeeId != MyEmployeeId)
            throw new NotFoundException("Loan", id);

        return Ok(ApiResponse<LoanResponseDto>.SuccessResponse(await _loans.CancelAsync(id), "Loan request cancelled."));
    }
}
