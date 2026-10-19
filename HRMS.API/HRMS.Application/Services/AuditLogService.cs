using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.AuditLogs;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class AuditLogService : IAuditLogService
{
    private const int MaxResults = 500;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AuditLogService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // Dates are compared in UTC. Default range = last 7 days. Result is capped at 500 newest rows.
    public async Task<List<AuditLogResponseDto>> GetAsync(AuditLogFilterDto filter)
    {
        var from = filter.FromDate?.Date ?? DateTime.UtcNow.Date.AddDays(-7);
        var to = (filter.ToDate?.Date ?? DateTime.UtcNow.Date).AddDays(1);

        var userId = filter.UserId;
        var table = string.IsNullOrWhiteSpace(filter.TableName) ? null : filter.TableName.Trim();
        var action = string.IsNullOrWhiteSpace(filter.ActionType) ? null : filter.ActionType.Trim();
        var recordId = string.IsNullOrWhiteSpace(filter.RecordId) ? null : filter.RecordId.Trim();

        var logs = await _unitOfWork.Repository<AuditLog>().FindAsync(l =>
            l.ActivityDate >= from && l.ActivityDate < to &&
            (!userId.HasValue || l.UserId == userId) &&
            (table == null || l.TableName == table) &&
            (action == null || l.ActionType == action) &&
            (recordId == null || l.RecordId == recordId));

        var latest = logs.OrderByDescending(l => l.ActivityDate).ThenByDescending(l => l.Id).Take(MaxResults).ToList();
        return _mapper.Map<List<AuditLogResponseDto>>(latest);
    }

    public async Task<AuditLogResponseDto> GetByIdAsync(int id)
    {
        var log = await _unitOfWork.Repository<AuditLog>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(AuditLog), id);

        return _mapper.Map<AuditLogResponseDto>(log);
    }
}
