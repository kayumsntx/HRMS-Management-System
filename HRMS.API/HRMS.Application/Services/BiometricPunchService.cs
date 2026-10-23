using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Biometric;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class BiometricPunchService : IBiometricPunchService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BiometricPunchService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<BiometricPunchResponseDto>> GetAsync(
        int? employeeId, int? deviceId, DateTime? fromDate, DateTime? toDate, bool? isProcessed)
    {
        var startDate = fromDate?.Date;
        var endDate = toDate?.Date.AddDays(1);

        // Without any date filter, only the last 7 days are returned (punch tables grow very fast).
        if (!startDate.HasValue && !endDate.HasValue)
            startDate = DateTime.Now.Date.AddDays(-7);

        var punches = await _unitOfWork.Repository<BiometricPunch>().FindAsync(p =>
            (!employeeId.HasValue || p.EmployeeId == employeeId) &&
            (!deviceId.HasValue || p.BiometricDeviceId == deviceId) &&
            (!startDate.HasValue || p.PunchDateTime >= startDate) &&
            (!endDate.HasValue || p.PunchDateTime < endDate) &&
            (!isProcessed.HasValue || p.IsProcessed == isProcessed));

        var limited = punches.OrderByDescending(p => p.PunchDateTime).Take(1000).ToList();
        return await MapAsync(limited);
    }

    public async Task<BiometricPunchResponseDto> GetByIdAsync(int id)
    {
        var punch = await _unitOfWork.Repository<BiometricPunch>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(BiometricPunch), id);

        return (await MapAsync(new[] { punch })).First();
    }

    public async Task<BiometricPunchResponseDto> CreateAsync(BiometricPunchCreateDto dto)
    {
        var punchTime = TrimToSeconds(dto.PunchDateTime);

        var device = await _unitOfWork.Repository<BiometricDevice>().GetByIdAsync(dto.BiometricDeviceId)
            ?? throw new NotFoundException(nameof(BiometricDevice), dto.BiometricDeviceId);

        if (!device.IsActive)
            throw new BadRequestException("This Biometric Device is inactive.");

        var duplicate = await _unitOfWork.Repository<BiometricPunch>().AnyAsync(p =>
            p.BiometricDeviceId == dto.BiometricDeviceId &&
            p.FingerprintUserId == dto.FingerprintUserId &&
            p.PunchDateTime == punchTime);

        if (duplicate)
            throw new BadRequestException("This punch already exists for the given device, user and time.");

        var employee = (await _unitOfWork.Repository<Employee>()
            .FindAsync(e => e.FingerprintUserId == dto.FingerprintUserId)).FirstOrDefault();

        var punch = _mapper.Map<BiometricPunch>(dto);
        punch.PunchDateTime = punchTime;
        punch.PunchType = NormalizePunchType(dto.PunchType);
        punch.EmployeeId = employee?.Id;

        device.LastSyncDate = DateTime.UtcNow;
        _unitOfWork.Repository<BiometricDevice>().Update(device);

        await _unitOfWork.Repository<BiometricPunch>().AddAsync(punch);
        await _unitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { punch })).First();
    }

    public async Task<BiometricPunchBulkResultDto> BulkCreateAsync(BiometricPunchBulkCreateDto dto)
    {
        var result = new BiometricPunchBulkResultDto { Total = dto.Punches.Count };

        var activeDevices = (await _unitOfWork.Repository<BiometricDevice>().GetAllAsync())
            .Where(d => d.IsActive)
            .ToDictionary(d => d.Id);

        var mappedEmployees = await _unitOfWork.Repository<Employee>().FindAsync(e => e.FingerprintUserId != null);
        var fingerprintMap = mappedEmployees.ToDictionary(e => e.FingerprintUserId!, e => e.Id);

        var validPunches = new List<BiometricPunchCreateDto>();
        foreach (var punch in dto.Punches)
        {
            if (!activeDevices.ContainsKey(punch.BiometricDeviceId))
            {
                result.InvalidDevice++;
                continue;
            }
            validPunches.Add(punch);
        }

        if (validPunches.Count == 0)
            return result;

        var minTime = validPunches.Min(p => TrimToSeconds(p.PunchDateTime));
        var maxTime = validPunches.Max(p => TrimToSeconds(p.PunchDateTime));
        var deviceIds = validPunches.Select(p => p.BiometricDeviceId).Distinct().ToList();

        var existing = await _unitOfWork.Repository<BiometricPunch>().FindAsync(p =>
            p.PunchDateTime >= minTime && p.PunchDateTime <= maxTime && deviceIds.Contains(p.BiometricDeviceId));

        var seen = new HashSet<(int DeviceId, string FingerprintUserId, DateTime Time)>(
            existing.Select(p => (p.BiometricDeviceId, p.FingerprintUserId, p.PunchDateTime)));

        var toInsert = new List<BiometricPunch>();

        foreach (var item in validPunches)
        {
            var time = TrimToSeconds(item.PunchDateTime);

            if (!seen.Add((item.BiometricDeviceId, item.FingerprintUserId, time)))
            {
                result.Duplicates++;
                continue;
            }

            int? employeeId = fingerprintMap.TryGetValue(item.FingerprintUserId, out var mappedId) ? mappedId : (int?)null;
            if (employeeId is null)
                result.UnmappedEmployee++;

            var entity = _mapper.Map<BiometricPunch>(item);
            entity.PunchDateTime = time;
            entity.PunchType = NormalizePunchType(item.PunchType);
            entity.EmployeeId = employeeId;
            toInsert.Add(entity);
        }

        if (toInsert.Count > 0)
        {
            await _unitOfWork.Repository<BiometricPunch>().AddRangeAsync(toInsert);

            foreach (var deviceId in toInsert.Select(p => p.BiometricDeviceId).Distinct())
            {
                var device = activeDevices[deviceId];
                await _unitOfWork.Repository<BiometricSyncHistory>().AddAsync(new BiometricSyncHistory
                {
                    BiometricDeviceId = deviceId,
                    SyncType = "PunchImport",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    TotalRecords = validPunches.Count(p => p.BiometricDeviceId == deviceId),
                    NewRecords = toInsert.Count(p => p.BiometricDeviceId == deviceId),
                    Status = "Success"
                });

                device.LastSyncDate = DateTime.UtcNow;
                _unitOfWork.Repository<BiometricDevice>().Update(device);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        result.Inserted = toInsert.Count;
        return result;
    }

    public async Task DeleteAsync(int id)
    {
        var punch = await _unitOfWork.Repository<BiometricPunch>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(BiometricPunch), id);

        if (punch.IsProcessed)
            throw new BadRequestException("A punch that has already been processed into attendance cannot be deleted.");

        _unitOfWork.Repository<BiometricPunch>().Delete(punch);
        await _unitOfWork.SaveChangesAsync();
    }

    // ---------------- Private Helpers ----------------

    private static DateTime TrimToSeconds(DateTime value)
        => new DateTime(value.Ticks - (value.Ticks % TimeSpan.TicksPerSecond), value.Kind);

    private static string? NormalizePunchType(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : Enum.Parse<PunchType>(value, true).ToString();

    private async Task<List<BiometricPunchResponseDto>> MapAsync(IEnumerable<BiometricPunch> punches)
    {
        var list = punches.ToList();

        var devices = (await _unitOfWork.Repository<BiometricDevice>().GetAllAsync())
            .ToDictionary(d => d.Id, d => d.DeviceName);

        var employeeIds = list.Where(p => p.EmployeeId.HasValue).Select(p => p.EmployeeId!.Value).Distinct().ToList();
        var employees = employeeIds.Count == 0
            ? new Dictionary<int, Employee>()
            : (await _unitOfWork.Repository<Employee>().FindAsync(e => employeeIds.Contains(e.Id))).ToDictionary(e => e.Id);

        var result = _mapper.Map<List<BiometricPunchResponseDto>>(list);
        foreach (var dto in result)
        {
            dto.DeviceName = devices.GetValueOrDefault(dto.BiometricDeviceId);

            if (dto.EmployeeId.HasValue && employees.TryGetValue(dto.EmployeeId.Value, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }
        }

        return result;
    }
}
