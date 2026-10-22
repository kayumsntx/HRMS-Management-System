using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Interfaces.Services;

public interface ILoanService
{
    Task<List<LoanResponseDto>> GetAsync(string? status, int? employeeId);
    Task<LoanResponseDto> GetByIdAsync(int id);
    Task<LoanResponseDto> CreateAsync(LoanCreateDto dto);
    Task<LoanResponseDto> ApproveAsync(int id);
    Task<LoanResponseDto> RejectAsync(int id, ReasonDto dto);
    Task<LoanResponseDto> CancelAsync(int id);
}
