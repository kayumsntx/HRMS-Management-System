using HRMS.Application.DTOs.EmployeeEducations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IEmployeeEducationService
    {
        Task<List<EmployeeEducationResponseDto>> GetByEmployeeIdAsync(int employeeId);
        Task<EmployeeEducationResponseDto> GetByIdAsync(int id);
        Task<EmployeeEducationResponseDto> CreateAsync(EmployeeEducationCreateDto dto);
        Task<EmployeeEducationResponseDto> UpdateAsync(int id, EmployeeEducationCreateDto dto);
        Task DeleteAsync(int id);
    }
}
