using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalaryComponentsController : CrudControllerBase<SalaryComponentCreateDto, SalaryComponentUpdateDto, SalaryComponentResponseDto>
{
    public SalaryComponentsController(ISalaryComponentService service) : base(service, "Salary component") { }
}

[Authorize(Roles = "Admin,HR")]
[ApiController]
[Route("api/salary/{salaryId:int}/components")]
public class SalaryBreakdownController : ControllerBase
{
    private readonly ISalaryBreakdownService _service;

    public SalaryBreakdownController(ISalaryBreakdownService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(int salaryId)
        => Ok(ApiResponse<SalaryBreakdownDto>.SuccessResponse(await _service.GetAsync(salaryId)));

    [HttpPut]
    public async Task<IActionResult> Save(int salaryId, [FromBody] SalaryBreakdownSaveDto dto)
        => Ok(ApiResponse<SalaryBreakdownDto>.SuccessResponse(await _service.SaveAsync(salaryId, dto), "Salary breakdown saved."));
}
