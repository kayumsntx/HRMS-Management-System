using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Interfaces.Services;

public interface ISalaryComponentService : ICrudService<SalaryComponentCreateDto, SalaryComponentUpdateDto, SalaryComponentResponseDto> { }

public interface ISalaryBreakdownService
{
    Task<SalaryBreakdownDto> GetAsync(int employeeSalaryId);
    Task<SalaryBreakdownDto> SaveAsync(int employeeSalaryId, SalaryBreakdownSaveDto dto);
}
