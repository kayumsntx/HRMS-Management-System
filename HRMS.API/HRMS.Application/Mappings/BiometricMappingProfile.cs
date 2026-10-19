using AutoMapper;
using HRMS.Application.DTOs.Biometric;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class BiometricMappingProfile : Profile
{
    public BiometricMappingProfile()
    {
        // Device
        CreateMap<BiometricDevice, BiometricDeviceResponseDto>();
        CreateMap<BiometricDeviceCreateDto, BiometricDevice>();
        CreateMap<BiometricDeviceUpdateDto, BiometricDevice>();

        // Punch
        CreateMap<BiometricPunch, BiometricPunchResponseDto>();
        CreateMap<BiometricPunchCreateDto, BiometricPunch>();
    }
}
