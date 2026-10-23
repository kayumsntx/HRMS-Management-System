using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Employees;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Admin/HR (fallback policy). Shares the /api/employees prefix with EmployeesController.
[ApiController]
[Route("api/employees")]
public class EmployeeSearchController : ControllerBase
{
    private readonly IEmployeeSearchService _service;

    public EmployeeSearchController(IEmployeeSearchService service)
    {
        _service = service;
    }

    // GET: api/employees/search?search=rahim&departmentId=1&pageNumber=1&pageSize=20
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] EmployeeSearchDto filter)
        => Ok(ApiResponse<PagedResult<EmployeeListDto>>.SuccessResponse(await _service.SearchAsync(filter)));
}
