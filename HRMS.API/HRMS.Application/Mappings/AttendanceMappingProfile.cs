using AutoMapper;
using HRMS.Application.DTOs.Attendance;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class AttendanceMappingProfile : Profile
{
    public AttendanceMappingProfile()
    {
        CreateMap<EmployeeAttendance, AttendanceResponseDto>();
    }
}
