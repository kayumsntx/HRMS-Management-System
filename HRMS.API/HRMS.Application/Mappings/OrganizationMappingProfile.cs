using AutoMapper;
using HRMS.Application.DTOs.Companies;
using HRMS.Application.DTOs.Departments;
using HRMS.Application.DTOs.Designations;
using HRMS.Application.DTOs.Employees;
using HRMS.Application.DTOs.EmployeeStatuses;
using HRMS.Application.DTOs.Grades;
using HRMS.Application.DTOs.Sections;
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

            // Department
            CreateMap<Department, DepartmentResponseDto>();
            CreateMap<DepartmentCreateDto, Department>();
            CreateMap<DepartmentUpdateDto, Department>();

            // Section
            CreateMap<Section, SectionResponseDto>();
            CreateMap<SectionCreateDto, Section>();
            CreateMap<SectionUpdateDto, Section>();


            // Designation
            CreateMap<Designation, DesignationResponseDto>();
            CreateMap<DesignationCreateDto, Designation>();
            CreateMap<DesignationUpdateDto, Designation>();

            // Grade
            CreateMap<Grade, GradeResponseDto>();
            CreateMap<GradeCreateDto, Grade>();
            CreateMap<GradeUpdateDto, Grade>();

            // EmployeeStatus
            CreateMap<EmployeeStatus, EmployeeStatusResponseDto>();
            CreateMap<EmployeeStatusCreateDto, EmployeeStatus>();
            CreateMap<EmployeeStatusUpdateDto, EmployeeStatus>();

            // Entity → DTO 
            CreateMap<Employee, EmployeeResponseDto>();
            CreateMap<Employee, EmployeeListDto>();

            // DTO → Entity
            CreateMap<EmployeeCreateDto, Employee>();
            CreateMap<EmployeeUpdateDto, Employee>();

      

            // in future- Unit, Department, Section, Designation, Grade, EmployeeStatus
            //  DTO/Entity 
        }
    }
}
