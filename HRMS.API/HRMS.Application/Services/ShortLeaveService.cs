using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class ShortLeaveService : IShortLeaveService
{
    // Business rules: change these to match your policy.
    private const decimal MaxHoursPerRequest = 2m;
    private const decimal MaxHoursPerMonth = 6m;

    private static readonly string Pending = ChangeRequestStatus.Pending.ToString();
    private static readonly string Approved = ChangeRequestStatus.Approved.ToString();
    private static readonly string Rejected = ChangeRequestStatus.Rejected.ToString();
    private static readonly string Cancelled = ChangeRequestStatus.Cancelled.ToString();
    private static readonly string[] ApproverRoles = { "Admin", "HR" };

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _notifier;
    private readonly IAttendanceLockService _lock;

    public ShortLeaveService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser,
        INotificationPublisher notifier, IAttendanceLockService lockService)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _notifier = notifier;
        _lock = lockService;
    }

    public async Task<List<ShortLeaveResponseDto>> GetAsync(string? status, int? employeeId, DateTime? fromDate, DateTime? toDate)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<ChangeRequestStatus>(status, "status");
        var from = fromDate?.Date ?? DateTime.Now.Date.AddDays(-60);
        var to = toDate?.Date ?? DateTime.Now.Date.AddDays(30);

        var items = await _uow.Repository<ShortLeave>().FindAsync(s =>
            s.LeaveDate >= from && s.LeaveDate <= to &&
            (statusText == null || s.Status == statusText) &&
            (!employeeId.HasValue || s.EmployeeId == employeeId.Value));

        return await MapAsync(items.OrderByDescending(s => s.LeaveDate).ThenByDescending(s => s.Id).Take(500));
    }

    public async Task<ShortLeaveResponseDto> GetByIdAsync(int id)
        => (await MapAsync(new[] { await LoadAsync(id) })).First();

    public async Task<ShortLeaveResponseDto> CreateAsync(ShortLeaveCreateDto dto)
    {
        var employee = await _uow.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        if (!employee.IsActive)
            throw new BadRequestException("This employee is inactive.");

        var date = dto.LeaveDate.Date;
        if (date < DateTime.Now.Date.AddDays(-30) || date > DateTime.Now.Date.AddDays(30))
            throw new BadRequestException("Leave Date must be within the last 30 days and the next 30 days.");

        await _lock.EnsureOpenAsync(date, date);

        var hours = Math.Round((decimal)(dto.ToTime - dto.FromTime).TotalHours, 2);
        if (hours <= 0)
            throw new BadRequestException("To Time must be after From Time.");

        if (hours > MaxHoursPerRequest)
            throw new BadRequestException($"A short leave cannot be longer than {MaxHoursPerRequest:0.##} hours.");

        var employeeId = employee.Id;

        var sameDay = await _uow.Repository<ShortLeave>().FindAsync(s =>
            s.EmployeeId == employeeId && s.LeaveDate == date && (s.Status == Pending || s.Status == Approved));
        if (sameDay.Any(s => dto.FromTime < s.ToTime && dto.ToTime > s.FromTime))
            throw new BadRequestException("This employee already has a short leave overlapping with this time.");

        var approvedText = LeaveStatus.Approved.ToString();
        if (await _uow.Repository<EmployeeLeave>().AnyAsync(l => l.EmployeeId == employeeId && l.Status == approvedText && l.FromDate <= date && l.ToDate >= date))
            throw new BadRequestException("This employee is on an approved full-day leave on this date.");

        var monthStart = new DateTime(date.Year, date.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var usedThisMonth = (await _uow.Repository<ShortLeave>().FindAsync(s =>
                s.EmployeeId == employeeId && s.LeaveDate >= monthStart && s.LeaveDate <= monthEnd && (s.Status == Pending || s.Status == Approved)))
            .Sum(s => s.TotalHours);

        if (usedThisMonth + hours > MaxHoursPerMonth)
            throw new BadRequestException($"The monthly limit of {MaxHoursPerMonth:0.##} short-leave hours would be exceeded (already used/requested: {usedThisMonth:0.##}).");

        var item = new ShortLeave
        {
            EmployeeId = employeeId,
            LeaveDate = date,
            FromTime = dto.FromTime,
            ToTime = dto.ToTime,
            TotalHours = hours,
            Reason = dto.Reason?.Trim(),
            Status = Pending
        };
        await _uow.Repository<ShortLeave>().AddAsync(item);

        var text = $"{employee.FullName} ({employee.EmployeeCode}) requested a short leave on {date:dd MMM yyyy} ({dto.FromTime:hh\\:mm}-{dto.ToTime:hh\\:mm}).";
        await _notifier.NotifyRolesAsync(ApproverRoles, NotificationCategory.Attendance, "New short leave request", text, "/attendance/short-leaves", _currentUser.UserId);

        if (employee.ReportingManagerId.HasValue)
            await _notifier.NotifyEmployeeAsync(employee.ReportingManagerId.Value, NotificationCategory.Attendance, "Team short leave request", text, "/team/short-leaves");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(item.Id);
    }

    public async Task<ShortLeaveResponseDto> ApproveAsync(int id)
    {
        var item = await LoadAsync(id);
        if (item.Status != Pending)
            throw new BadRequestException($"Only a Pending request can be approved. Current status: {item.Status}.");

        await _lock.EnsureOpenAsync(item.LeaveDate, item.LeaveDate);

        item.Status = Approved;
        item.ApprovedBy = _currentUser.UserId;
        item.ApprovedDate = DateTime.UtcNow;
        item.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<ShortLeave>().Update(item);

        // If the day's attendance already exists, keep its short-leave total in sync.
        var employeeId = item.EmployeeId;
        var date = item.LeaveDate.Date;
        var attendance = (await _uow.Repository<EmployeeAttendance>().FindAsync(a => a.EmployeeId == employeeId && a.WorkingDate == date)).FirstOrDefault();
        if (attendance != null)
        {
            var alreadyApproved = (await _uow.Repository<ShortLeave>().FindAsync(s =>
                    s.EmployeeId == employeeId && s.LeaveDate == date && s.Status == Approved && s.Id != item.Id))
                .Sum(s => s.TotalHours);

            attendance.TotalShortLeave = alreadyApproved + item.TotalHours;
            attendance.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<EmployeeAttendance>().Update(attendance);
        }

        await _notifier.NotifyEmployeeAsync(item.EmployeeId, NotificationCategory.Attendance, "Short leave approved",
            $"Your short leave on {item.LeaveDate:dd MMM yyyy} has been approved.", "/my/attendance");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<ShortLeaveResponseDto> RejectAsync(int id, ReasonDto dto)
    {
        var item = await LoadAsync(id);
        if (item.Status != Pending)
            throw new BadRequestException($"Only a Pending request can be rejected. Current status: {item.Status}.");

        item.Status = Rejected;
        item.RejectionReason = dto.Reason.Trim();
        item.ApprovedBy = _currentUser.UserId;
        item.ApprovedDate = DateTime.UtcNow;
        item.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<ShortLeave>().Update(item);

        await _notifier.NotifyEmployeeAsync(item.EmployeeId, NotificationCategory.Attendance, "Short leave rejected",
            $"Your short leave on {item.LeaveDate:dd MMM yyyy} was rejected. Reason: {dto.Reason.Trim()}", "/my/attendance");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<ShortLeaveResponseDto> CancelAsync(int id)
    {
        var item = await LoadAsync(id);
        if (item.Status != Pending)
            throw new BadRequestException($"Only a Pending request can be cancelled. Current status: {item.Status}.");

        item.Status = Cancelled;
        item.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<ShortLeave>().Update(item);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    private async Task<ShortLeave> LoadAsync(int id)
        => await _uow.Repository<ShortLeave>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(ShortLeave), id);

    private async Task<List<ShortLeaveResponseDto>> MapAsync(IEnumerable<ShortLeave> items)
    {
        var list = items.ToList();
        var ids = list.Select(s => s.EmployeeId).Distinct().ToList();
        var employees = ids.Count == 0 ? new Dictionary<int, Employee>()
            : (await _uow.Repository<Employee>().FindAsync(e => ids.Contains(e.Id))).ToDictionary(e => e.Id);

        var result = _mapper.Map<List<ShortLeaveResponseDto>>(list);
        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }
        }
        return result;
    }
}
