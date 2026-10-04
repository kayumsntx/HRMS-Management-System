using HRMS.Application.DTOs.Designations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IDesignationService
    {
        Task<List<DesignationResponseDto>> GetAllAsync();
        Task<DesignationResponseDto> GetByIdAsync(int id);
        Task<DesignationResponseDto> CreateAsync(DesignationCreateDto dto);
        Task<DesignationResponseDto> UpdateAsync(int id, DesignationUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
