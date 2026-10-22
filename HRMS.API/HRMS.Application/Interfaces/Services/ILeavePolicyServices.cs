using HRMS.Application.DTOs.Leave;

namespace HRMS.Application.Interfaces.Services;

public interface ILeavePolicyService : ICrudService<LeavePolicyCreateDto, LeavePolicyUpdateDto, LeavePolicyResponseDto> { }

public interface ILeaveBalanceGenerationService
{
    Task<LeaveBalanceGenerateResultDto> GenerateAsync(LeaveBalanceGenerateDto dto);
}
