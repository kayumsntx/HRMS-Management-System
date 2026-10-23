using AutoMapper;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.DTOs.Biometric;
using HRMS.Application.DTOs.Payroll;
using HRMS.Application.DTOs.Security;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class OperationsMappingProfile : Profile
{
    public OperationsMappingProfile()
    {
        CreateMap<LoginHistory, LoginHistoryResponseDto>();

        CreateMap<Menu, MenuResponseDto>();
        CreateMap<MenuCreateDto, Menu>();
        CreateMap<MenuUpdateDto, Menu>();

        CreateMap<ShortLeave, ShortLeaveResponseDto>();

        CreateMap<AttendanceMonthLock, AttendanceLockResponseDto>()
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsActive));

        CreateMap<BiometricSyncHistory, BiometricSyncHistoryResponseDto>();

        CreateMap<SalaryComponent, SalaryComponentResponseDto>();
        CreateMap<SalaryComponentCreateDto, SalaryComponent>();
        CreateMap<SalaryComponentUpdateDto, SalaryComponent>();
    }
}
