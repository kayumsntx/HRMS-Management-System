using HRMS.Application.DTOs.Masters;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RequisitionTypesController : CrudControllerBase<RequisitionTypeCreateDto, RequisitionTypeUpdateDto, RequisitionTypeResponseDto>
{
    public RequisitionTypesController(IRequisitionTypeService service) : base(service, "Requisition type") { }
}

[ApiController]
[Route("api/[controller]")]
public class TrainingProgramsController : CrudControllerBase<TrainingProgramCreateDto, TrainingProgramUpdateDto, TrainingProgramResponseDto>
{
    public TrainingProgramsController(ITrainingProgramService service) : base(service, "Training program") { }
}

[ApiController]
[Route("api/[controller]")]
public class TrainersController : CrudControllerBase<TrainerCreateDto, TrainerUpdateDto, TrainerResponseDto>
{
    public TrainersController(ITrainerService service) : base(service, "Trainer") { }
}

[ApiController]
[Route("api/[controller]")]
public class PerformancePeriodsController : CrudControllerBase<PerformancePeriodCreateDto, PerformancePeriodUpdateDto, PerformancePeriodResponseDto>
{
    public PerformancePeriodsController(IPerformancePeriodService service) : base(service, "Performance period") { }
}

[ApiController]
[Route("api/[controller]")]
public class KpisController : CrudControllerBase<KpiCreateDto, KpiUpdateDto, KpiResponseDto>
{
    public KpisController(IKpiService service) : base(service, "KPI") { }
}
