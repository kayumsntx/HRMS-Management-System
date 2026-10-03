using AutoMapper;
using HRMS.Application.DTOs.Companies;
using HRMS.Application.DTOs.Units;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HRMS.Application.Mappings
{
    public class OrganizationMappingProfile : Profile 
    {
        public OrganizationMappingProfile()
        {


            // Company

            CreateMap <Company, CompanyResponseDto>();
            CreateMap <CompanyCreateDto, Company>();
            CreateMap <CompanyUpdateDto, Company>();

            //Unit
            CreateMap<Unit, UnitResponseDto>();
            
            CreateMap<UnitCreateDto, Unit>();
            CreateMap<UnitUpdateDto, Unit>();

            // in future- Unit, Department, Section, Designation, Grade, EmployeeStatus
            //  DTO/Entity 
        }
    }
}
