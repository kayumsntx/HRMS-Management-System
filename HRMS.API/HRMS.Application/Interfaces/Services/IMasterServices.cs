using HRMS.Application.DTOs.Masters;

namespace HRMS.Application.Interfaces.Services;

public interface IRequisitionTypeService : ICrudService<RequisitionTypeCreateDto, RequisitionTypeUpdateDto, RequisitionTypeResponseDto> { }
public interface ITrainingProgramService : ICrudService<TrainingProgramCreateDto, TrainingProgramUpdateDto, TrainingProgramResponseDto> { }
public interface ITrainerService : ICrudService<TrainerCreateDto, TrainerUpdateDto, TrainerResponseDto> { }
public interface IPerformancePeriodService : ICrudService<PerformancePeriodCreateDto, PerformancePeriodUpdateDto, PerformancePeriodResponseDto> { }
public interface IKpiService : ICrudService<KpiCreateDto, KpiUpdateDto, KpiResponseDto> { }
