using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Sections;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SectionsController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionsController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sections = await _sectionService.GetAllAsync();
            return Ok(ApiResponse<List<SectionResponseDto>>.SuccessResponse(sections));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var section = await _sectionService.GetByIdAsync(id);
            return Ok(ApiResponse<SectionResponseDto>.SuccessResponse(section));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SectionCreateDto dto)
        {
            var created = await _sectionService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<SectionResponseDto>.SuccessResponse(created, "Section created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SectionUpdateDto dto)
        {
            var updated = await _sectionService.UpdateAsync(id, dto);
            return Ok(ApiResponse<SectionResponseDto>.SuccessResponse(updated, "Section updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _sectionService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Section deleted successfully."));
        }
    }
}
