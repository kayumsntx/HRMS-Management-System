using HRMS.Application.DTOs.Masters;

namespace HRMS.Application.Interfaces.Services;

public interface ISubSectionService : ICrudService<SubSectionCreateDto, SubSectionUpdateDto, SubSectionResponseDto> { }
public interface IJobLocationService : ICrudService<JobLocationCreateDto, JobLocationUpdateDto, JobLocationResponseDto> { }
public interface IStaffCategoryService : ICrudService<StaffCategoryCreateDto, StaffCategoryUpdateDto, StaffCategoryResponseDto> { }
public interface ISkillCategoryService : ICrudService<SkillCategoryCreateDto, SkillCategoryUpdateDto, SkillCategoryResponseDto> { }
public interface IPaymentTypeService : ICrudService<PaymentTypeCreateDto, PaymentTypeUpdateDto, PaymentTypeResponseDto> { }
public interface IBankService : ICrudService<BankCreateDto, BankUpdateDto, BankResponseDto> { }
