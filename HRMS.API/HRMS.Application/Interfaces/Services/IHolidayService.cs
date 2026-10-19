using HRMS.Application.DTOs.Holidays;

namespace HRMS.Application.Interfaces.Services;

public interface IHolidayService
{
    Task<List<HolidayResponseDto>> GetAllAsync(int? year);
    Task<HolidayResponseDto> GetByIdAsync(int id);
    Task<HolidayResponseDto> CreateAsync(HolidayCreateDto dto);
    Task<HolidayResponseDto> UpdateAsync(int id, HolidayUpdateDto dto);
    Task DeleteAsync(int id);
}
