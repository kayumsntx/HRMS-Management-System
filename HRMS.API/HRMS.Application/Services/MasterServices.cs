using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Masters;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;

namespace HRMS.Application.Services;

public class RequisitionTypeService : CrudService<RequisitionType, RequisitionTypeCreateDto, RequisitionTypeUpdateDto, RequisitionTypeResponseDto>, IRequisitionTypeService
{
    public RequisitionTypeService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not RequisitionTypeCreateDto d) return;
        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<RequisitionType>().AnyAsync(x => x.TypeName == d.TypeName && x.Id != excludeId))
            throw new BadRequestException($"Requisition type '{d.TypeName}' already exists.");
    }
}

public class TrainingProgramService : CrudService<TrainingProgram, TrainingProgramCreateDto, TrainingProgramUpdateDto, TrainingProgramResponseDto>, ITrainingProgramService
{
    public TrainingProgramService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not TrainingProgramCreateDto d) return;
        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<TrainingProgram>().AnyAsync(x => x.ProgramName == d.ProgramName && x.Id != excludeId))
            throw new BadRequestException($"Training program '{d.ProgramName}' already exists.");
    }
}

public class TrainerService : CrudService<Trainer, TrainerCreateDto, TrainerUpdateDto, TrainerResponseDto>, ITrainerService
{
    public TrainerService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not TrainerCreateDto d) return;
        if (d.EmployeeId.HasValue && await UnitOfWork.Repository<Employee>().GetByIdAsync(d.EmployeeId.Value) is null)
            throw new NotFoundException(nameof(Employee), d.EmployeeId.Value);
    }
}

public class PerformancePeriodService : CrudService<PerformancePeriod, PerformancePeriodCreateDto, PerformancePeriodUpdateDto, PerformancePeriodResponseDto>, IPerformancePeriodService
{
    public PerformancePeriodService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not PerformancePeriodCreateDto d) return;
        var excludeId = currentId ?? 0;
        if (await UnitOfWork.Repository<PerformancePeriod>().AnyAsync(x => x.PeriodName == d.PeriodName && x.Id != excludeId))
            throw new BadRequestException($"Performance period '{d.PeriodName}' already exists.");
    }
}

public class KpiService : CrudService<Kpi, KpiCreateDto, KpiUpdateDto, KpiResponseDto>, IKpiService
{
    public KpiService(IUnitOfWork unitOfWork, IMapper mapper) : base(unitOfWork, mapper) { }

    protected override async Task ValidateAsync(object dto, int? currentId)
    {
        if (dto is not KpiCreateDto d) return;

        if (d.DepartmentId.HasValue && await UnitOfWork.Repository<Department>().GetByIdAsync(d.DepartmentId.Value) is null)
            throw new NotFoundException(nameof(Department), d.DepartmentId.Value);

        if (d.DesignationId.HasValue && await UnitOfWork.Repository<Designation>().GetByIdAsync(d.DesignationId.Value) is null)
            throw new NotFoundException(nameof(Designation), d.DesignationId.Value);

        var excludeId = currentId ?? 0;
        var deptId = d.DepartmentId;
        var desigId = d.DesignationId;
        if (await UnitOfWork.Repository<Kpi>().AnyAsync(x => x.KpiName == d.KpiName && x.DepartmentId == deptId && x.DesignationId == desigId && x.Id != excludeId))
            throw new BadRequestException($"KPI '{d.KpiName}' already exists for the same department/designation.");
    }
}
