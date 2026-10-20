using HRMS.Application.DTOs.MySpace;
using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Interfaces.Services;

public interface IMyPayslipService
{
    Task<List<MyPayslipSummaryDto>> GetListAsync(int employeeId);
    Task<PayslipDto> GetAsync(int employeeId, int payrollId);
}
