using HRMS.Application.DTOs.EmployeeExtras;

namespace HRMS.Application.Interfaces.Services;

public interface IEmployeeSkillService : IEmployeeChildService<EmployeeSkillCreateDto, EmployeeSkillUpdateDto, EmployeeSkillResponseDto> { }
public interface IEmployeePreviousJobService : IEmployeeChildService<EmployeePreviousJobCreateDto, EmployeePreviousJobUpdateDto, EmployeePreviousJobResponseDto> { }
public interface IEmployeeAddressService : IEmployeeChildService<EmployeeAddressCreateDto, EmployeeAddressUpdateDto, EmployeeAddressResponseDto> { }
public interface IEmployeeBankAccountService : IEmployeeChildService<EmployeeBankAccountCreateDto, EmployeeBankAccountUpdateDto, EmployeeBankAccountResponseDto> { }
