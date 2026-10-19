using HRMS.Application.DTOs.Shifts;

namespace HRMS.Application.Interfaces.Services;

public interface IShiftService
{
    Task<List<ShiftResponseDto>> GetAllAsync();
    Task<ShiftResponseDto> GetByIdAsync(int id);
    Task<ShiftResponseDto> CreateAsync(ShiftCreateDto dto);
    Task<ShiftResponseDto> UpdateAsync(int id, ShiftUpdateDto dto);
    Task DeleteAsync(int id);
}
