using AutoMapper;
using HRMS.Application.DTOs.EmployeeEducations;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Mappings
{
    public class EmployeeEducationMappingProfile : Profile
    {
        public EmployeeEducationMappingProfile()
        {
            CreateMap<EmployeeEducation, EmployeeEducationResponseDto>();
            CreateMap<EmployeeEducationCreateDto, EmployeeEducation>();
        }
    }
}
