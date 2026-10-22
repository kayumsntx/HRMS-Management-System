using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BonusesController : EmployeeChildControllerBase<BonusCreateDto, BonusUpdateDto, BonusResponseDto>
{
    private readonly IBonusService _service;

    public BonusesController(IBonusService service) : base(service, "Bonus")
    {
        _service = service;
    }

    // GET: api/bonuses/month?year=2026&month=10
    [HttpGet("month")]
    public async Task<IActionResult> GetByMonth([FromQuery] int year, [FromQuery] int month)
        => Ok(ApiResponse<List<BonusResponseDto>>.SuccessResponse(await _service.GetByMonthAsync(year, month)));
}
