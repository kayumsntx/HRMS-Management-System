using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Separations;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class SeparationService : ISeparationService
{
    private static readonly string Pending = SeparationStatus.Pending.ToString();
    private static readonly string Approved = SeparationStatus.Approved.ToString();
    private static readonly string Rejected = SeparationStatus.Rejected.ToString();
    private static readonly string Completed = SeparationStatus.Completed.ToString();
    private static readonly string Cancelled = SeparationStatus.Cancelled.ToString();
    private static readonly string Draft = SettlementStatus.Draft.ToString();
    private static readonly string SettlementApproved = SettlementStatus.Approved.ToString();
    private static readonly string SettlementPaid = SettlementStatus.Paid.ToString();
    private static readonly string[] ApproverRoles = { "Admin", "HR" };

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _notifier;

    public SeparationService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser, INotificationPublisher notifier)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    // =====================================================================
    // QUERY
    // =====================================================================
    public async Task<List<SeparationResponseDto>> GetAllAsync(string? status, string? type)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<SeparationStatus>(status, "status");
        var typeText = string.IsNullOrWhiteSpace(type) ? null : EnumHelper.Normalize<SeparationType>(type, "separation type");

        var items = await _uow.Repository<EmployeeSeparation>().FindAsync(s =>
            (statusText == null || s.Status == statusText) && (typeText == null || s.SeparationType == typeText));

        return await MapAsync(items.OrderByDescending(s => s.RequestDate).ThenByDescending(s => s.Id).Take(500));
    }

    public async Task<List<SeparationResponseDto>> GetByEmployeeAsync(int employeeId)
    {
        if (await _uow.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var items = await _uow.Repository<EmployeeSeparation>().FindAsync(s => s.EmployeeId == employeeId);
        return await MapAsync(items.OrderByDescending(s => s.RequestDate).ThenByDescending(s => s.Id));
    }

    public async Task<SeparationDetailDto> GetByIdAsync(int id)
    {
        var separation = await LoadAsync(id);
        var detail = new SeparationDetailDto { Separation = (await MapAsync(new[] { separation })).First() };

        var interview = (await _uow.Repository<ExitInterview>().FindAsync(x => x.SeparationId == id)).FirstOrDefault();
        if (interview != null)
        {
            detail.ExitInterview = _mapper.Map<ExitInterviewResponseDto>(interview);
            if (interview.InterviewerEmployeeId.HasValue)
                detail.ExitInterview.InterviewerName = (await _uow.Repository<Employee>().GetByIdAsync(interview.InterviewerEmployeeId.Value))?.FullName;
        }

        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);
        var clearances = await _uow.Repository<SeparationClearance>().FindAsync(c => c.SeparationId == id);
        detail.Clearances = _mapper.Map<List<ClearanceResponseDto>>(clearances.OrderBy(c => c.Id).ToList());
        foreach (var c in detail.Clearances)
            c.DepartmentName = departments.GetValueOrDefault(c.DepartmentId);

        var settlement = (await _uow.Repository<FinalSettlement>().FindAsync(x => x.SeparationId == id)).FirstOrDefault();
        if (settlement != null)
            detail.Settlement = _mapper.Map<SettlementResponseDto>(settlement);

        return detail;
    }

    // =====================================================================
    // REQUEST / APPROVE / REJECT / CANCEL
    // =====================================================================
    public Task<SeparationDetailDto> CreateAsync(SeparationCreateDto dto)
        => CreateInternalAsync(dto.EmployeeId, EnumHelper.Normalize<SeparationType>(dto.SeparationType, "separation type"),
            dto.RequestDate, dto.LastWorkingDate, dto.Reason, false);

    public Task<SeparationDetailDto> CreateForSelfAsync(int employeeId, MySeparationCreateDto dto)
        => CreateInternalAsync(employeeId, SeparationType.Resignation.ToString(), null, dto.LastWorkingDate, dto.Reason, true);

    private async Task<SeparationDetailDto> CreateInternalAsync(int employeeId, string type, DateTime? requestDate, DateTime? lastWorkingDate, string reason, bool notifyHr)
    {
        var employee = await _uow.Repository<Employee>().GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        if (!employee.IsActive)
            throw new BadRequestException("This employee is already inactive.");

        if (await _uow.Repository<EmployeeSeparation>().AnyAsync(s => s.EmployeeId == employeeId && (s.Status == Pending || s.Status == Approved)))
            throw new BadRequestException("This employee already has an open separation request.");

        var request = (requestDate ?? DateTime.Now).Date;
        if (request < DateTime.Now.Date.AddDays(-30))
            throw new BadRequestException("Request Date cannot be more than 30 days in the past.");

        if (lastWorkingDate.HasValue && lastWorkingDate.Value.Date < request)
            throw new BadRequestException("Last Working Date cannot be before the Request Date.");

        var separation = new EmployeeSeparation
        {
            EmployeeId = employeeId,
            SeparationType = type,
            RequestDate = request,
            LastWorkingDate = lastWorkingDate?.Date,
            Reason = reason.Trim(),
            Status = Pending
        };

        await _uow.Repository<EmployeeSeparation>().AddAsync(separation);

        if (notifyHr)
            await _notifier.NotifyRolesAsync(ApproverRoles, NotificationCategory.Approval, "New resignation request",
                $"{employee.FullName} ({employee.EmployeeCode}) submitted a resignation request.", "/separations", _currentUser.UserId);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(separation.Id);
    }

    public async Task<SeparationDetailDto> ApproveAsync(int id, SeparationApproveDto dto)
    {
        var separation = await LoadAsync(id);
        if (separation.Status != Pending)
            throw new BadRequestException($"Only a Pending request can be approved. Current status: {separation.Status}.");

        var lastWorkingDate = dto.LastWorkingDate.Date;
        if (lastWorkingDate < separation.RequestDate.Date)
            throw new BadRequestException("Last Working Date cannot be before the Request Date.");

        separation.Status = Approved;
        separation.LastWorkingDate = lastWorkingDate;
        separation.ApprovedBy = _currentUser.UserId;
        separation.ApprovedDate = DateTime.UtcNow;
        separation.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<EmployeeSeparation>().Update(separation);

        await _notifier.NotifyEmployeeAsync(separation.EmployeeId, NotificationCategory.Approval, "Separation approved",
            $"Your {separation.SeparationType} request was approved. Last working date: {lastWorkingDate:dd MMM yyyy}.", "/profile");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<SeparationDetailDto> RejectAsync(int id, ReasonDto dto)
    {
        var separation = await LoadAsync(id);
        if (separation.Status != Pending)
            throw new BadRequestException($"Only a Pending request can be rejected. Current status: {separation.Status}.");

        separation.Status = Rejected;
        separation.RejectionReason = dto.Reason.Trim();
        separation.ApprovedBy = _currentUser.UserId;
        separation.ApprovedDate = DateTime.UtcNow;
        separation.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<EmployeeSeparation>().Update(separation);

        await _notifier.NotifyEmployeeAsync(separation.EmployeeId, NotificationCategory.Approval, "Separation request rejected",
            $"Your {separation.SeparationType} request was rejected. Reason: {dto.Reason.Trim()}", "/profile");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<SeparationDetailDto> CancelAsync(int id)
    {
        var separation = await LoadAsync(id);
        if (separation.Status != Pending && separation.Status != Approved)
            throw new BadRequestException($"Only a Pending or Approved request can be cancelled. Current status: {separation.Status}.");

        var settlement = (await _uow.Repository<FinalSettlement>().FindAsync(s => s.SeparationId == id)).FirstOrDefault();
        if (settlement != null && settlement.Status != Draft)
            throw new BadRequestException("A separation with an approved or paid settlement cannot be cancelled.");

        separation.Status = Cancelled;
        separation.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<EmployeeSeparation>().Update(separation);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // =====================================================================
    // EXIT INTERVIEW
    // =====================================================================
    public async Task<SeparationDetailDto> SaveExitInterviewAsync(int id, ExitInterviewDto dto)
    {
        var separation = await LoadAsync(id);
        EnsureApproved(separation);

        if (dto.InterviewerEmployeeId.HasValue && await _uow.Repository<Employee>().GetByIdAsync(dto.InterviewerEmployeeId.Value) is null)
            throw new NotFoundException(nameof(Employee), dto.InterviewerEmployeeId.Value);

        var interview = (await _uow.Repository<ExitInterview>().FindAsync(x => x.SeparationId == id)).FirstOrDefault();
        if (interview == null)
        {
            interview = new ExitInterview { SeparationId = id };
            await _uow.Repository<ExitInterview>().AddAsync(interview);
        }
        else
        {
            interview.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<ExitInterview>().Update(interview);
        }

        interview.InterviewDate = dto.InterviewDate.Date;
        interview.InterviewerEmployeeId = dto.InterviewerEmployeeId;
        interview.Feedback = dto.Feedback?.Trim();
        interview.WouldRejoin = dto.WouldRejoin;

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // =====================================================================
    // CLEARANCE
    // =====================================================================
    public async Task<SeparationDetailDto> AddClearanceAsync(int id, ClearanceCreateDto dto)
    {
        var separation = await LoadAsync(id);
        EnsureApproved(separation);

        var department = await _uow.Repository<Department>().GetByIdAsync(dto.DepartmentId)
            ?? throw new NotFoundException(nameof(Department), dto.DepartmentId);

        if (!department.IsActive)
            throw new BadRequestException("This Department is inactive.");

        if (await _uow.Repository<SeparationClearance>().AnyAsync(c => c.SeparationId == id && c.DepartmentId == dto.DepartmentId))
            throw new BadRequestException("A clearance item for this department already exists.");

        await _uow.Repository<SeparationClearance>().AddAsync(new SeparationClearance
        {
            SeparationId = id,
            DepartmentId = dto.DepartmentId,
            DueAmount = dto.DueAmount,
            Remarks = dto.Remarks?.Trim()
        });

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<SeparationDetailDto> UpdateClearanceAsync(int clearanceId, ClearanceUpdateDto dto)
    {
        var clearance = await _uow.Repository<SeparationClearance>().GetByIdAsync(clearanceId)
            ?? throw new NotFoundException(nameof(SeparationClearance), clearanceId);

        var separation = await LoadAsync(clearance.SeparationId);
        EnsureApproved(separation);

        clearance.DueAmount = dto.DueAmount;
        clearance.Remarks = dto.Remarks?.Trim();
        clearance.UpdatedDate = DateTime.UtcNow;

        if (dto.IsCleared && !clearance.IsCleared)
        {
            clearance.ClearedBy = _currentUser.UserId;
            clearance.ClearedDate = DateTime.UtcNow;
        }
        else if (!dto.IsCleared)
        {
            clearance.ClearedBy = null;
            clearance.ClearedDate = null;
        }

        clearance.IsCleared = dto.IsCleared;

        _uow.Repository<SeparationClearance>().Update(clearance);
        await _uow.SaveChangesAsync();
        return await GetByIdAsync(separation.Id);
    }

    public async Task<SeparationDetailDto> DeleteClearanceAsync(int clearanceId)
    {
        var clearance = await _uow.Repository<SeparationClearance>().GetByIdAsync(clearanceId)
            ?? throw new NotFoundException(nameof(SeparationClearance), clearanceId);

        var separation = await LoadAsync(clearance.SeparationId);
        EnsureApproved(separation);

        _uow.Repository<SeparationClearance>().Delete(clearance);
        await _uow.SaveChangesAsync();
        return await GetByIdAsync(separation.Id);
    }

    // =====================================================================
    // FINAL SETTLEMENT
    // =====================================================================
    public async Task<SeparationDetailDto> SaveSettlementAsync(int id, SettlementSaveDto dto)
    {
        var separation = await LoadAsync(id);
        EnsureApproved(separation);

        if (!separation.LastWorkingDate.HasValue)
            throw new BadRequestException("The separation has no Last Working Date.");

        var settlement = (await _uow.Repository<FinalSettlement>().FindAsync(s => s.SeparationId == id)).FirstOrDefault();
        if (settlement != null && settlement.Status != Draft)
            throw new BadRequestException("An approved or paid settlement cannot be modified.");

        var employee = await _uow.Repository<Employee>().GetByIdAsync(separation.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), separation.EmployeeId);

        var lastDay = separation.LastWorkingDate.Value.Date;
        var salary = (await _uow.Repository<EmployeeSalary>().FindAsync(s =>
                s.EmployeeId == employee.Id && s.IsActive && s.EffectiveFrom <= lastDay && (s.EffectiveTo == null || s.EffectiveTo >= lastDay)))
            .OrderByDescending(s => s.EffectiveFrom)
            .FirstOrDefault()
            ?? throw new BadRequestException("No effective salary record was found for the last working date.");

        var month = lastDay.Month;
        var year = lastDay.Year;
        var payroll = (await _uow.Repository<Payroll>().FindAsync(p => p.PayrollMonth == month && p.PayrollYear == year)).FirstOrDefault();

        var alreadyPaid = false;
        if (payroll != null && (payroll.Status == PayrollStatus.Approved.ToString() || payroll.Status == PayrollStatus.Paid.ToString()))
        {
            var payrollId = payroll.Id;
            var employeeId = employee.Id;
            alreadyPaid = await _uow.Repository<PayrollDetail>().AnyAsync(d => d.PayrollId == payrollId && d.EmployeeId == employeeId);
        }

        decimal payable = 0m;
        if (!alreadyPaid)
        {
            var monthStart = new DateTime(year, month, 1);
            var start = employee.DateOfJoining.Date > monthStart ? employee.DateOfJoining.Date : monthStart;
            var workedDays = Math.Max(0, (lastDay - start).Days + 1);
            payable = Math.Round(salary.GrossSalary * workedDays / DateTime.DaysInMonth(year, month), 2, MidpointRounding.AwayFromZero);
        }

        var dues = (await _uow.Repository<SeparationClearance>().FindAsync(c => c.SeparationId == id)).Sum(c => c.DueAmount);
        var totalDeduction = dues + dto.OtherDeduction;

        if (settlement == null)
        {
            settlement = new FinalSettlement { SeparationId = id, Status = Draft };
            await _uow.Repository<FinalSettlement>().AddAsync(settlement);
        }
        else
        {
            settlement.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<FinalSettlement>().Update(settlement);
        }

        settlement.PayableSalary = payable;
        settlement.LeaveEncashment = dto.LeaveEncashment;
        settlement.OtherPayable = dto.OtherPayable;
        settlement.OtherDeduction = dto.OtherDeduction;
        settlement.TotalDeduction = totalDeduction;
        settlement.NetPayable = payable + dto.LeaveEncashment + dto.OtherPayable - totalDeduction;
        settlement.SettlementDate = dto.SettlementDate?.Date;

        await _uow.SaveChangesAsync();

        var detail = await GetByIdAsync(id);
        if (alreadyPaid && detail.Settlement != null)
            detail.Settlement.Notes = "The last month's salary is already part of an approved/paid payroll, so Payable Salary is 0.";

        return detail;
    }

    public async Task<SeparationDetailDto> ApproveSettlementAsync(int id)
    {
        var settlement = await LoadSettlementAsync(id);
        if (settlement.Status != Draft)
            throw new BadRequestException($"Only a Draft settlement can be approved. Current status: {settlement.Status}.");

        settlement.Status = SettlementApproved;
        settlement.ApprovedBy = _currentUser.UserId;
        settlement.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<FinalSettlement>().Update(settlement);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<SeparationDetailDto> PaySettlementAsync(int id)
    {
        var settlement = await LoadSettlementAsync(id);
        if (settlement.Status != SettlementApproved)
            throw new BadRequestException($"Only an Approved settlement can be marked as Paid. Current status: {settlement.Status}.");

        settlement.Status = SettlementPaid;
        settlement.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<FinalSettlement>().Update(settlement);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // =====================================================================
    // COMPLETE: deactivates the employee, linked users and the active ID card
    // =====================================================================
    public async Task<SeparationDetailDto> CompleteAsync(int id)
    {
        var separation = await LoadAsync(id);
        EnsureApproved(separation);

        if (!separation.LastWorkingDate.HasValue || separation.LastWorkingDate.Value.Date > DateTime.Now.Date)
            throw new BadRequestException("The last working date has not arrived yet.");

        var clearances = await _uow.Repository<SeparationClearance>().FindAsync(c => c.SeparationId == id);
        if (clearances.Any(c => !c.IsCleared))
            throw new BadRequestException("All departmental clearances must be completed first.");

        var settlement = (await _uow.Repository<FinalSettlement>().FindAsync(s => s.SeparationId == id)).FirstOrDefault();
        if (settlement == null || settlement.Status == Draft)
            throw new BadRequestException("The final settlement must be approved before completing the separation.");

        var employee = await _uow.Repository<Employee>().GetByIdAsync(separation.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), separation.EmployeeId);

        employee.IsActive = false;
        employee.UpdatedDate = DateTime.UtcNow;

        var statusName = separation.SeparationType == SeparationType.Termination.ToString() ? "Terminated" : "Resigned";
        var status = (await _uow.Repository<EmployeeStatus>().FindAsync(s => s.StatusName == statusName)).FirstOrDefault();
        if (status != null)
            employee.EmployeeStatusId = status.Id;

        _uow.Repository<Employee>().Update(employee);

        var employeeId = employee.Id;
        foreach (var user in await _uow.Repository<User>().FindAsync(u => u.EmployeeId == employeeId && u.IsActive))
        {
            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<User>().Update(user);
        }

        var activeCard = IdCardStatus.Active.ToString();
        foreach (var card in await _uow.Repository<IdCard>().FindAsync(c => c.EmployeeId == employeeId && c.CardStatus == activeCard))
        {
            card.CardStatus = IdCardStatus.Cancelled.ToString();
            card.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<IdCard>().Update(card);
        }

        separation.Status = Completed;
        separation.CompletedDate = DateTime.UtcNow;
        separation.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<EmployeeSeparation>().Update(separation);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // ---------------- Private Helpers ----------------

    private async Task<EmployeeSeparation> LoadAsync(int id)
        => await _uow.Repository<EmployeeSeparation>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(EmployeeSeparation), id);

    private async Task<FinalSettlement> LoadSettlementAsync(int separationId)
    {
        await LoadAsync(separationId);
        return (await _uow.Repository<FinalSettlement>().FindAsync(s => s.SeparationId == separationId)).FirstOrDefault()
            ?? throw new NotFoundException("Final settlement not found. Please calculate it first.");
    }

    private static void EnsureApproved(EmployeeSeparation separation)
    {
        if (separation.Status != Approved)
            throw new BadRequestException($"This action is allowed only for an Approved separation. Current status: {separation.Status}.");
    }

    private async Task<List<SeparationResponseDto>> MapAsync(IEnumerable<EmployeeSeparation> items)
    {
        var list = items.ToList();
        var employees = (await _uow.Repository<Employee>().GetAllAsync()).ToDictionary(e => e.Id);
        var designations = (await _uow.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);
        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);

        var result = _mapper.Map<List<SeparationResponseDto>>(list);
        foreach (var dto in result)
        {
            if (!employees.TryGetValue(dto.EmployeeId, out var employee))
                continue;

            dto.EmployeeCode = employee.EmployeeCode;
            dto.EmployeeName = employee.FullName;
            dto.DesignationName = designations.GetValueOrDefault(employee.DesignationId);
            dto.DepartmentName = departments.GetValueOrDefault(employee.DepartmentId);
        }

        return result;
    }
}
