using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Security;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class LoginHistoryController : ControllerBase
{
    private readonly ILoginHistoryService _service;

    public LoginHistoryController(ILoginHistoryService service)
    {
        _service = service;
    }

    // GET: api/loginhistory?userName=admin&isSuccess=false&fromDate=2026-10-01&toDate=2026-10-20
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] LoginHistoryFilterDto filter)
        => Ok(ApiResponse<List<LoginHistoryResponseDto>>.SuccessResponse(await _service.GetAsync(filter)));
}
