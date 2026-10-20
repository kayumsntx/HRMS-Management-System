using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

// A manager can only act on leave requests of employees whose ReportingManager is that manager.
public class TeamLeaveService : ITeamLeaveService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILeaveService _leaveService;

    public TeamLeaveService(IUnitOfWork unitOfWork, ILeaveService leaveService)
    {
        _unitOfWork = unitOfWork;
        _leaveService = leaveService;
    }

    public async Task<List<LeaveResponseDto>> GetTeamLeavesAsync(int managerEmployeeId, string? status)
    {
        var teamIds = (await _unitOfWork.Repository<Employee>()
                .FindAsync(e => e.IsActive && e.ReportingManagerId == managerEmployeeId))
            .Select(e => e.Id)
            .ToHashSet();

        if (teamIds.Count == 0)
            return new List<LeaveResponseDto>();

        var leaves = await _leaveService.GetAllAsync(status);
        return leaves.Where(l => teamIds.Contains(l.EmployeeId)).ToList();
    }

    public async Task<LeaveResponseDto> ApproveAsync(int managerEmployeeId, int leaveId)
    {
        await EnsureTeamLeaveAsync(managerEmployeeId, leaveId);
        return await _leaveService.ApproveAsync(leaveId);
    }

    public async Task<LeaveResponseDto> RejectAsync(int managerEmployeeId, int leaveId, LeaveRejectDto dto)
    {
        await EnsureTeamLeaveAsync(managerEmployeeId, leaveId);
        return await _leaveService.RejectAsync(leaveId, dto);
    }

    private async Task EnsureTeamLeaveAsync(int managerEmployeeId, int leaveId)
    {
        var leave = await _leaveService.GetByIdAsync(leaveId);

        var isTeamMember = await _unitOfWork.Repository<Employee>()
            .AnyAsync(e => e.Id == leave.EmployeeId && e.ReportingManagerId == managerEmployeeId);

        // Reported as "not found" on purpose so other people's requests are not revealed.
        if (!isTeamMember)
            throw new NotFoundException(nameof(EmployeeLeave), leaveId);
    }
}
