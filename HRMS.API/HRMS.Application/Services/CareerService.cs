using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Career;
using HRMS.Application.DTOs.Common;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class CareerService : ICareerService
{
    private static readonly string Pending = ChangeRequestStatus.Pending.ToString();
    private static readonly string Approved = ChangeRequestStatus.Approved.ToString();
    private static readonly string Rejected = ChangeRequestStatus.Rejected.ToString();
    private static readonly string Cancelled = ChangeRequestStatus.Cancelled.ToString();

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _notifier;

    public CareerService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser, INotificationPublisher notifier)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<List<CareerChangeResponseDto>> GetAsync(int? employeeId, string? changeType, string? status)
    {
        var typeText = string.IsNullOrWhiteSpace(changeType) ? null : EnumHelper.Normalize<CareerActionType>(changeType, "change type");
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<ChangeRequestStatus>(status, "status");

        var items = await _uow.Repository<EmployeeCareerHistory>().FindAsync(c =>
            (!employeeId.HasValue || c.EmployeeId == employeeId.Value) &&
            (typeText == null || c.ChangeType == typeText) &&
            (statusText == null || c.Status == statusText));

        return await MapAsync(items.OrderByDescending(c => c.EffectiveDate).ThenByDescending(c => c.Id).Take(500));
    }

    public async Task<List<CareerChangeResponseDto>> GetByEmployeeAsync(int employeeId)
    {
        if (await _uow.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var items = await _uow.Repository<EmployeeCareerHistory>().FindAsync(c => c.EmployeeId == employeeId);
        return await MapAsync(items.OrderByDescending(c => c.EffectiveDate).ThenByDescending(c => c.Id));
    }

    public async Task<CareerChangeResponseDto> GetByIdAsync(int id)
        => (await MapAsync(new[] { await LoadAsync(id) })).First();

    public async Task<CareerChangeResponseDto> CreateAsync(CareerChangeCreateDto dto)
    {
        var employee = await _uow.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        if (!employee.IsActive)
            throw new BadRequestException("A career change cannot be created for an inactive employee.");

        var type = EnumHelper.Normalize<CareerActionType>(dto.ChangeType, "change type");
        var action = Enum.Parse<CareerActionType>(type);
        var effective = dto.EffectiveDate.Date;

        if (effective < employee.DateOfJoining.Date)
            throw new BadRequestException("Effective Date cannot be before the employee's joining date.");

        if (effective > DateTime.Now.Date.AddYears(1))
            throw new BadRequestException("Effective Date cannot be more than one year in the future.");

        if (await _uow.Repository<EmployeeCareerHistory>().AnyAsync(c => c.EmployeeId == dto.EmployeeId && c.Status == Pending))
            throw new BadRequestException("This employee already has a pending career change request.");

        // Only real changes are kept.
        int? newDesignation = dto.NewDesignationId.HasValue && dto.NewDesignationId != employee.DesignationId ? dto.NewDesignationId : null;
        int? newGrade = dto.NewGradeId.HasValue && dto.NewGradeId != employee.GradeId ? dto.NewGradeId : null;
        int? newDepartment = dto.NewDepartmentId.HasValue && dto.NewDepartmentId != employee.DepartmentId ? dto.NewDepartmentId : null;
        int? newUnit = dto.NewUnitId.HasValue && dto.NewUnitId != employee.UnitId ? dto.NewUnitId : null;
        int? newSection = dto.NewSectionId.HasValue && dto.NewSectionId != employee.SectionId ? dto.NewSectionId : null;

        if (!(newDesignation.HasValue || newGrade.HasValue || newDepartment.HasValue || newUnit.HasValue || newSection.HasValue))
            throw new BadRequestException("No change detected compared to the employee's current data.");

        var matchesType = action switch
        {
            CareerActionType.Promotion => newDesignation.HasValue || newGrade.HasValue,
            CareerActionType.DesignationChange => newDesignation.HasValue,
            CareerActionType.GradeChange => newGrade.HasValue,
            CareerActionType.DepartmentChange => newDepartment.HasValue,
            _ => newUnit.HasValue || newDepartment.HasValue || newSection.HasValue
        };

        if (!matchesType)
            throw new BadRequestException($"The provided values do not match a {action} change.");

        if (newDesignation.HasValue) await EnsureActiveAsync<Designation>(newDesignation.Value);
        if (newGrade.HasValue) await EnsureActiveAsync<Grade>(newGrade.Value);
        if (newDepartment.HasValue) await EnsureActiveAsync<Department>(newDepartment.Value);

        if (newUnit.HasValue)
        {
            var unit = await _uow.Repository<Unit>().GetByIdAsync(newUnit.Value) ?? throw new NotFoundException(nameof(Unit), newUnit.Value);
            if (!unit.IsActive) throw new BadRequestException("Unit is inactive.");
            if (unit.CompanyId != employee.CompanyId) throw new BadRequestException("The Unit does not belong to the employee's company.");
        }

        if (newSection.HasValue)
        {
            var section = await _uow.Repository<Section>().GetByIdAsync(newSection.Value) ?? throw new NotFoundException(nameof(Section), newSection.Value);
            if (!section.IsActive) throw new BadRequestException("Section is inactive.");
            if (section.DepartmentId != (newDepartment ?? employee.DepartmentId))
                throw new BadRequestException("The Section does not belong to the target department.");
        }

        var record = new EmployeeCareerHistory
        {
            EmployeeId = employee.Id,
            ChangeType = type,
            OldDesignationId = employee.DesignationId,
            NewDesignationId = newDesignation,
            OldGradeId = employee.GradeId,
            NewGradeId = newGrade,
            OldDepartmentId = employee.DepartmentId,
            NewDepartmentId = newDepartment,
            OldUnitId = employee.UnitId,
            NewUnitId = newUnit,
            OldSectionId = employee.SectionId,
            NewSectionId = newSection,
            EffectiveDate = effective,
            Remarks = dto.Remarks?.Trim(),
            Status = Pending
        };

        await _uow.Repository<EmployeeCareerHistory>().AddAsync(record);
        await _uow.SaveChangesAsync();

        return await GetByIdAsync(record.Id);
    }

    public async Task<CareerChangeResponseDto> ApproveAsync(int id)
    {
        var record = await LoadAsync(id);
        if (record.Status != Pending)
            throw new BadRequestException($"Only a Pending request can be approved. Current status: {record.Status}.");

        var employee = await _uow.Repository<Employee>().GetByIdAsync(record.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), record.EmployeeId);

        if (!employee.IsActive)
            throw new BadRequestException("The employee is inactive.");

        record.Status = Approved;
        record.ApprovedBy = _currentUser.UserId;
        record.ApprovedDate = DateTime.UtcNow;
        record.UpdatedDate = DateTime.UtcNow;

        var appliedNow = record.EffectiveDate.Date <= DateTime.Now.Date;
        if (appliedNow)
            Apply(record, employee);

        _uow.Repository<EmployeeCareerHistory>().Update(record);

        await _notifier.NotifyEmployeeAsync(record.EmployeeId, NotificationCategory.Approval, $"{record.ChangeType} approved",
            appliedNow
                ? $"Your {record.ChangeType} has been approved and is effective from {record.EffectiveDate:dd MMM yyyy}."
                : $"Your {record.ChangeType} has been approved and will be effective from {record.EffectiveDate:dd MMM yyyy}.",
            "/profile");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<CareerChangeResponseDto> RejectAsync(int id, ReasonDto dto)
    {
        var record = await LoadAsync(id);
        if (record.Status != Pending)
            throw new BadRequestException($"Only a Pending request can be rejected. Current status: {record.Status}.");

        record.Status = Rejected;
        record.RejectionReason = dto.Reason.Trim();
        record.ApprovedBy = _currentUser.UserId;
        record.ApprovedDate = DateTime.UtcNow;
        record.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<EmployeeCareerHistory>().Update(record);

        await _notifier.NotifyEmployeeAsync(record.EmployeeId, NotificationCategory.Approval, $"{record.ChangeType} rejected",
            $"Your {record.ChangeType} request was rejected. Reason: {dto.Reason.Trim()}", "/profile");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<CareerChangeResponseDto> CancelAsync(int id)
    {
        var record = await LoadAsync(id);
        if (record.Status != Pending)
            throw new BadRequestException($"Only a Pending request can be cancelled. Current status: {record.Status}.");

        record.Status = Cancelled;
        record.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<EmployeeCareerHistory>().Update(record);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // Applies every approved change whose effective date has arrived (future-dated promotions).
    public async Task<int> ApplyDueAsync()
    {
        var today = DateTime.Now.Date;
        var due = await _uow.Repository<EmployeeCareerHistory>().FindAsync(c => c.Status == Approved && !c.IsApplied && c.EffectiveDate <= today);

        var applied = 0;
        foreach (var record in due.OrderBy(c => c.EffectiveDate).ThenBy(c => c.Id))
        {
            var employee = await _uow.Repository<Employee>().GetByIdAsync(record.EmployeeId);
            if (employee is null || !employee.IsActive)
                continue;

            Apply(record, employee);
            _uow.Repository<EmployeeCareerHistory>().Update(record);
            applied++;
        }

        if (applied > 0)
            await _uow.SaveChangesAsync();

        return applied;
    }

    // ---------------- Private Helpers ----------------

    private void Apply(EmployeeCareerHistory record, Employee employee)
    {
        if (record.NewDesignationId.HasValue) employee.DesignationId = record.NewDesignationId.Value;
        if (record.NewGradeId.HasValue) employee.GradeId = record.NewGradeId;
        if (record.NewUnitId.HasValue) employee.UnitId = record.NewUnitId;

        if (record.NewDepartmentId.HasValue)
        {
            employee.DepartmentId = record.NewDepartmentId.Value;
            employee.SectionId = record.NewSectionId;   // the old section belongs to the old department
        }
        else if (record.NewSectionId.HasValue)
        {
            employee.SectionId = record.NewSectionId;
        }

        employee.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Employee>().Update(employee);

        record.IsApplied = true;
        record.AppliedDate = DateTime.UtcNow;
    }

    private async Task EnsureActiveAsync<T>(int id) where T : BaseEntity
    {
        var entity = await _uow.Repository<T>().GetByIdAsync(id) ?? throw new NotFoundException(typeof(T).Name, id);
        if (!entity.IsActive)
            throw new BadRequestException($"{typeof(T).Name} is inactive.");
    }

    private async Task<EmployeeCareerHistory> LoadAsync(int id)
        => await _uow.Repository<EmployeeCareerHistory>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(EmployeeCareerHistory), id);

    private static string? Name(Dictionary<int, string> map, int? id) => id.HasValue ? map.GetValueOrDefault(id.Value) : null;

    private async Task<List<CareerChangeResponseDto>> MapAsync(IEnumerable<EmployeeCareerHistory> items)
    {
        var list = items.ToList();
        var designations = (await _uow.Repository<Designation>().GetAllAsync()).ToDictionary(x => x.Id, x => x.DesignationName);
        var grades = (await _uow.Repository<Grade>().GetAllAsync()).ToDictionary(x => x.Id, x => x.GradeName);
        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(x => x.Id, x => x.DepartmentName);
        var units = (await _uow.Repository<Unit>().GetAllAsync()).ToDictionary(x => x.Id, x => x.UnitName);
        var sections = (await _uow.Repository<Section>().GetAllAsync()).ToDictionary(x => x.Id, x => x.SectionName);
        var employees = (await _uow.Repository<Employee>().GetAllAsync()).ToDictionary(x => x.Id);

        var result = _mapper.Map<List<CareerChangeResponseDto>>(list);
        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }

            dto.OldDesignationName = Name(designations, dto.OldDesignationId);
            dto.NewDesignationName = Name(designations, dto.NewDesignationId);
            dto.OldGradeName = Name(grades, dto.OldGradeId);
            dto.NewGradeName = Name(grades, dto.NewGradeId);
            dto.OldDepartmentName = Name(departments, dto.OldDepartmentId);
            dto.NewDepartmentName = Name(departments, dto.NewDepartmentId);
            dto.OldUnitName = Name(units, dto.OldUnitId);
            dto.NewUnitName = Name(units, dto.NewUnitId);
            dto.OldSectionName = Name(sections, dto.OldSectionId);
            dto.NewSectionName = Name(sections, dto.NewSectionId);
        }

        return result;
    }
}
