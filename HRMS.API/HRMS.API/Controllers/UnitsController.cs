using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Units;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UnitsController : ControllerBase
    {
        private readonly IUnitService _unitService;

        public UnitsController(IUnitService unitService)
        {
            _unitService = unitService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var units = await _unitService.GetAllAsync();
            return Ok(ApiResponse<List<UnitResponseDto>>.SuccessResponse(units));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var unit = await _unitService.GetByIdAsync(id);
            return Ok(ApiResponse<UnitResponseDto>.SuccessResponse(unit));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UnitCreateDto dto)
        {
            var created = await _unitService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<UnitResponseDto>.SuccessResponse(created, "Unit Created"));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UnitUpdateDto dto)
        {
            var updated = await _unitService.UpdateAsync(id, dto);
            return Ok(ApiResponse<UnitResponseDto>.SuccessResponse(updated, "Unit Updated"));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _unitService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Unit Deleted"));
        }
    }
}
