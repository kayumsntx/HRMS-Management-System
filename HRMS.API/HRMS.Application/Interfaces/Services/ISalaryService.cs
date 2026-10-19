using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Interfaces.Services;

public interface ISalaryService
{
    Task<List<EmployeeSalaryResponseDto>> GetByEmployeeIdAsync(int employeeId);
    Task<EmployeeSalaryResponseDto> GetCurrentAsync(int employeeId, DateTime? asOfDate);
    Task<EmployeeSalaryResponseDto> GetByIdAsync(int id);
    Task<EmployeeSalaryResponseDto> CreateAsync(EmployeeSalaryCreateDto dto);
    Task<EmployeeSalaryResponseDto> UpdateAsync(int id, EmployeeSalaryCreateDto dto);
    Task DeleteAsync(int id);
}
