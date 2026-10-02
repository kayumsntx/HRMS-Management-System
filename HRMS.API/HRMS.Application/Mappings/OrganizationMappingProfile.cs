using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

using HRMS.Application.DTOs.Companies;
using HRMS.Domain.Entities;
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

            // in future- Unit, Department, Section, Designation, Grade, EmployeeStatus
            //  DTO/Entity 
        }
    }
}
