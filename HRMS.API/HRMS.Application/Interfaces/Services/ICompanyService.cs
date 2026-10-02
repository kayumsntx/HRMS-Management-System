using HRMS.Application.DTOs.Companies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<List<CompanyResponseDto>> GetAllAsync();
        Task<CompanyResponseDto> GetByIdAsync(int id);
        Task<CompanyResponseDto> CreateAsync(CompanyCreateDto dto);
        Task<CompanyResponseDto> UpdateAsync(int id, CompanyUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
