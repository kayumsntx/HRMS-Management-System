using AutoMapper;
using HRMS.Application.DTOs.Leave;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class LeaveMappingProfile : Profile
{
    public LeaveMappingProfile()
    {
        // LeaveType
        CreateMap<LeaveType, LeaveTypeResponseDto>();
        CreateMap<LeaveTypeCreateDto, LeaveType>();
        CreateMap<LeaveTypeUpdateDto, LeaveType>();

        // Leave Balance
        CreateMap<EmployeeLeaveBalance, LeaveBalanceResponseDto>();
        CreateMap<LeaveBalanceCreateDto, EmployeeLeaveBalance>();

        // Leave
        CreateMap<EmployeeLeave, LeaveResponseDto>();
        CreateMap<LeaveApplyDto, EmployeeLeave>();
    }
}
