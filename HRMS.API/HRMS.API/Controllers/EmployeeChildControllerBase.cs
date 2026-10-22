using HRMS.Application.Common.Models;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Personal data (addresses, bank accounts ...): Admin/HR only, on EVERY action.
[Authorize(Roles = "Admin,HR")]
public abstract class EmployeeChildControllerBase<TCreate, TUpdate, TResponse> : ControllerBase
{
    private readonly IEmployeeChildService<TCreate, TUpdate, TResponse> _service;
    private readonly string _label;

    protected EmployeeChildControllerBase(IEmployeeChildService<TCreate, TUpdate, TResponse> service, string label)
    {
        _service = service;
        _label = label;
    }

    [HttpGet("employee/{employeeId:int}")]
    public async Task<IActionResult> GetByEmployee(int employeeId)
        => Ok(ApiResponse<List<TResponse>>.SuccessResponse(await _service.GetByEmployeeAsync(employeeId)));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<TResponse>.SuccessResponse(await _service.GetByIdAsync(id)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TCreate dto)
        => Ok(ApiResponse<TResponse>.SuccessResponse(await _service.CreateAsync(dto), $"{_label} created successfully."));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TUpdate dto)
        => Ok(ApiResponse<TResponse>.SuccessResponse(await _service.UpdateAsync(id, dto), $"{_label} updated successfully."));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", $"{_label} deleted successfully."));
    }
}
