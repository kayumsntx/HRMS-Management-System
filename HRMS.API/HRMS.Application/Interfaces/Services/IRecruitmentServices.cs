using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Recruitment;

namespace HRMS.Application.Interfaces.Services;

public interface IRecruitmentService
{
    Task<List<ManpowerResponseDto>> GetManpowerAsync(string? status);
    Task<ManpowerResponseDto> GetManpowerByIdAsync(int id);
    Task<ManpowerResponseDto> CreateManpowerAsync(ManpowerCreateDto dto);
    Task<ManpowerResponseDto> ApproveManpowerAsync(int id);
    Task<ManpowerResponseDto> RejectManpowerAsync(int id, ReasonDto dto);
    Task<ManpowerResponseDto> CloseManpowerAsync(int id);

    Task<List<VacancyResponseDto>> GetVacanciesAsync(string? status);
    Task<VacancyResponseDto> GetVacancyByIdAsync(int id);
    Task<VacancyResponseDto> CreateVacancyAsync(VacancyCreateDto dto);
    Task<VacancyResponseDto> UpdateVacancyAsync(int id, VacancyUpdateDto dto);
    Task<VacancyResponseDto> CloseVacancyAsync(int id);
    Task<VacancyResponseDto> CancelVacancyAsync(int id);

    Task<List<ApplicantResponseDto>> GetApplicantsAsync(int? vacancyId, string? status);
    Task<ApplicantResponseDto> GetApplicantByIdAsync(int id);
    Task<ApplicantResponseDto> CreateApplicantAsync(ApplicantCreateDto dto);
    Task<ApplicantResponseDto> UpdateApplicantAsync(int id, ApplicantUpdateDto dto);
    Task<ApplicantResponseDto> ChangeApplicantStatusAsync(int id, ApplicantStatusDto dto);
}

public interface IHiringService
{
    Task<List<InterviewResponseDto>> GetInterviewsAsync(int? applicantId, string? status);
    Task<InterviewResponseDto> GetInterviewByIdAsync(int id);
    Task<InterviewResponseDto> ScheduleInterviewAsync(InterviewScheduleDto dto);
    Task<InterviewResponseDto> UpdateInterviewAsync(int id, InterviewUpdateDto dto);
    Task<InterviewResponseDto> CancelInterviewAsync(int id);
    Task<InterviewResponseDto> EvaluateAsync(int interviewId, InterviewEvaluateDto dto);

    Task<List<OfferResponseDto>> GetOffersAsync(string? status, int? applicantId);
    Task<OfferResponseDto> GetOfferByIdAsync(int id);
    Task<OfferResponseDto> CreateOfferAsync(OfferCreateDto dto);
    Task<OfferResponseDto> ChangeOfferStatusAsync(int id, OfferStatusDto dto);
    Task<OfferResponseDto> HireAsync(int id, HireDto dto);
}
