using AutoMapper;
using HRMS.Application.DTOs.EmployeeExtras;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.DTOs.Masters;
using HRMS.Application.DTOs.Payroll;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class ExtraModulesMappingProfile : Profile
{
    public ExtraModulesMappingProfile()
    {
        // ---- Organization masters ----
        CreateMap<SubSection, SubSectionResponseDto>();
        CreateMap<SubSectionCreateDto, SubSection>();
        CreateMap<SubSectionUpdateDto, SubSection>();

        CreateMap<JobLocation, JobLocationResponseDto>();
        CreateMap<JobLocationCreateDto, JobLocation>();
        CreateMap<JobLocationUpdateDto, JobLocation>();

        CreateMap<StaffCategory, StaffCategoryResponseDto>();
        CreateMap<StaffCategoryCreateDto, StaffCategory>();
        CreateMap<StaffCategoryUpdateDto, StaffCategory>();

        CreateMap<SkillCategory, SkillCategoryResponseDto>();
        CreateMap<SkillCategoryCreateDto, SkillCategory>();
        CreateMap<SkillCategoryUpdateDto, SkillCategory>();

        CreateMap<PaymentType, PaymentTypeResponseDto>();
        CreateMap<PaymentTypeCreateDto, PaymentType>();
        CreateMap<PaymentTypeUpdateDto, PaymentType>();

        CreateMap<Bank, BankResponseDto>();
        CreateMap<BankCreateDto, Bank>();
        CreateMap<BankUpdateDto, Bank>();

        // ---- Employee extras (update DTOs have no EmployeeId, so the owner can never change) ----
        CreateMap<EmployeeSkill, EmployeeSkillResponseDto>();
        CreateMap<EmployeeSkillCreateDto, EmployeeSkill>();
        CreateMap<EmployeeSkillUpdateDto, EmployeeSkill>();

        CreateMap<EmployeePreviousJob, EmployeePreviousJobResponseDto>();
        CreateMap<EmployeePreviousJobCreateDto, EmployeePreviousJob>();
        CreateMap<EmployeePreviousJobUpdateDto, EmployeePreviousJob>();

        CreateMap<EmployeeAddress, EmployeeAddressResponseDto>();
        CreateMap<EmployeeAddressCreateDto, EmployeeAddress>();
        CreateMap<EmployeeAddressUpdateDto, EmployeeAddress>();

        CreateMap<EmployeeBankAccount, EmployeeBankAccountResponseDto>();
        CreateMap<EmployeeBankAccountCreateDto, EmployeeBankAccount>();
        CreateMap<EmployeeBankAccountUpdateDto, EmployeeBankAccount>();

        // ---- Leave policy ----
        CreateMap<LeavePolicy, LeavePolicyResponseDto>();
        CreateMap<LeavePolicyCreateDto, LeavePolicy>();
        CreateMap<LeavePolicyUpdateDto, LeavePolicy>();

        // ---- Bonus / Loan ----
        CreateMap<Bonus, BonusResponseDto>();
        CreateMap<BonusCreateDto, Bonus>();
        CreateMap<BonusUpdateDto, Bonus>();

        CreateMap<Loan, LoanResponseDto>();
    }
}
