using HRMS.Application.DTOs.Career;
using HRMS.Application.DTOs.Common;

namespace HRMS.Application.Interfaces.Services;

public interface ICareerService
{
    Task<List<CareerChangeResponseDto>> GetAsync(int? employeeId, string? changeType, string? status);
    Task<List<CareerChangeResponseDto>> GetByEmployeeAsync(int employeeId);
    Task<CareerChangeResponseDto> GetByIdAsync(int id);
    Task<CareerChangeResponseDto> CreateAsync(CareerChangeCreateDto dto);
    Task<CareerChangeResponseDto> ApproveAsync(int id);
    Task<CareerChangeResponseDto> RejectAsync(int id, ReasonDto dto);
    Task<CareerChangeResponseDto> CancelAsync(int id);
    Task<int> ApplyDueAsync();
}
