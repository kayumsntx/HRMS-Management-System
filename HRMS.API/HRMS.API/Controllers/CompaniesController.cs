using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Companies;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        // GET: api/companies
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var companies = await _companyService.GetAllAsync();
            return Ok(ApiResponse<List<CompanyResponseDto>>.SuccessResponse(companies));
        }

        // GET: api/companies/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var company = await _companyService.GetByIdAsync(id);
            return Ok(ApiResponse<CompanyResponseDto>.SuccessResponse(company));
        }

        // POST: api/companies
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CompanyCreateDto dto)
        {
            var created = await _companyService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = created.Id },
                ApiResponse<CompanyResponseDto>.SuccessResponse(created, "Company Create"));
        }

        // PUT: api/companies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CompanyUpdateDto dto)
        {
            var updated = await _companyService.UpdateAsync(id, dto);
            return Ok(ApiResponse<CompanyResponseDto>.SuccessResponse(updated, "Company Updated"));
        }

        // DELETE: api/companies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _companyService.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse("", "Company Deleted"));
        }
    }
}
