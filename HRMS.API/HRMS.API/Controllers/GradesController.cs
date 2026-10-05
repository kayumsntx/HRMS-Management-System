using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Grades;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GradesController : ControllerBase
    {
        private readonly IGradeService _gradeService;

        public GradesController(IGradeService gradeService)
        {
            _gradeService = gradeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var grades = await _gradeService.GetAllAsync();
            return Ok(ApiResponse<List<GradeResponseDto>>.SuccessResponse(grades));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var grade = await _gradeService.GetByIdAsync(id);
            return Ok(ApiResponse<GradeResponseDto>.SuccessResponse(grade));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GradeCreateDto dto)
        {
            var created = await _gradeService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<GradeResponseDto>.SuccessResponse(created, "Grade created successfully."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GradeUpdateDto dto)
        {
            var updated = await _gradeService.UpdateAsync(id, dto);
            return Ok(ApiResponse<GradeResponseDto>.SuccessResponse(updated, "Grade updated successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _gradeService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Grade deleted successfully."));
        }
    }
}
