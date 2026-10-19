using AutoMapper;
using HRMS.Application.DTOs.Payroll;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class PayrollMappingProfile : Profile
{
    public PayrollMappingProfile()
    {
        // Salary
        CreateMap<EmployeeSalary, EmployeeSalaryResponseDto>()
            .ForMember(dest => dest.AllowanceAmount, opt => opt.MapFrom(src => src.GrossSalary - src.BasicSalary));
        CreateMap<EmployeeSalaryCreateDto, EmployeeSalary>();

        // Payroll
        CreateMap<Payroll, PayrollResponseDto>();
        CreateMap<PayrollDetail, PayrollDetailResponseDto>();
    }
}
