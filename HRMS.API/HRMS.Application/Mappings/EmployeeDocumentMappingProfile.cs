using AutoMapper;
using HRMS.Application.DTOs.EmployeeDocuments;
using HRMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application.Mappings
{
    public class EmployeeDocumentMappingProfile : Profile
    {
        public EmployeeDocumentMappingProfile()
        {
            CreateMap<EmployeeDocument, EmployeeDocumentResponseDto>();
            CreateMap<EmployeeDocumentCreateDto, EmployeeDocument>();
        }
    }
}
