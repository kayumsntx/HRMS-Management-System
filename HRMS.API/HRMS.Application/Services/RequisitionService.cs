using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Requisitions;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class RequisitionService : IRequisitionService
{
    private const string Prefix = "RQ";
    private static readonly string Pending = RequisitionStatus.Pending.ToString();
    private static readonly string Approved = RequisitionStatus.Approved.ToString();
    private static readonly string Rejected = RequisitionStatus.Rejected.ToString();
    private static readonly string Cancelled = RequisitionStatus.Cancelled.ToString();
    private static readonly string[] ApproverRoles = { "Admin", "HR" };

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationPublisher _notifier;

    public RequisitionService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser, INotificationPublisher notifier)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
        _notifier = notifier;
    }

    public async Task<List<RequisitionResponseDto>> GetAsync(string? status, int? typeId, int? employeeId, DateTime? fromDate, DateTime? toDate)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<RequisitionStatus>(status, "status");
        var from = fromDate?.Date;
        var to = toDate?.Date.AddDays(1);

        var items = await _uow.Repository<Requisition>().FindAsync(r =>
            (statusText == null || r.Status == statusText) &&
            (!typeId.HasValue || r.RequisitionTypeId == typeId.Value) &&
            (!employeeId.HasValue || r.EmployeeId == employeeId.Value) &&
            (!from.HasValue || r.RequisitionDate >= from.Value) &&
            (!to.HasValue || r.RequisitionDate < to.Value));

        return await MapAsync(items.OrderByDescending(r => r.RequisitionDate).ThenByDescending(r => r.Id).Take(500));
    }

    public async Task<RequisitionResponseDto> GetByIdAsync(int id)
        => (await MapAsync(new[] { await LoadAsync(id) })).First();

    public async Task<RequisitionResponseDto> CreateAsync(RequisitionCreateDto dto)
    {
        var employee = await _uow.Repository<Employee>().GetByIdAsync(dto.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId);

        if (!employee.IsActive)
            throw new BadRequestException("A requisition cannot be created for an inactive employee.");

        var type = await _uow.Repository<RequisitionType>().GetByIdAsync(dto.RequisitionTypeId)
            ?? throw new NotFoundException(nameof(RequisitionType), dto.RequisitionTypeId);

        if (!type.IsActive)
            throw new BadRequestException("This Requisition Type is inactive.");

        var date = (dto.RequisitionDate ?? DateTime.Now).Date;
        if (date < DateTime.Now.Date.AddDays(-30) || date > DateTime.Now.Date.AddDays(90))
            throw new BadRequestException("Requisition Date must be within the last 30 days and the next 90 days.");

        var numbers = (await _uow.Repository<Requisition>().FindAsync(r => r.RequisitionNo.StartsWith(Prefix))).Select(r => r.RequisitionNo);

        var requisition = new Requisition
        {
            RequisitionNo = NumberHelper.Next(Prefix, numbers, 6),
            RequisitionTypeId = dto.RequisitionTypeId,
            EmployeeId = dto.EmployeeId,
            RequisitionDate = date,
            Purpose = dto.Purpose?.Trim(),
            Status = Pending,
            Details = dto.Items.Select(i => new RequisitionDetail
            {
                ItemName = i.ItemName.Trim(),
                Quantity = i.Quantity,
                Unit = i.Unit?.Trim(),
                Remarks = i.Remarks?.Trim()
            }).ToList()
        };

        await _uow.Repository<Requisition>().AddAsync(requisition);

        await _notifier.NotifyRolesAsync(ApproverRoles, NotificationCategory.Approval, "New requisition",
            $"{employee.FullName} ({employee.EmployeeCode}) submitted {type.TypeName} requisition {requisition.RequisitionNo}.",
            "/requisitions", _currentUser.UserId);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(requisition.Id);
    }

    public async Task<RequisitionResponseDto> ApproveAsync(int id)
    {
        var requisition = await LoadAsync(id);
        if (requisition.Status != Pending)
            throw new BadRequestException($"Only a Pending requisition can be approved. Current status: {requisition.Status}.");

        requisition.Status = Approved;
        requisition.ApprovedBy = _currentUser.UserId;
        requisition.ApprovedDate = DateTime.UtcNow;
        requisition.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Requisition>().Update(requisition);

        await _notifier.NotifyEmployeeAsync(requisition.EmployeeId, NotificationCategory.Approval, "Requisition approved",
            $"Your requisition {requisition.RequisitionNo} has been approved.", "/requisitions");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<RequisitionResponseDto> RejectAsync(int id, ReasonDto dto)
    {
        var requisition = await LoadAsync(id);
        if (requisition.Status != Pending)
            throw new BadRequestException($"Only a Pending requisition can be rejected. Current status: {requisition.Status}.");

        requisition.Status = Rejected;
        requisition.RejectionReason = dto.Reason.Trim();
        requisition.ApprovedBy = _currentUser.UserId;
        requisition.ApprovedDate = DateTime.UtcNow;
        requisition.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Requisition>().Update(requisition);

        await _notifier.NotifyEmployeeAsync(requisition.EmployeeId, NotificationCategory.Approval, "Requisition rejected",
            $"Your requisition {requisition.RequisitionNo} was rejected. Reason: {dto.Reason.Trim()}", "/requisitions");

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<RequisitionResponseDto> CancelAsync(int id)
    {
        var requisition = await LoadAsync(id);
        if (requisition.Status != Pending)
            throw new BadRequestException($"Only a Pending requisition can be cancelled. Current status: {requisition.Status}.");

        requisition.Status = Cancelled;
        requisition.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Requisition>().Update(requisition);

        await _uow.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    // ---------------- Private Helpers ----------------

    private async Task<Requisition> LoadAsync(int id)
        => await _uow.Repository<Requisition>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(Requisition), id);

    private async Task<List<RequisitionResponseDto>> MapAsync(IEnumerable<Requisition> items)
    {
        var list = items.ToList();
        var ids = list.Select(r => r.Id).ToList();
        var employeeIds = list.Select(r => r.EmployeeId).Distinct().ToList();

        var details = ids.Count == 0 ? new List<RequisitionDetail>() : await _uow.Repository<RequisitionDetail>().FindAsync(d => ids.Contains(d.RequisitionId));
        var employees = employeeIds.Count == 0 ? new Dictionary<int, Employee>() : (await _uow.Repository<Employee>().FindAsync(e => employeeIds.Contains(e.Id))).ToDictionary(e => e.Id);
        var types = (await _uow.Repository<RequisitionType>().GetAllAsync()).ToDictionary(t => t.Id, t => t.TypeName);

        var result = _mapper.Map<List<RequisitionResponseDto>>(list);
        foreach (var dto in result)
        {
            dto.TypeName = types.GetValueOrDefault(dto.RequisitionTypeId);
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }
            dto.Items = _mapper.Map<List<RequisitionItemResponseDto>>(details.Where(d => d.RequisitionId == dto.Id).OrderBy(d => d.Id).ToList());
        }

        return result;
    }
}
