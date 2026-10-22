using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.EmployeeExtras;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class EmployeeSkillService
    : EmployeeChildCrudService<EmployeeSkill, EmployeeSkillCreateDto, EmployeeSkillUpdateDto, EmployeeSkillResponseDto>, IEmployeeSkillService
{
    public EmployeeSkillService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateChildAsync(object dto, int employeeId, int? currentId)
    {
        var d = (EmployeeSkillBase)dto;

        var category = await UnitOfWork.Repository<SkillCategory>().GetByIdAsync(d.SkillCategoryId)
            ?? throw new NotFoundException(nameof(SkillCategory), d.SkillCategoryId);

        if (!category.IsActive)
            throw new BadRequestException("This Skill Category is inactive.");

        var excludeId = currentId ?? 0;
        var name = d.SkillName.Trim();
        if (await UnitOfWork.Repository<EmployeeSkill>().AnyAsync(x =>
                x.EmployeeId == employeeId && x.SkillCategoryId == d.SkillCategoryId && x.SkillName == name && x.IsActive && x.Id != excludeId))
            throw new BadRequestException("This skill is already recorded for the employee.");
    }

    protected override async Task<List<EmployeeSkillResponseDto>> MapAsync(IEnumerable<EmployeeSkill> items)
    {
        var categories = (await UnitOfWork.Repository<SkillCategory>().GetAllAsync()).ToDictionary(c => c.Id, c => c.CategoryName);
        var result = Mapper.Map<List<EmployeeSkillResponseDto>>(items.ToList());
        foreach (var dto in result)
            dto.SkillCategoryName = categories.GetValueOrDefault(dto.SkillCategoryId);
        return result;
    }
}

public class EmployeePreviousJobService
    : EmployeeChildCrudService<EmployeePreviousJob, EmployeePreviousJobCreateDto, EmployeePreviousJobUpdateDto, EmployeePreviousJobResponseDto>, IEmployeePreviousJobService
{
    public EmployeePreviousJobService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateChildAsync(object dto, int employeeId, int? currentId)
    {
        var d = (EmployeePreviousJobBase)dto;
        var employee = await UnitOfWork.Repository<Employee>().GetByIdAsync(employeeId);

        // A previous job must have ended before the employee joined this company.
        if (employee != null && d.ToDate.HasValue && d.ToDate.Value.Date > employee.DateOfJoining.Date)
            throw new BadRequestException("A previous job cannot end after the employee's joining date.");
    }
}

public class EmployeeAddressService
    : EmployeeChildCrudService<EmployeeAddress, EmployeeAddressCreateDto, EmployeeAddressUpdateDto, EmployeeAddressResponseDto>, IEmployeeAddressService
{
    public EmployeeAddressService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateChildAsync(object dto, int employeeId, int? currentId)
    {
        var d = (EmployeeAddressBase)dto;
        var type = char.ToUpper(d.AddressType[0]) + d.AddressType.Substring(1).ToLower();   // normalized: Present / Permanent
        d.AddressType = type;

        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<EmployeeAddress>().AnyAsync(x => x.EmployeeId == employeeId && x.AddressType == type && x.Id != excludeId))
            throw new BadRequestException($"A {type} address already exists for this employee. Update it instead.");
    }
}

public class EmployeeBankAccountService
    : EmployeeChildCrudService<EmployeeBankAccount, EmployeeBankAccountCreateDto, EmployeeBankAccountUpdateDto, EmployeeBankAccountResponseDto>, IEmployeeBankAccountService
{
    public EmployeeBankAccountService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateChildAsync(object dto, int employeeId, int? currentId)
    {
        var d = (EmployeeBankAccountBase)dto;

        var paymentType = await UnitOfWork.Repository<PaymentType>().GetByIdAsync(d.PaymentTypeId)
            ?? throw new NotFoundException(nameof(PaymentType), d.PaymentTypeId);

        if (!paymentType.IsActive)
            throw new BadRequestException("This Payment Type is inactive.");

        if (d.BankId.HasValue)
        {
            var bank = await UnitOfWork.Repository<Bank>().GetByIdAsync(d.BankId.Value)
                ?? throw new NotFoundException(nameof(Bank), d.BankId.Value);

            if (!bank.IsActive)
                throw new BadRequestException("This Bank is inactive.");
        }

        var excludeId = currentId ?? 0;
        var account = d.AccountNumber.Trim();
        var bankId = d.BankId;
        if (await UnitOfWork.Repository<EmployeeBankAccount>().AnyAsync(x =>
                x.EmployeeId == employeeId && x.AccountNumber == account && x.BankId == bankId && x.IsActive && x.Id != excludeId))
            throw new BadRequestException("This account is already recorded for the employee.");
    }

    protected override async Task<List<EmployeeBankAccountResponseDto>> MapAsync(IEnumerable<EmployeeBankAccount> items)
    {
        var banks = (await UnitOfWork.Repository<Bank>().GetAllAsync()).ToDictionary(b => b.Id, b => b.BankName);
        var types = (await UnitOfWork.Repository<PaymentType>().GetAllAsync()).ToDictionary(t => t.Id, t => t.PaymentTypeName);

        var result = Mapper.Map<List<EmployeeBankAccountResponseDto>>(items.ToList());
        foreach (var dto in result)
        {
            dto.PaymentTypeName = types.GetValueOrDefault(dto.PaymentTypeId);
            dto.BankName = dto.BankId.HasValue ? banks.GetValueOrDefault(dto.BankId.Value) : null;
        }
        return result;
    }
}
