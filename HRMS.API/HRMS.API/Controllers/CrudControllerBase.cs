using HRMS.Application.Common.Models;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

// Any logged-in user can read. Only Admin/HR can create, update, delete.
public abstract class CrudControllerBase<TCreate, TUpdate, TResponse> : ControllerBase
{
    private readonly ICrudService<TCreate, TUpdate, TResponse> _service;
    private readonly string _label;

    protected CrudControllerBase(ICrudService<TCreate, TUpdate, TResponse> service, string label)
    {
        _service = service;
        _label = label;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(ApiResponse<List<TResponse>>.SuccessResponse(await _service.GetAllAsync()));

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(ApiResponse<TResponse>.SuccessResponse(await _service.GetByIdAsync(id)));

    [Authorize(Roles = "Admin,HR")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TCreate dto)
        => Ok(ApiResponse<TResponse>.SuccessResponse(await _service.CreateAsync(dto), $"{_label} created successfully."));

    [Authorize(Roles = "Admin,HR")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TUpdate dto)
        => Ok(ApiResponse<TResponse>.SuccessResponse(await _service.UpdateAsync(id, dto), $"{_label} updated successfully."));

    [Authorize(Roles = "Admin,HR")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return Ok(ApiResponse<string>.SuccessResponse("", $"{_label} deleted successfully."));
    }
}
