using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Masters;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class SubSectionService : CrudService<SubSection, SubSectionCreateDto, SubSectionUpdateDto, SubSectionResponseDto>, ISubSectionService
{
    public SubSectionService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not SubSectionCreateDto d) return;

        var section = await UnitOfWork.Repository<Section>().GetByIdAsync(d.SectionId)
            ?? throw new NotFoundException(nameof(Section), d.SectionId);

        if (!section.IsActive)
            throw new BadRequestException("This Section is inactive.");

        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<SubSection>().AnyAsync(x => x.SectionId == d.SectionId && x.SubSectionName == d.SubSectionName && x.Id != excludeId))
            throw new BadRequestException($"Sub Section '{d.SubSectionName}' already exists in this Section.");
    }

    protected override async Task<List<SubSectionResponseDto>> MapAsync(IEnumerable<SubSection> items)
    {
        var list = items.ToList();
        var sections = (await UnitOfWork.Repository<Section>().GetAllAsync()).ToDictionary(s => s.Id, s => s.SectionName);

        var result = Mapper.Map<List<SubSectionResponseDto>>(list);
        foreach (var dto in result)
            dto.SectionName = sections.GetValueOrDefault(dto.SectionId);

        return result;
    }
}

public class JobLocationService : CrudService<JobLocation, JobLocationCreateDto, JobLocationUpdateDto, JobLocationResponseDto>, IJobLocationService
{
    public JobLocationService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not JobLocationCreateDto d) return;
        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<JobLocation>().AnyAsync(x => x.LocationName == d.LocationName && x.Id != excludeId))
            throw new BadRequestException($"Job location '{d.LocationName}' already exists.");
    }
}

public class StaffCategoryService : CrudService<StaffCategory, StaffCategoryCreateDto, StaffCategoryUpdateDto, StaffCategoryResponseDto>, IStaffCategoryService
{
    public StaffCategoryService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not StaffCategoryCreateDto d) return;
        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<StaffCategory>().AnyAsync(x => x.CategoryName == d.CategoryName && x.Id != excludeId))
            throw new BadRequestException($"Staff category '{d.CategoryName}' already exists.");
    }
}

public class SkillCategoryService : CrudService<SkillCategory, SkillCategoryCreateDto, SkillCategoryUpdateDto, SkillCategoryResponseDto>, ISkillCategoryService
{
    public SkillCategoryService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not SkillCategoryCreateDto d) return;
        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<SkillCategory>().AnyAsync(x => x.CategoryName == d.CategoryName && x.Id != excludeId))
            throw new BadRequestException($"Skill category '{d.CategoryName}' already exists.");
    }
}

public class PaymentTypeService : CrudService<PaymentType, PaymentTypeCreateDto, PaymentTypeUpdateDto, PaymentTypeResponseDto>, IPaymentTypeService
{
    public PaymentTypeService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not PaymentTypeCreateDto d) return;
        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<PaymentType>().AnyAsync(x => x.PaymentTypeName == d.PaymentTypeName && x.Id != excludeId))
            throw new BadRequestException($"Payment type '{d.PaymentTypeName}' already exists.");
    }
}

public class BankService : CrudService<Bank, BankCreateDto, BankUpdateDto, BankResponseDto>, IBankService
{
    public BankService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not BankCreateDto d) return;
        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<Bank>().AnyAsync(x => x.BankName == d.BankName && x.Id != excludeId))
            throw new BadRequestException($"Bank '{d.BankName}' already exists.");
    }
}
