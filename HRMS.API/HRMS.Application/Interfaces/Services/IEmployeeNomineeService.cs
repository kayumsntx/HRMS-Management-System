using HRMS.Application.DTOs.EmployeeNominees;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IEmployeeNomineeService
    {
        Task<List<EmployeeNomineeResponseDto>> GetByEmployeeIdAsync(int employeeId);
        Task<EmployeeNomineeResponseDto> GetByIdAsync(int id);
        Task<EmployeeNomineeResponseDto> CreateAsync(EmployeeNomineeCreateDto dto);
        Task<EmployeeNomineeResponseDto> UpdateAsync(int id, EmployeeNomineeCreateDto dto);
        Task DeleteAsync(int id);
    }
}
