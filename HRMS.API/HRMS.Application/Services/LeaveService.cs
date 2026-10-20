using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class LeaveService : ILeaveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _notifier;

    private static readonly string Pending = LeaveStatus.Pending.ToString();
    private static readonly string Approved = LeaveStatus.Approved.ToString();
    private static readonly string Rejected = LeaveStatus.Rejected.ToString();
    private static readonly string Cancelled = LeaveStatus.Cancelled.ToString();

    private static readonly string[] ApproverRoles = { "Admin", "HR" };

    public LeaveService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser, INotificationPublisher notifier)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<List<LeaveResponseDto>> GetAllAsync(string? status)
    {
        var leaves = await _unitOfWork.Repository<EmployeeLeave>().GetAllAsync();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!Enum.TryParse<LeaveStatus>(status, true, out var parsed))
                throw new BadRequestException("Invalid status. Allowed: Pending, Approved, Rejected, Cancelled.");

            var statusText = parsed.ToString();
            leaves = leaves.Where(l => l.Status == statusText).ToList();
        }

        return await MapListAsync(leaves.OrderByDescending(l => l.AppliedDate));
    }

    public async Task<List<LeaveResponseDto>> GetByEmployeeIdAsync(int employeeId)
    {
        if (await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var leaves = (await _unitOfWork.Repository<EmployeeLeave>().GetAllAsync())
            .Where(l => l.EmployeeId == employeeId)
            .OrderByDescending(l => l.AppliedDate);

        return await MapListAsync(leaves);
    }

    public async Task<LeaveResponseDto> GetByIdAsync(int id)
    {
        var leave = await _unitOfWork.Repository<EmployeeLeave>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeLeave), id);

        return (await MapListAsync(new[] { leave })).First();
    }

    public async Task<LeaveResponseDto> ApplyAsync(LeaveApplyDto dto)
    {
        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        var leaveType = await _unitOfWork.Repository<LeaveType>().GetByIdAsync(dto.LeaveTypeId)
            ?? throw new NotFoundException(nameof(LeaveType), dto.LeaveTypeId);

        if (!leaveType.IsActive)
            throw new BadRequestException("This Leave Type is inactive.");

        var from = dto.FromDate.Date;
        var to = dto.ToDate.Date;

        if (from.Year != to.Year)
            throw new BadRequestException("A leave request cannot span two different years. Please submit separate requests.");

        var days = (decimal)((to - from).Days + 1);

        // No overlapping Pending/Approved leave for the same employee.
        var existing = await _unitOfWork.Repository<EmployeeLeave>().GetAllAsync();
        var overlaps = existing.Any(l =>
            l.EmployeeId == dto.EmployeeId &&
            (l.Status == Pending || l.Status == Approved) &&
            from <= l.ToDate.Date && to >= l.FromDate.Date);

        if (overlaps)
            throw new BadRequestException("This Employee already has a Pending or Approved leave overlapping with these dates.");

        // Paid leave requires enough balance.
        if (leaveType.IsPaid)
        {
            var balance = await FindBalanceAsync(dto.EmployeeId, dto.LeaveTypeId, from.Year)
                ?? throw new BadRequestException($"No leave balance found for {leaveType.LeaveTypeName} in {from.Year}.");

            if (balance.RemainingDays < days)
                throw new BadRequestException($"Insufficient leave balance. Remaining: {balance.RemainingDays}, Requested: {days}.");
        }

        var leave = _mapper.Map<EmployeeLeave>(dto);
        leave.FromDate = from;
        leave.ToDate = to;
        leave.NumberOfDays = days;
        leave.Status = Pending;
        leave.AppliedDate = DateTime.UtcNow;

        await _unitOfWork.Repository<EmployeeLeave>().AddAsync(leave);

        // Tell Admin/HR users there is a new request (saved together with the leave).
        await _notifier.NotifyRolesAsync(
            ApproverRoles,
            NotificationCategory.Leave,
            "New leave request",
            $"{employee.FullName} ({employee.EmployeeCode}) applied for {leaveType.LeaveTypeName} from {from:dd MMM yyyy} to {to:dd MMM yyyy} ({days} day(s)).",
            "/leave",
            _currentUser.UserId);
if (employee.ReportingManagerId.HasValue)
        {
            await _notifier.NotifyEmployeeAsync(
                employee.ReportingManagerId.Value,
                NotificationCategory.Leave,
                "Team leave request",
                $"{employee.FullName} ({employee.EmployeeCode}) applied for {leaveType.LeaveTypeName} from {from:dd MMM yyyy} to {to:dd MMM yyyy} ({days} day(s)).",
                "/team/leaves");
        }

        await _unitOfWork.SaveChangesAsync();

        return (await MapListAsync(new[] { leave })).First();
    }

    public async Task<LeaveResponseDto> ApproveAsync(int id)
    {
        var leave = await _unitOfWork.Repository<EmployeeLeave>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeLeave), id);

        if (leave.Status != Pending)
            throw new BadRequestException($"Only Pending leave can be approved. Current status: {leave.Status}.");

        var leaveType = await _unitOfWork.Repository<LeaveType>().GetByIdAsync(leave.LeaveTypeId)
            ?? throw new NotFoundException(nameof(LeaveType), leave.LeaveTypeId);

        if (leaveType.IsPaid)
        {
            var balance = await FindBalanceAsync(leave.EmployeeId, leave.LeaveTypeId, leave.FromDate.Year)
                ?? throw new BadRequestException($"No leave balance found for {leaveType.LeaveTypeName} in {leave.FromDate.Year}.");

            if (balance.RemainingDays < leave.NumberOfDays)
                throw new BadRequestException($"Insufficient leave balance. Remaining: {balance.RemainingDays}, Requested: {leave.NumberOfDays}.");

            balance.UsedDays += leave.NumberOfDays;
            balance.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Repository<EmployeeLeaveBalance>().Update(balance);
        }

        leave.Status = Approved;
        leave.ApprovedBy = _currentUser.UserId;
        leave.ApprovedDate = DateTime.UtcNow;
        leave.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<EmployeeLeave>().Update(leave);

        await _notifier.NotifyEmployeeAsync(
            leave.EmployeeId,
            NotificationCategory.Leave,
            "Leave approved",
            $"Your {leaveType.LeaveTypeName} request from {leave.FromDate:dd MMM yyyy} to {leave.ToDate:dd MMM yyyy} has been approved.",
            "/leave");

        await _unitOfWork.SaveChangesAsync(); // balance + leave + notification saved together

        return (await MapListAsync(new[] { leave })).First();
    }

    public async Task<LeaveResponseDto> RejectAsync(int id, LeaveRejectDto dto)
    {
        var leave = await _unitOfWork.Repository<EmployeeLeave>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeLeave), id);

        if (leave.Status != Pending)
            throw new BadRequestException($"Only Pending leave can be rejected. Current status: {leave.Status}.");

        var leaveType = await _unitOfWork.Repository<LeaveType>().GetByIdAsync(leave.LeaveTypeId);

        leave.Status = Rejected;
        leave.RejectionReason = dto.RejectionReason;
        leave.ApprovedBy = _currentUser.UserId;
        leave.ApprovedDate = DateTime.UtcNow;
        leave.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<EmployeeLeave>().Update(leave);

        await _notifier.NotifyEmployeeAsync(
            leave.EmployeeId,
            NotificationCategory.Leave,
            "Leave rejected",
            $"Your {leaveType?.LeaveTypeName ?? "leave"} request from {leave.FromDate:dd MMM yyyy} to {leave.ToDate:dd MMM yyyy} was rejected. Reason: {dto.RejectionReason}",
            "/leave");

        await _unitOfWork.SaveChangesAsync();

        return (await MapListAsync(new[] { leave })).First();
    }

    public async Task<LeaveResponseDto> CancelAsync(int id)
    {
        var leave = await _unitOfWork.Repository<EmployeeLeave>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeLeave), id);

        if (leave.Status != Pending && leave.Status != Approved)
            throw new BadRequestException($"Only Pending or Approved leave can be cancelled. Current status: {leave.Status}.");

        // If an approved paid leave is cancelled, give the days back.
        if (leave.Status == Approved)
        {
            var leaveType = await _unitOfWork.Repository<LeaveType>().GetByIdAsync(leave.LeaveTypeId);
            if (leaveType is { IsPaid: true })
            {
                var balance = await FindBalanceAsync(leave.EmployeeId, leave.LeaveTypeId, leave.FromDate.Year);
                if (balance is not null)
                {
                    balance.UsedDays = Math.Max(0, balance.UsedDays - leave.NumberOfDays);
                    balance.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.Repository<EmployeeLeaveBalance>().Update(balance);
                }
            }
        }

        leave.Status = Cancelled;
        leave.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<EmployeeLeave>().Update(leave);
        await _unitOfWork.SaveChangesAsync();

        return (await MapListAsync(new[] { leave })).First();
    }

    // ---------------- Private Helpers ----------------

    private async Task<EmployeeLeaveBalance?> FindBalanceAsync(int employeeId, int leaveTypeId, int year)
    {
        var balances = await _unitOfWork.Repository<EmployeeLeaveBalance>().GetAllAsync();
        return balances.FirstOrDefault(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.LeaveYear == year);
    }

    private async Task<List<LeaveResponseDto>> MapListAsync(IEnumerable<EmployeeLeave> leaves)
    {
        var employees = (await _unitOfWork.Repository<Employee>().GetAllAsync())
            .ToDictionary(e => e.Id, e => e.FullName);
        var leaveTypes = (await _unitOfWork.Repository<LeaveType>().GetAllAsync())
            .ToDictionary(l => l.Id, l => l.LeaveTypeName);

        var result = _mapper.Map<List<LeaveResponseDto>>(leaves.ToList());
        foreach (var dto in result)
        {
            dto.EmployeeName = employees.GetValueOrDefault(dto.EmployeeId);
            dto.LeaveTypeName = leaveTypes.GetValueOrDefault(dto.LeaveTypeId);
        }

        return result;
    }
}
