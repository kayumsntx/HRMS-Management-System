using HRMS.Application.DTOs.Biometric;

namespace HRMS.Application.Interfaces.Services;

public interface IBiometricPunchService
{
    Task<List<BiometricPunchResponseDto>> GetAsync(int? employeeId, int? deviceId, DateTime? fromDate, DateTime? toDate, bool? isProcessed);
    Task<BiometricPunchResponseDto> GetByIdAsync(int id);
    Task<BiometricPunchResponseDto> CreateAsync(BiometricPunchCreateDto dto);
    Task<BiometricPunchBulkResultDto> BulkCreateAsync(BiometricPunchBulkCreateDto dto);
    Task DeleteAsync(int id);
}
