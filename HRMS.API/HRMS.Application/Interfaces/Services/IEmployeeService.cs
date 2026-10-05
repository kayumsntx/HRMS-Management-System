using HRMS.Application.DTOs.Employees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IEmployeeService
    {
        Task<List<EmployeeListDto>> GetAllAsync();
        Task<EmployeeResponseDto> GetByIdAsync(int id);
        Task<EmployeeResponseDto> CreateAsync(EmployeeCreateDto dto);
        Task<EmployeeResponseDto> UpdateAsync(int id, EmployeeUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
