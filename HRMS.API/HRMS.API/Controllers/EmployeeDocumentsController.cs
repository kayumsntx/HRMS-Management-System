using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.EmployeeDocuments;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeDocumentsController : ControllerBase
    {
        private readonly IEmployeeDocumentService _service;

        public EmployeeDocumentsController(IEmployeeDocumentService service)
        {
            _service = service;
        }

        [HttpGet("employee/{employeeId}")]
        public async Task<IActionResult> GetByEmployeeId(int employeeId)
        {
            var result = await _service.GetByEmployeeIdAsync(employeeId);
            return Ok(ApiResponse<List<EmployeeDocumentResponseDto>>.SuccessResponse(result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResponse<EmployeeDocumentResponseDto>.SuccessResponse(result));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] EmployeeDocumentCreateDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                ApiResponse<EmployeeDocumentResponseDto>.SuccessResponse(created, "Document uploaded successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Document deleted successfully."));
        }
    }
}
