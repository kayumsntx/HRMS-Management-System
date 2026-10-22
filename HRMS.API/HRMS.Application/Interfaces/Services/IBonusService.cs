using HRMS.Application.DTOs.Payroll;

namespace HRMS.Application.Interfaces.Services;

public interface IBonusService : IEmployeeChildService<BonusCreateDto, BonusUpdateDto, BonusResponseDto>
{
    Task<List<BonusResponseDto>> GetByMonthAsync(int year, int month);
}
