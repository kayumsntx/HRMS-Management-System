using AutoMapper;
using HRMS.Application.DTOs.ShiftAssignments;
using HRMS.Application.DTOs.Shifts;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class ShiftMappingProfile : Profile
{
    public ShiftMappingProfile()
    {
        // Shift
        CreateMap<Shift, ShiftResponseDto>();
        CreateMap<ShiftCreateDto, Shift>();
        CreateMap<ShiftUpdateDto, Shift>();

        // ShiftAssignment
        CreateMap<ShiftAssignment, ShiftAssignmentResponseDto>();
        CreateMap<ShiftAssignmentCreateDto, ShiftAssignment>();
    }
}
