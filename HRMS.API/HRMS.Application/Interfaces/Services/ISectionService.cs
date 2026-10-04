using HRMS.Application.DTOs.Sections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface ISectionService
    {
        Task<List<SectionResponseDto>> GetAllAsync();
        Task<SectionResponseDto> GetByIdAsync(int id);
        Task<SectionResponseDto> CreateAsync(SectionCreateDto dto);
        Task<SectionResponseDto> UpdateAsync(int id, SectionUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
