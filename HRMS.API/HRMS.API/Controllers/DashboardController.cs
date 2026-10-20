using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Dashboard;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    // GET: api/dashboard?date=2026-10-20   (default = today)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateTime? date)
    {
        var result = await _service.GetSummaryAsync(date);
        return Ok(ApiResponse<DashboardSummaryDto>.SuccessResponse(result));
    }
}
