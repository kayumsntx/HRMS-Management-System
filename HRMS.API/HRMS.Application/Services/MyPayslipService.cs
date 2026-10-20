using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.MySpace;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

// An employee can only see payslips of payrolls that are Approved or Paid.
public class MyPayslipService : IMyPayslipService
{
    private static readonly string StatusApproved = PayrollStatus.Approved.ToString();
    private static readonly string StatusPaid = PayrollStatus.Paid.ToString();

    private readonly IUnitOfWork _unitOfWork;
    private readonly IPayrollService _payrollService;

    public MyPayslipService(IUnitOfWork unitOfWork, IPayrollService payrollService)
    {
        _unitOfWork = unitOfWork;
        _payrollService = payrollService;
    }

    public async Task<List<MyPayslipSummaryDto>> GetListAsync(int employeeId)
    {
        var details = await _unitOfWork.Repository<PayrollDetail>().FindAsync(d => d.EmployeeId == employeeId);
        var payrollIds = details.Select(d => d.PayrollId).ToList();

        if (payrollIds.Count == 0)
            return new List<MyPayslipSummaryDto>();

        var payrolls = await _unitOfWork.Repository<Payroll>()
            .FindAsync(p => payrollIds.Contains(p.Id) && (p.Status == StatusApproved || p.Status == StatusPaid));

        var detailByPayroll = details.ToDictionary(d => d.PayrollId);

        return payrolls
            .OrderByDescending(p => p.PayrollYear)
            .ThenByDescending(p => p.PayrollMonth)
            .Select(p =>
            {
                var d = detailByPayroll[p.Id];
                return new MyPayslipSummaryDto
                {
                    PayrollId = p.Id,
                    PayrollMonth = p.PayrollMonth,
                    PayrollYear = p.PayrollYear,
                    Status = p.Status,
                    GrossSalary = d.GrossSalary,
                    TotalDeduction = d.TotalDeduction,
                    NetSalary = d.NetSalary
                };
            })
            .ToList();
    }

    public async Task<PayslipDto> GetAsync(int employeeId, int payrollId)
    {
        var payroll = await _unitOfWork.Repository<Payroll>().GetByIdAsync(payrollId);

        if (payroll is null || (payroll.Status != StatusApproved && payroll.Status != StatusPaid))
            throw new NotFoundException("Payslip not found.");

        return await _payrollService.GetPayslipAsync(payrollId, employeeId);
    }
}
