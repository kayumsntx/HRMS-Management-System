using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

// Approving a month freezes its attendance: it can no longer be re-processed or corrected,
// and short leaves of that month cannot change. Reopening is blocked once the month's payroll is Approved/Paid.
public class AttendanceLockService : IAttendanceLockService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public AttendanceLockService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    public async Task<List<AttendanceLockResponseDto>> GetAsync(int? year)
    {
        var y = year ?? DateTime.Now.Year;
        var items = await _uow.Repository<AttendanceMonthLock>().FindAsync(l => l.Year == y);
        return _mapper.Map<List<AttendanceLockResponseDto>>(items.OrderBy(l => l.Month).ToList());
    }

    public async Task<AttendanceLockResponseDto> ApproveAsync(AttendanceLockDto dto)
    {
        var monthStart = new DateTime(dto.Year, dto.Month, 1);
        var nextMonth = monthStart.AddMonths(1);

        if (nextMonth > DateTime.Now.Date)
            throw new BadRequestException("The month has not ended yet. It can be approved from the first day of the next month.");

        if (!await _uow.Repository<EmployeeAttendance>().AnyAsync(a => a.WorkingDate >= monthStart && a.WorkingDate < nextMonth))
            throw new BadRequestException("There are no attendance records for this month. Please process attendance first.");

        var year = dto.Year;
        var month = dto.Month;
        var row = (await _uow.Repository<AttendanceMonthLock>().FindAsync(l => l.Year == year && l.Month == month)).FirstOrDefault();

        if (row != null && row.IsActive)
            throw new BadRequestException("This month's attendance is already approved.");

        if (row == null)
        {
            row = new AttendanceMonthLock { Year = dto.Year, Month = dto.Month };
            await _uow.Repository<AttendanceMonthLock>().AddAsync(row);
        }
        else
        {
            row.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<AttendanceMonthLock>().Update(row);
        }

        row.IsActive = true;
        row.ApprovedBy = _currentUser.UserId;
        row.ApprovedDate = DateTime.UtcNow;
        row.Remarks = dto.Remarks?.Trim();

        await _uow.SaveChangesAsync();
        return _mapper.Map<AttendanceLockResponseDto>(row);
    }

    public async Task<AttendanceLockResponseDto> ReopenAsync(int id, ReasonDto dto)
    {
        var row = await _uow.Repository<AttendanceMonthLock>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AttendanceMonthLock), id);

        if (!row.IsActive)
            throw new BadRequestException("This month is not locked.");

        var year = row.Year;
        var month = row.Month;
        var payroll = (await _uow.Repository<Payroll>().FindAsync(p => p.PayrollYear == year && p.PayrollMonth == month)).FirstOrDefault();
        if (payroll != null && (payroll.Status == PayrollStatus.Approved.ToString() || payroll.Status == PayrollStatus.Paid.ToString()))
            throw new BadRequestException($"The payroll of this month is already {payroll.Status}. The attendance cannot be reopened.");

        row.IsActive = false;
        row.Remarks = $"Reopened: {dto.Reason.Trim()}";
        row.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<AttendanceMonthLock>().Update(row);

        await _uow.SaveChangesAsync();
        return _mapper.Map<AttendanceLockResponseDto>(row);
    }

    public async Task EnsureOpenAsync(DateTime from, DateTime to)
    {
        var locks = await _uow.Repository<AttendanceMonthLock>().FindAsync(l => l.IsActive && l.Year >= from.Year && l.Year <= to.Year);
        if (locks.Count == 0)
            return;

        for (var month = new DateTime(from.Year, from.Month, 1); month <= to.Date; month = month.AddMonths(1))
        {
            if (locks.Any(l => l.Year == month.Year && l.Month == month.Month))
                throw new BadRequestException($"The attendance of {month:MMMM yyyy} is approved and locked. Reopen it first to make changes.");
        }
    }
}
