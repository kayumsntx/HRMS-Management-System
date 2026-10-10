using HRMS.Application.DTOs.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IRoleService
    {
        Task<List<RoleResponseDto>> GetAllAsync();
        Task<RoleResponseDto> GetByIdAsync(int id);
        Task<RoleResponseDto> CreateAsync(RoleCreateDto dto);
        Task<RoleResponseDto> UpdateAsync(int id, RoleUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
