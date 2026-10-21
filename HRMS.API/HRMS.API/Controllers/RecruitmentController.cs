using HRMS.Application.Common.Models;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Recruitment;
using HRMS.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecruitmentController : ControllerBase
{
    private readonly IRecruitmentService _recruitment;
    private readonly IHiringService _hiring;

    public RecruitmentController(IRecruitmentService recruitment, IHiringService hiring)
    {
        _recruitment = recruitment;
        _hiring = hiring;
    }

    // ---------------- Manpower requisition ----------------

    [HttpGet("manpower")]
    public async Task<IActionResult> GetManpower([FromQuery] string? status)
        => Ok(ApiResponse<List<ManpowerResponseDto>>.SuccessResponse(await _recruitment.GetManpowerAsync(status)));

    [HttpGet("manpower/{id:int}")]
    public async Task<IActionResult> GetManpowerById(int id)
        => Ok(ApiResponse<ManpowerResponseDto>.SuccessResponse(await _recruitment.GetManpowerByIdAsync(id)));

    [HttpPost("manpower")]
    public async Task<IActionResult> CreateManpower([FromBody] ManpowerCreateDto dto)
        => Ok(ApiResponse<ManpowerResponseDto>.SuccessResponse(await _recruitment.CreateManpowerAsync(dto), "Manpower requisition created."));

    [HttpPost("manpower/{id:int}/approve")]
    public async Task<IActionResult> ApproveManpower(int id)
        => Ok(ApiResponse<ManpowerResponseDto>.SuccessResponse(await _recruitment.ApproveManpowerAsync(id), "Manpower requisition approved."));

    [HttpPost("manpower/{id:int}/reject")]
    public async Task<IActionResult> RejectManpower(int id, [FromBody] ReasonDto dto)
        => Ok(ApiResponse<ManpowerResponseDto>.SuccessResponse(await _recruitment.RejectManpowerAsync(id, dto), "Manpower requisition rejected."));

    [HttpPost("manpower/{id:int}/close")]
    public async Task<IActionResult> CloseManpower(int id)
        => Ok(ApiResponse<ManpowerResponseDto>.SuccessResponse(await _recruitment.CloseManpowerAsync(id), "Manpower requisition closed."));

    // ---------------- Vacancy ----------------

    [HttpGet("vacancies")]
    public async Task<IActionResult> GetVacancies([FromQuery] string? status)
        => Ok(ApiResponse<List<VacancyResponseDto>>.SuccessResponse(await _recruitment.GetVacanciesAsync(status)));

    [HttpGet("vacancies/{id:int}")]
    public async Task<IActionResult> GetVacancyById(int id)
        => Ok(ApiResponse<VacancyResponseDto>.SuccessResponse(await _recruitment.GetVacancyByIdAsync(id)));

    [HttpPost("vacancies")]
    public async Task<IActionResult> CreateVacancy([FromBody] VacancyCreateDto dto)
        => Ok(ApiResponse<VacancyResponseDto>.SuccessResponse(await _recruitment.CreateVacancyAsync(dto), "Vacancy created."));

    [HttpPut("vacancies/{id:int}")]
    public async Task<IActionResult> UpdateVacancy(int id, [FromBody] VacancyUpdateDto dto)
        => Ok(ApiResponse<VacancyResponseDto>.SuccessResponse(await _recruitment.UpdateVacancyAsync(id, dto), "Vacancy updated."));

    [HttpPost("vacancies/{id:int}/close")]
    public async Task<IActionResult> CloseVacancy(int id)
        => Ok(ApiResponse<VacancyResponseDto>.SuccessResponse(await _recruitment.CloseVacancyAsync(id), "Vacancy closed."));

    [HttpPost("vacancies/{id:int}/cancel")]
    public async Task<IActionResult> CancelVacancy(int id)
        => Ok(ApiResponse<VacancyResponseDto>.SuccessResponse(await _recruitment.CancelVacancyAsync(id), "Vacancy cancelled."));

    // ---------------- Applicants ----------------

    [HttpGet("applicants")]
    public async Task<IActionResult> GetApplicants([FromQuery] int? vacancyId, [FromQuery] string? status)
        => Ok(ApiResponse<List<ApplicantResponseDto>>.SuccessResponse(await _recruitment.GetApplicantsAsync(vacancyId, status)));

    [HttpGet("applicants/{id:int}")]
    public async Task<IActionResult> GetApplicantById(int id)
        => Ok(ApiResponse<ApplicantResponseDto>.SuccessResponse(await _recruitment.GetApplicantByIdAsync(id)));

    [HttpPost("applicants")]
    public async Task<IActionResult> CreateApplicant([FromBody] ApplicantCreateDto dto)
        => Ok(ApiResponse<ApplicantResponseDto>.SuccessResponse(await _recruitment.CreateApplicantAsync(dto), "Applicant added."));

    [HttpPut("applicants/{id:int}")]
    public async Task<IActionResult> UpdateApplicant(int id, [FromBody] ApplicantUpdateDto dto)
        => Ok(ApiResponse<ApplicantResponseDto>.SuccessResponse(await _recruitment.UpdateApplicantAsync(id, dto), "Applicant updated."));

    [HttpPut("applicants/{id:int}/status")]
    public async Task<IActionResult> ChangeApplicantStatus(int id, [FromBody] ApplicantStatusDto dto)
        => Ok(ApiResponse<ApplicantResponseDto>.SuccessResponse(await _recruitment.ChangeApplicantStatusAsync(id, dto), "Applicant status updated."));

    // ---------------- Interviews ----------------

    [HttpGet("interviews")]
    public async Task<IActionResult> GetInterviews([FromQuery] int? applicantId, [FromQuery] string? status)
        => Ok(ApiResponse<List<InterviewResponseDto>>.SuccessResponse(await _hiring.GetInterviewsAsync(applicantId, status)));

    [HttpGet("interviews/{id:int}")]
    public async Task<IActionResult> GetInterviewById(int id)
        => Ok(ApiResponse<InterviewResponseDto>.SuccessResponse(await _hiring.GetInterviewByIdAsync(id)));

    [HttpPost("interviews")]
    public async Task<IActionResult> ScheduleInterview([FromBody] InterviewScheduleDto dto)
        => Ok(ApiResponse<InterviewResponseDto>.SuccessResponse(await _hiring.ScheduleInterviewAsync(dto), "Interview scheduled."));

    [HttpPut("interviews/{id:int}")]
    public async Task<IActionResult> UpdateInterview(int id, [FromBody] InterviewUpdateDto dto)
        => Ok(ApiResponse<InterviewResponseDto>.SuccessResponse(await _hiring.UpdateInterviewAsync(id, dto), "Interview updated."));

    [HttpPost("interviews/{id:int}/cancel")]
    public async Task<IActionResult> CancelInterview(int id)
        => Ok(ApiResponse<InterviewResponseDto>.SuccessResponse(await _hiring.CancelInterviewAsync(id), "Interview cancelled."));

    [HttpPost("interviews/{id:int}/evaluate")]
    public async Task<IActionResult> Evaluate(int id, [FromBody] InterviewEvaluateDto dto)
        => Ok(ApiResponse<InterviewResponseDto>.SuccessResponse(await _hiring.EvaluateAsync(id, dto), "Evaluation saved."));

    // ---------------- Offers ----------------

    [HttpGet("offers")]
    public async Task<IActionResult> GetOffers([FromQuery] string? status, [FromQuery] int? applicantId)
        => Ok(ApiResponse<List<OfferResponseDto>>.SuccessResponse(await _hiring.GetOffersAsync(status, applicantId)));

    [HttpGet("offers/{id:int}")]
    public async Task<IActionResult> GetOfferById(int id)
        => Ok(ApiResponse<OfferResponseDto>.SuccessResponse(await _hiring.GetOfferByIdAsync(id)));

    [HttpPost("offers")]
    public async Task<IActionResult> CreateOffer([FromBody] OfferCreateDto dto)
        => Ok(ApiResponse<OfferResponseDto>.SuccessResponse(await _hiring.CreateOfferAsync(dto), "Job offer created."));

    [HttpPut("offers/{id:int}/status")]
    public async Task<IActionResult> ChangeOfferStatus(int id, [FromBody] OfferStatusDto dto)
        => Ok(ApiResponse<OfferResponseDto>.SuccessResponse(await _hiring.ChangeOfferStatusAsync(id, dto), "Offer status updated."));

    // Converts an Accepted offer into a real employee record.
    [HttpPost("offers/{id:int}/hire")]
    public async Task<IActionResult> Hire(int id, [FromBody] HireDto dto)
        => Ok(ApiResponse<OfferResponseDto>.SuccessResponse(await _hiring.HireAsync(id, dto), "Applicant hired and employee created."));
}
