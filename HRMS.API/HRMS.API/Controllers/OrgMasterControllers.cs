using HRMS.Application.DTOs.Masters;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubSectionsController : CrudControllerBase<SubSectionCreateDto, SubSectionUpdateDto, SubSectionResponseDto>
{
    public SubSectionsController(ISubSectionService service) : base(service, "Sub section") { }
}

[ApiController]
[Route("api/[controller]")]
public class JobLocationsController : CrudControllerBase<JobLocationCreateDto, JobLocationUpdateDto, JobLocationResponseDto>
{
    public JobLocationsController(IJobLocationService service) : base(service, "Job location") { }
}

[ApiController]
[Route("api/[controller]")]
public class StaffCategoriesController : CrudControllerBase<StaffCategoryCreateDto, StaffCategoryUpdateDto, StaffCategoryResponseDto>
{
    public StaffCategoriesController(IStaffCategoryService service) : base(service, "Staff category") { }
}

[ApiController]
[Route("api/[controller]")]
public class SkillCategoriesController : CrudControllerBase<SkillCategoryCreateDto, SkillCategoryUpdateDto, SkillCategoryResponseDto>
{
    public SkillCategoriesController(ISkillCategoryService service) : base(service, "Skill category") { }
}

[ApiController]
[Route("api/[controller]")]
public class PaymentTypesController : CrudControllerBase<PaymentTypeCreateDto, PaymentTypeUpdateDto, PaymentTypeResponseDto>
{
    public PaymentTypesController(IPaymentTypeService service) : base(service, "Payment type") { }
}

[ApiController]
[Route("api/[controller]")]
public class BanksController : CrudControllerBase<BankCreateDto, BankUpdateDto, BankResponseDto>
{
    public BanksController(IBankService service) : base(service, "Bank") { }
}
