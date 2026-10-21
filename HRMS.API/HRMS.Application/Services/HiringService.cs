using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.DTOs.Employees;
using HRMS.Application.DTOs.Recruitment;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class HiringService : IHiringService
{
    private static readonly string AShortlisted = ApplicantStatus.Shortlisted.ToString();
    private static readonly string AInterviewed = ApplicantStatus.Interviewed.ToString();
    private static readonly string ASelected = ApplicantStatus.Selected.ToString();
    private static readonly string AHired = ApplicantStatus.Hired.ToString();
    private static readonly string IScheduled = InterviewStatus.Scheduled.ToString();
    private static readonly string ICompleted = InterviewStatus.Completed.ToString();
    private static readonly string ICancelled = InterviewStatus.Cancelled.ToString();
    private static readonly string OSent = OfferStatus.Sent.ToString();
    private static readonly string OAccepted = OfferStatus.Accepted.ToString();

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly IEmployeeService _employeeService;

    public HiringService(IUnitOfWork uow, IMapper mapper, IEmployeeService employeeService)
    {
        _uow = uow;
        _mapper = mapper;
        _employeeService = employeeService;
    }

    // =====================================================================
    // INTERVIEW
    // =====================================================================
    public async Task<List<InterviewResponseDto>> GetInterviewsAsync(int? applicantId, string? status)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<InterviewStatus>(status, "status");
        var items = await _uow.Repository<Interview>().FindAsync(i =>
            (!applicantId.HasValue || i.ApplicantId == applicantId.Value) && (statusText == null || i.Status == statusText));

        return await MapInterviewsAsync(items.OrderByDescending(i => i.InterviewDate).Take(500));
    }

    public async Task<InterviewResponseDto> GetInterviewByIdAsync(int id)
        => (await MapInterviewsAsync(new[] { await LoadInterviewAsync(id) })).First();

    public async Task<InterviewResponseDto> ScheduleInterviewAsync(InterviewScheduleDto dto)
    {
        var applicant = await LoadApplicantAsync(dto.ApplicantId);
        if (applicant.Status != AShortlisted && applicant.Status != AInterviewed)
            throw new BadRequestException("Only a Shortlisted or Interviewed applicant can be scheduled for an interview.");

        var applicantId = applicant.Id;
        if (await _uow.Repository<Interview>().AnyAsync(i => i.ApplicantId == applicantId && i.Status == IScheduled))
            throw new BadRequestException("This applicant already has a scheduled interview.");

        var round = (await _uow.Repository<Interview>().FindAsync(i => i.ApplicantId == applicantId)).Select(i => i.InterviewRound).DefaultIfEmpty(0).Max() + 1;

        var interview = new Interview
        {
            ApplicantId = applicantId,
            InterviewRound = round,
            InterviewDate = dto.InterviewDate,
            Venue = dto.Venue?.Trim(),
            Status = IScheduled
        };

        await _uow.Repository<Interview>().AddAsync(interview);
        await _uow.SaveChangesAsync();
        return await GetInterviewByIdAsync(interview.Id);
    }

    public async Task<InterviewResponseDto> UpdateInterviewAsync(int id, InterviewUpdateDto dto)
    {
        var interview = await LoadInterviewAsync(id);
        if (interview.Status != IScheduled)
            throw new BadRequestException($"Only a Scheduled interview can be edited. Current status: {interview.Status}.");

        interview.InterviewDate = dto.InterviewDate;
        interview.Venue = dto.Venue?.Trim();
        interview.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Interview>().Update(interview);

        await _uow.SaveChangesAsync();
        return await GetInterviewByIdAsync(id);
    }

    public async Task<InterviewResponseDto> CancelInterviewAsync(int id)
    {
        var interview = await LoadInterviewAsync(id);
        if (interview.Status != IScheduled)
            throw new BadRequestException($"Only a Scheduled interview can be cancelled. Current status: {interview.Status}.");

        interview.Status = ICancelled;
        interview.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Interview>().Update(interview);

        await _uow.SaveChangesAsync();
        return await GetInterviewByIdAsync(id);
    }

    public async Task<InterviewResponseDto> EvaluateAsync(int interviewId, InterviewEvaluateDto dto)
    {
        var interview = await LoadInterviewAsync(interviewId);
        if (interview.Status == ICancelled)
            throw new BadRequestException("A cancelled interview cannot be evaluated.");

        var evaluator = await _uow.Repository<Employee>().GetByIdAsync(dto.EvaluatorEmployeeId)
            ?? throw new NotFoundException(nameof(Employee), dto.EvaluatorEmployeeId);

        if (!evaluator.IsActive)
            throw new BadRequestException("The evaluator is inactive.");

        var evaluatorId = evaluator.Id;
        if (await _uow.Repository<InterviewEvaluation>().AnyAsync(e => e.InterviewId == interviewId && e.EvaluatorEmployeeId == evaluatorId))
            throw new BadRequestException("This evaluator has already evaluated this interview.");

        await _uow.Repository<InterviewEvaluation>().AddAsync(new InterviewEvaluation
        {
            InterviewId = interviewId,
            EvaluatorEmployeeId = evaluatorId,
            Score = dto.Score,
            Comments = dto.Comments?.Trim(),
            Result = EnumHelper.Normalize<InterviewResult>(dto.Result, "result"),
            EvaluatedDate = DateTime.UtcNow
        });

        interview.Status = ICompleted;
        interview.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Interview>().Update(interview);

        var applicant = await LoadApplicantAsync(interview.ApplicantId);
        if (applicant.Status == AShortlisted)
        {
            applicant.Status = AInterviewed;
            applicant.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<Applicant>().Update(applicant);
        }

        await _uow.SaveChangesAsync();
        return await GetInterviewByIdAsync(interviewId);
    }

    // =====================================================================
    // OFFER / HIRE
    // =====================================================================
    public async Task<List<OfferResponseDto>> GetOffersAsync(string? status, int? applicantId)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<OfferStatus>(status, "status");
        var items = await _uow.Repository<JobOffer>().FindAsync(o =>
            (statusText == null || o.Status == statusText) && (!applicantId.HasValue || o.ApplicantId == applicantId.Value));

        return await MapOffersAsync(items.OrderByDescending(o => o.Id));
    }

    public async Task<OfferResponseDto> GetOfferByIdAsync(int id)
        => (await MapOffersAsync(new[] { await LoadOfferAsync(id) })).First();

    public async Task<OfferResponseDto> CreateOfferAsync(OfferCreateDto dto)
    {
        var applicant = await LoadApplicantAsync(dto.ApplicantId);
        if (applicant.Status != ASelected)
            throw new BadRequestException("A job offer can be created only for a Selected applicant.");

        var applicantId = applicant.Id;
        if (await _uow.Repository<JobOffer>().AnyAsync(o => o.ApplicantId == applicantId && (o.Status == OSent || o.Status == OAccepted)))
            throw new BadRequestException("This applicant already has an active job offer.");

        var vacancy = await _uow.Repository<JobVacancy>().GetByIdAsync(applicant.JobVacancyId)
            ?? throw new NotFoundException(nameof(JobVacancy), applicant.JobVacancyId);

        var designationId = dto.DesignationId ?? vacancy.DesignationId;
        var designation = await _uow.Repository<Designation>().GetByIdAsync(designationId)
            ?? throw new NotFoundException(nameof(Designation), designationId);

        if (!designation.IsActive)
            throw new BadRequestException("Designation is inactive.");

        var offer = new JobOffer
        {
            ApplicantId = applicantId,
            DesignationId = designationId,
            OfferedSalary = dto.OfferedSalary,
            OfferDate = (dto.OfferDate ?? DateTime.Now).Date,
            JoiningDate = dto.JoiningDate?.Date,
            Status = OSent
        };

        await _uow.Repository<JobOffer>().AddAsync(offer);
        await _uow.SaveChangesAsync();
        return await GetOfferByIdAsync(offer.Id);
    }

    public async Task<OfferResponseDto> ChangeOfferStatusAsync(int id, OfferStatusDto dto)
    {
        var offer = await LoadOfferAsync(id);
        var target = EnumHelper.Normalize<OfferStatus>(dto.Status, "status");

        if (target == OSent)
            throw new BadRequestException("Allowed statuses: Accepted, Rejected, Expired.");

        if (offer.Status != OSent)
            throw new BadRequestException($"Only a Sent offer can be changed. Current status: {offer.Status}.");

        offer.Status = target;
        offer.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<JobOffer>().Update(offer);

        await _uow.SaveChangesAsync();
        return await GetOfferByIdAsync(id);
    }

    // Creates the employee from the accepted offer (normal employee creation rules apply),
    // then marks the applicant as Hired and links the employee to the offer.
    public async Task<OfferResponseDto> HireAsync(int id, HireDto dto)
    {
        var offer = await LoadOfferAsync(id);
        if (offer.Status != OAccepted)
            throw new BadRequestException($"Only an Accepted offer can be converted to an employee. Current status: {offer.Status}.");

        if (offer.EmployeeId.HasValue)
            throw new BadRequestException("This offer has already been converted to an employee.");

        var applicant = await LoadApplicantAsync(offer.ApplicantId);
        if (applicant.Status != ASelected)
            throw new BadRequestException($"The applicant must be in Selected status. Current status: {applicant.Status}.");

        var vacancy = await _uow.Repository<JobVacancy>().GetByIdAsync(applicant.JobVacancyId)
            ?? throw new NotFoundException(nameof(JobVacancy), applicant.JobVacancyId);

        if (vacancy.Status == VacancyStatus.Cancelled.ToString())
            throw new BadRequestException("The vacancy of this applicant has been cancelled.");

        var vacancyId = vacancy.Id;
        var hiredCount = (await _uow.Repository<Applicant>().FindAsync(a => a.JobVacancyId == vacancyId && a.Status == AHired)).Count;
        if (hiredCount >= vacancy.NumberOfVacancy)
            throw new BadRequestException("All positions of this vacancy are already filled.");

        if (dto.UnitId.HasValue)
        {
            var unit = await _uow.Repository<Unit>().GetByIdAsync(dto.UnitId.Value) ?? throw new NotFoundException(nameof(Unit), dto.UnitId.Value);
            if (unit.CompanyId != dto.CompanyId)
                throw new BadRequestException("The Unit does not belong to the selected company.");
        }

        if (dto.SectionId.HasValue)
        {
            var section = await _uow.Repository<Section>().GetByIdAsync(dto.SectionId.Value) ?? throw new NotFoundException(nameof(Section), dto.SectionId.Value);
            if (section.DepartmentId != vacancy.DepartmentId)
                throw new BadRequestException("The Section does not belong to the vacancy's department.");
        }

        var joining = (dto.JoiningDate ?? offer.JoiningDate ?? DateTime.Now).Date;

        // Saved by EmployeeService itself (the unit of work is shared inside one request).
        var employee = await _employeeService.CreateAsync(new EmployeeCreateDto
        {
            FullName = applicant.FullName,
            Email = applicant.Email,
            MobileNumber = applicant.MobileNumber,
            DateOfJoining = joining,
            EmployeeStatusId = dto.EmployeeStatusId,
            DesignationId = offer.DesignationId,
            GradeId = dto.GradeId,
            CompanyId = dto.CompanyId,
            UnitId = dto.UnitId,
            DepartmentId = vacancy.DepartmentId,
            SectionId = dto.SectionId,
            ReportingManagerId = dto.ReportingManagerId
        });

        offer.EmployeeId = employee.Id;
        offer.JoiningDate = joining;
        offer.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<JobOffer>().Update(offer);

        applicant.Status = AHired;
        applicant.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Applicant>().Update(applicant);

        if (hiredCount + 1 >= vacancy.NumberOfVacancy && vacancy.Status == VacancyStatus.Open.ToString())
        {
            vacancy.Status = VacancyStatus.Closed.ToString();
            vacancy.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<JobVacancy>().Update(vacancy);
        }

        await _uow.SaveChangesAsync();
        return await GetOfferByIdAsync(id);
    }

    // ---------------- Private Helpers ----------------

    private async Task<Interview> LoadInterviewAsync(int id)
        => await _uow.Repository<Interview>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(Interview), id);

    private async Task<Applicant> LoadApplicantAsync(int id)
        => await _uow.Repository<Applicant>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(Applicant), id);

    private async Task<JobOffer> LoadOfferAsync(int id)
        => await _uow.Repository<JobOffer>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(JobOffer), id);

    private async Task<List<InterviewResponseDto>> MapInterviewsAsync(IEnumerable<Interview> items)
    {
        var list = items.ToList();

        var applicantIds = list.Select(i => i.ApplicantId).Distinct().ToList();
        var applicants = applicantIds.Count == 0
            ? new Dictionary<int, Applicant>()
            : (await _uow.Repository<Applicant>().FindAsync(a => applicantIds.Contains(a.Id))).ToDictionary(a => a.Id);

        var vacancies = (await _uow.Repository<JobVacancy>().GetAllAsync()).ToDictionary(v => v.Id, v => v.Title);

        var interviewIds = list.Select(i => i.Id).ToList();
        var evaluations = interviewIds.Count == 0
            ? new List<InterviewEvaluation>()
            : await _uow.Repository<InterviewEvaluation>().FindAsync(e => interviewIds.Contains(e.InterviewId));

        var evaluatorIds = evaluations.Where(e => e.EvaluatorEmployeeId.HasValue).Select(e => e.EvaluatorEmployeeId!.Value).Distinct().ToList();
        var evaluators = evaluatorIds.Count == 0
            ? new Dictionary<int, string>()
            : (await _uow.Repository<Employee>().FindAsync(e => evaluatorIds.Contains(e.Id))).ToDictionary(e => e.Id, e => e.FullName);

        var result = _mapper.Map<List<InterviewResponseDto>>(list);
        foreach (var dto in result)
        {
            if (applicants.TryGetValue(dto.ApplicantId, out var applicant))
            {
                dto.ApplicantName = applicant.FullName;
                dto.VacancyTitle = vacancies.GetValueOrDefault(applicant.JobVacancyId);
            }

            var evals = _mapper.Map<List<InterviewEvaluationResponseDto>>(
                evaluations.Where(e => e.InterviewId == dto.Id).OrderBy(e => e.Id).ToList());

            foreach (var ev in evals.Where(e => e.EvaluatorEmployeeId.HasValue))
                ev.EvaluatorName = evaluators.GetValueOrDefault(ev.EvaluatorEmployeeId!.Value);

            dto.Evaluations = evals;
        }

        return result;
    }

    private async Task<List<OfferResponseDto>> MapOffersAsync(IEnumerable<JobOffer> items)
    {
        var list = items.ToList();

        var applicants = (await _uow.Repository<Applicant>().GetAllAsync()).ToDictionary(a => a.Id, a => a.FullName);
        var designations = (await _uow.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);

        var employeeIds = list.Where(o => o.EmployeeId.HasValue).Select(o => o.EmployeeId!.Value).Distinct().ToList();
        var employees = employeeIds.Count == 0
            ? new Dictionary<int, string>()
            : (await _uow.Repository<Employee>().FindAsync(e => employeeIds.Contains(e.Id))).ToDictionary(e => e.Id, e => e.EmployeeCode);

        var result = _mapper.Map<List<OfferResponseDto>>(list);
        foreach (var dto in result)
        {
            dto.ApplicantName = applicants.GetValueOrDefault(dto.ApplicantId);
            dto.DesignationName = designations.GetValueOrDefault(dto.DesignationId);
            if (dto.EmployeeId.HasValue)
                dto.EmployeeCode = employees.GetValueOrDefault(dto.EmployeeId.Value);
        }

        return result;
    }
}
