using HRMS.Application.DTOs.EmployeeStatuses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Interfaces.Services
{
    public interface IEmployeeStatusService
    {
        Task<List<EmployeeStatusResponseDto>> GetAllAsync();
        Task<EmployeeStatusResponseDto> GetByIdAsync(int id);
        Task<EmployeeStatusResponseDto> CreateAsync(EmployeeStatusCreateDto dto);
        Task<EmployeeStatusResponseDto> UpdateAsync(int id, EmployeeStatusUpdateDto dto);
        Task DeleteAsync(int id);
    }
}
