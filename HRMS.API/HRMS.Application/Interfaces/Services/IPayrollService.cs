using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Interfaces.Services;

public interface IPayrollService
{
    Task<List<PayrollResponseDto>> GetAllAsync();
    Task<PayrollResponseDto> GetByIdAsync(int id);
    Task<List<PayrollDetailResponseDto>> GetDetailsAsync(int payrollId);
    Task<PayrollProcessResultDto> ProcessAsync(PayrollProcessDto dto);
    Task<PayrollDetailResponseDto> AdjustDetailAsync(int detailId, PayrollDetailAdjustDto dto);
    Task<PayrollResponseDto> ApproveAsync(int id);
    Task<PayrollResponseDto> MarkAsPaidAsync(int id);
    Task DeleteAsync(int id);
    Task<PayslipDto> GetPayslipAsync(int payrollId, int employeeId);
}
