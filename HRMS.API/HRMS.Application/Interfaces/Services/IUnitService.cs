using HRMS.Application.DTOs.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IUnitService
    {
        Task<List<UnitResponseDto>> GetAllAsync();
        Task<UnitResponseDto> GetByIdAsync(int id);
        Task<UnitResponseDto> CreateAsync(UnitCreateDto dto);
        Task<UnitResponseDto> UpdateAsync(int id, UnitUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
