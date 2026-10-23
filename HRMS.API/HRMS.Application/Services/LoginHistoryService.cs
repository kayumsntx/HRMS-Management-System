using AutoMapper;
using HRMS.Application.DTOs.Security;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class LoginHistoryService : ILoginHistoryService
{
    private const int MaxResults = 500;

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public LoginHistoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    // Times are UTC. Default range = last 7 days. The newest 500 rows are returned.
    public async Task<List<LoginHistoryResponseDto>> GetAsync(LoginHistoryFilterDto filter)
    {
        var from = filter.FromDate?.Date ?? DateTime.UtcNow.Date.AddDays(-7);
        var to = (filter.ToDate?.Date ?? DateTime.UtcNow.Date).AddDays(1);
        var name = string.IsNullOrWhiteSpace(filter.UserName) ? null : filter.UserName.Trim();
        var success = filter.IsSuccess;

        var items = await _uow.Repository<LoginHistory>().FindAsync(h =>
            h.LoginTime >= from && h.LoginTime < to &&
            (name == null || h.UserName == name) &&
            (!success.HasValue || h.IsSuccess == success.Value));

        var latest = items.OrderByDescending(h => h.LoginTime).ThenByDescending(h => h.Id).Take(MaxResults).ToList();
        return _mapper.Map<List<LoginHistoryResponseDto>>(latest);
    }
}
