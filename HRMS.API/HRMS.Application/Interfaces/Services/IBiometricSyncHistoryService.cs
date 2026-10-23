using HRMS.Application.DTOs.Biometric;

namespace HRMS.Application.Interfaces.Services;

public interface IBiometricSyncHistoryService
{
    Task<List<BiometricSyncHistoryResponseDto>> GetAsync(int? deviceId, DateTime? fromDate, DateTime? toDate);
}
