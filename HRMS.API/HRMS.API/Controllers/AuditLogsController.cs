using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.AuditLogs;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _service;

    public AuditLogsController(IAuditLogService service)
    {
        _service = service;
    }

    // GET: api/auditlogs?tableName=Employees&actionType=Update&fromDate=2026-10-01&toDate=2026-10-20
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AuditLogFilterDto filter)
    {
        var result = await _service.GetAsync(filter);
        return Ok(ApiResponse<List<AuditLogResponseDto>>.SuccessResponse(result));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(ApiResponse<AuditLogResponseDto>.SuccessResponse(result));
    }
}
