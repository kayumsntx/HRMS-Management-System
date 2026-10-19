using HRMS.Application.DTOs.Biometric;

namespace HRMS.Application.Interfaces.Services;

public interface IBiometricDeviceService
{
    Task<List<BiometricDeviceResponseDto>> GetAllAsync();
    Task<BiometricDeviceResponseDto> GetByIdAsync(int id);
    Task<BiometricDeviceResponseDto> CreateAsync(BiometricDeviceCreateDto dto);
    Task<BiometricDeviceResponseDto> UpdateAsync(int id, BiometricDeviceUpdateDto dto);
    Task DeleteAsync(int id);
}
