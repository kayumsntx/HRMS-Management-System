using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Biometric;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Admin/HR (fallback policy).
[ApiController]
[Route("api/biometric/sync-history")]
public class BiometricSyncController : ControllerBase
{
    private readonly IBiometricSyncHistoryService _service;

    public BiometricSyncController(IBiometricSyncHistoryService service)
    {
        _service = service;
    }

    // GET: api/biometric/sync-history?deviceId=1&fromDate=2026-10-01&toDate=2026-10-20
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? deviceId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        => Ok(ApiResponse<List<BiometricSyncHistoryResponseDto>>.SuccessResponse(await _service.GetAsync(deviceId, fromDate, toDate)));
}
