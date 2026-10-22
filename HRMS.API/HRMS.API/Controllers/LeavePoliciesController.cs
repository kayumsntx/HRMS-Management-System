using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeavePoliciesController : CrudControllerBase<LeavePolicyCreateDto, LeavePolicyUpdateDto, LeavePolicyResponseDto>
{
    private readonly ILeaveBalanceGenerationService _generator;

    public LeavePoliciesController(ILeavePolicyService service, ILeaveBalanceGenerationService generator)
        : base(service, "Leave policy")
    {
        _generator = generator;
    }

    // POST: api/leavepolicies/generate   {"year":2026}  or  {"year":2026,"employeeId":5}
    [Authorize(Roles = "Admin,HR")]
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] LeaveBalanceGenerateDto dto)
    {
        var result = await _generator.GenerateAsync(dto);
        return Ok(ApiResponse<LeaveBalanceGenerateResultDto>.SuccessResponse(result, $"{result.Created} leave balance(s) created."));
    }
}
