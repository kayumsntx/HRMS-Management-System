using AutoMapper;
using HRMS.Application.DTOs.EmployeeNominees;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Mappings
{
    public class EmployeeNomineeMappingProfile : Profile
    {
        public EmployeeNomineeMappingProfile()
        {
            CreateMap<EmployeeNominee, EmployeeNomineeResponseDto>();
            CreateMap<EmployeeNomineeCreateDto, EmployeeNominee>();
        }
    }
}
