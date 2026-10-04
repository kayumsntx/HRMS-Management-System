using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Designations;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignationsController : ControllerBase
    {
        private readonly IDesignationService _designationService;

        public DesignationsController(IDesignationService designationService)
        {
            _designationService = designationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var designations = await _designationService.GetAllAsync();
            return Ok(ApiResponse<List<DesignationResponseDto>>.SuccessResponse(designations));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var designation = await _designationService.GetByIdAsync(id);
            return Ok(ApiResponse<DesignationResponseDto>.SuccessResponse(designation));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DesignationCreateDto dto)
        {
            var created = await _designationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<DesignationResponseDto>.SuccessResponse(created, "Designation created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DesignationUpdateDto dto)
        {
            var updated = await _designationService.UpdateAsync(id, dto);
            return Ok(ApiResponse<DesignationResponseDto>.SuccessResponse(updated, "Designation updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _designationService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Designation deleted successfully."));
        }
    }
}
