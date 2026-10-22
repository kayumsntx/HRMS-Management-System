using HRMS.Application.DTOs.EmployeeExtras;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeSkillsController : EmployeeChildControllerBase<EmployeeSkillCreateDto, EmployeeSkillUpdateDto, EmployeeSkillResponseDto>
{
    public EmployeeSkillsController(IEmployeeSkillService service) : base(service, "Skill") { }
}

[ApiController]
[Route("api/[controller]")]
public class EmployeePreviousJobsController : EmployeeChildControllerBase<EmployeePreviousJobCreateDto, EmployeePreviousJobUpdateDto, EmployeePreviousJobResponseDto>
{
    public EmployeePreviousJobsController(IEmployeePreviousJobService service) : base(service, "Previous job") { }
}

[ApiController]
[Route("api/[controller]")]
public class EmployeeAddressesController : EmployeeChildControllerBase<EmployeeAddressCreateDto, EmployeeAddressUpdateDto, EmployeeAddressResponseDto>
{
    public EmployeeAddressesController(IEmployeeAddressService service) : base(service, "Address") { }
}

[ApiController]
[Route("api/[controller]")]
public class EmployeeBankAccountsController : EmployeeChildControllerBase<EmployeeBankAccountCreateDto, EmployeeBankAccountUpdateDto, EmployeeBankAccountResponseDto>
{
    public EmployeeBankAccountsController(IEmployeeBankAccountService service) : base(service, "Bank account") { }
}
