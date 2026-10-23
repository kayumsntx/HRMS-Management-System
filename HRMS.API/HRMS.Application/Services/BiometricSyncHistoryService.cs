using AutoMapper;
using HRMS.Application.DTOs.Biometric;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class BiometricSyncHistoryService : IBiometricSyncHistoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public BiometricSyncHistoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<BiometricSyncHistoryResponseDto>> GetAsync(int? deviceId, DateTime? fromDate, DateTime? toDate)
    {
        var from = fromDate?.Date ?? DateTime.UtcNow.Date.AddDays(-7);
        var to = (toDate?.Date ?? DateTime.UtcNow.Date).AddDays(1);

        var items = await _uow.Repository<BiometricSyncHistory>().FindAsync(h =>
            h.StartTime >= from && h.StartTime < to && (!deviceId.HasValue || h.BiometricDeviceId == deviceId.Value));

        var devices = (await _uow.Repository<BiometricDevice>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DeviceName);

        var result = _mapper.Map<List<BiometricSyncHistoryResponseDto>>(items.OrderByDescending(h => h.StartTime).Take(200).ToList());
        foreach (var dto in result)
            dto.DeviceName = devices.GetValueOrDefault(dto.BiometricDeviceId);

        return result;
    }
}
