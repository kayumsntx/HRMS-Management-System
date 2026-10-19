using AutoMapper;
using HRMS.Application.DTOs.AuditLogs;
using HRMS.Application.DTOs.Holidays;
using HRMS.Application.DTOs.IdCards;
using HRMS.Application.DTOs.Notifications;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class SystemMappingProfile : Profile
{
    public SystemMappingProfile()
    {
        // Holiday
        CreateMap<Holiday, HolidayResponseDto>()
            .ForMember(dest => dest.DayName, opt => opt.MapFrom(src => src.HolidayDate.DayOfWeek.ToString()));
        CreateMap<HolidayCreateDto, Holiday>();
        CreateMap<HolidayUpdateDto, Holiday>();

        // Audit Log
        CreateMap<AuditLog, AuditLogResponseDto>();

        // Notification
        CreateMap<Notification, NotificationResponseDto>();

        // ID Card
        CreateMap<IdCard, IdCardResponseDto>();
    }
}
