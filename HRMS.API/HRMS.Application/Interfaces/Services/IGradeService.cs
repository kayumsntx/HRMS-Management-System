using HRMS.Application.DTOs.Grades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IGradeService
    {
        Task<List<GradeResponseDto>> GetAllAsync();
        Task<GradeResponseDto> GetByIdAsync(int id);
        Task<GradeResponseDto> CreateAsync(GradeCreateDto dto);
        Task<GradeResponseDto> UpdateAsync(int id, GradeUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
