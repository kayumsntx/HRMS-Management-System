using AutoMapper;
using HRMS.Application.DTOs.Career;
using HRMS.Application.DTOs.Masters;
using HRMS.Application.DTOs.Performance;
using HRMS.Application.DTOs.Recruitment;
using HRMS.Application.DTOs.Requisitions;
using HRMS.Application.DTOs.Separations;
using HRMS.Application.DTOs.Training;
using HRMS.Domain.Entities;

namespace HRMS.Application.Mappings;

public class HrModulesMappingProfile : Profile
{
    public HrModulesMappingProfile()
    {
        // ---- Master data (generic CRUD) ----
        CreateMap<RequisitionType, RequisitionTypeResponseDto>();
        CreateMap<RequisitionTypeCreateDto, RequisitionType>();
        CreateMap<RequisitionTypeUpdateDto, RequisitionType>();

        CreateMap<TrainingProgram, TrainingProgramResponseDto>();
        CreateMap<TrainingProgramCreateDto, TrainingProgram>();
        CreateMap<TrainingProgramUpdateDto, TrainingProgram>();

        CreateMap<Trainer, TrainerResponseDto>();
        CreateMap<TrainerCreateDto, Trainer>();
        CreateMap<TrainerUpdateDto, Trainer>();

        CreateMap<PerformancePeriod, PerformancePeriodResponseDto>();
        CreateMap<PerformancePeriodCreateDto, PerformancePeriod>();
        CreateMap<PerformancePeriodUpdateDto, PerformancePeriod>();

        CreateMap<Kpi, KpiResponseDto>();
        CreateMap<KpiCreateDto, Kpi>();
        CreateMap<KpiUpdateDto, Kpi>();

        // ---- Requisition ----
        CreateMap<Requisition, RequisitionResponseDto>();
        CreateMap<RequisitionDetail, RequisitionItemResponseDto>();

        // ---- Career ----
        CreateMap<EmployeeCareerHistory, CareerChangeResponseDto>();

        // ---- Separation ----
        CreateMap<EmployeeSeparation, SeparationResponseDto>();
        CreateMap<ExitInterview, ExitInterviewResponseDto>();
        CreateMap<SeparationClearance, ClearanceResponseDto>();
        CreateMap<FinalSettlement, SettlementResponseDto>();

        // ---- Recruitment ----
        CreateMap<ManpowerRequisition, ManpowerResponseDto>();
        CreateMap<JobVacancy, VacancyResponseDto>();
        CreateMap<Applicant, ApplicantResponseDto>();
        CreateMap<ApplicantCreateDto, Applicant>();
        CreateMap<ApplicantUpdateDto, Applicant>();
        CreateMap<Interview, InterviewResponseDto>();
        CreateMap<InterviewEvaluation, InterviewEvaluationResponseDto>();
        CreateMap<JobOffer, OfferResponseDto>();

        // ---- Performance ----
        CreateMap<EmployeeGoal, GoalResponseDto>();
        CreateMap<PerformanceEvaluation, EvaluationResponseDto>();

        // ---- Training ----
        CreateMap<TrainingSchedule, TrainingScheduleResponseDto>();
        CreateMap<TrainingParticipant, TrainingParticipantResponseDto>();
    }
}
