using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Common;
using HRMS.Application.DTOs.Recruitment;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class RecruitmentService : IRecruitmentService
{
    private const string Prefix = "MR";
    private static readonly string MrPending = ManpowerStatus.Pending.ToString();
    private static readonly string MrApproved = ManpowerStatus.Approved.ToString();
    private static readonly string MrRejected = ManpowerStatus.Rejected.ToString();
    private static readonly string MrClosed = ManpowerStatus.Closed.ToString();
    private static readonly string VOpen = VacancyStatus.Open.ToString();
    private static readonly string VClosed = VacancyStatus.Closed.ToString();
    private static readonly string VCancelled = VacancyStatus.Cancelled.ToString();
    private static readonly string AApplied = ApplicantStatus.Applied.ToString();
    private static readonly string AShortlisted = ApplicantStatus.Shortlisted.ToString();
    private static readonly string AInterviewed = ApplicantStatus.Interviewed.ToString();
    private static readonly string ASelected = ApplicantStatus.Selected.ToString();
    private static readonly string ARejected = ApplicantStatus.Rejected.ToString();
    private static readonly string AHired = ApplicantStatus.Hired.ToString();

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public RecruitmentService(IUnitOfWork uow, IMapper mapper, ICurrentUserService currentUser)
    {
        _uow = uow;
        _mapper = mapper;
        _currentUser = currentUser;
    }

    // =====================================================================
    // MANPOWER REQUISITION
    // =====================================================================
    public async Task<List<ManpowerResponseDto>> GetManpowerAsync(string? status)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<ManpowerStatus>(status, "status");
        var items = await _uow.Repository<ManpowerRequisition>().FindAsync(m => statusText == null || m.Status == statusText);
        return await MapManpowerAsync(items.OrderByDescending(m => m.Id));
    }

    public async Task<ManpowerResponseDto> GetManpowerByIdAsync(int id)
        => (await MapManpowerAsync(new[] { await LoadManpowerAsync(id) })).First();

    public async Task<ManpowerResponseDto> CreateManpowerAsync(ManpowerCreateDto dto)
    {
        await EnsureActiveAsync<Department>(dto.DepartmentId);
        await EnsureActiveAsync<Designation>(dto.DesignationId);

        var numbers = (await _uow.Repository<ManpowerRequisition>().FindAsync(m => m.RequisitionNo.StartsWith(Prefix))).Select(m => m.RequisitionNo);

        var item = new ManpowerRequisition
        {
            RequisitionNo = NumberHelper.Next(Prefix, numbers, 6),
            DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId,
            RequiredCount = dto.RequiredCount,
            Justification = dto.Justification?.Trim(),
            RequiredDate = dto.RequiredDate?.Date,
            Status = MrPending,
            RequestedBy = _currentUser.UserId
        };

        await _uow.Repository<ManpowerRequisition>().AddAsync(item);
        await _uow.SaveChangesAsync();
        return await GetManpowerByIdAsync(item.Id);
    }

    public async Task<ManpowerResponseDto> ApproveManpowerAsync(int id)
    {
        var item = await LoadManpowerAsync(id);
        if (item.Status != MrPending)
            throw new BadRequestException($"Only a Pending requisition can be approved. Current status: {item.Status}.");

        item.Status = MrApproved;
        item.ApprovedBy = _currentUser.UserId;
        item.ApprovedDate = DateTime.UtcNow;
        item.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<ManpowerRequisition>().Update(item);

        await _uow.SaveChangesAsync();
        return await GetManpowerByIdAsync(id);
    }

    public async Task<ManpowerResponseDto> RejectManpowerAsync(int id, ReasonDto dto)
    {
        var item = await LoadManpowerAsync(id);
        if (item.Status != MrPending)
            throw new BadRequestException($"Only a Pending requisition can be rejected. Current status: {item.Status}.");

        item.Status = MrRejected;
        item.RejectionReason = dto.Reason.Trim();
        item.ApprovedBy = _currentUser.UserId;
        item.ApprovedDate = DateTime.UtcNow;
        item.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<ManpowerRequisition>().Update(item);

        await _uow.SaveChangesAsync();
        return await GetManpowerByIdAsync(id);
    }

    public async Task<ManpowerResponseDto> CloseManpowerAsync(int id)
    {
        var item = await LoadManpowerAsync(id);
        if (item.Status != MrApproved)
            throw new BadRequestException($"Only an Approved requisition can be closed. Current status: {item.Status}.");

        item.Status = MrClosed;
        item.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<ManpowerRequisition>().Update(item);

        await _uow.SaveChangesAsync();
        return await GetManpowerByIdAsync(id);
    }

    // =====================================================================
    // VACANCY
    // =====================================================================
    public async Task<List<VacancyResponseDto>> GetVacanciesAsync(string? status)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<VacancyStatus>(status, "status");
        var items = await _uow.Repository<JobVacancy>().FindAsync(v => statusText == null || v.Status == statusText);
        return await MapVacanciesAsync(items.OrderByDescending(v => v.Id));
    }

    public async Task<VacancyResponseDto> GetVacancyByIdAsync(int id)
        => (await MapVacanciesAsync(new[] { await LoadVacancyAsync(id) })).First();

    public async Task<VacancyResponseDto> CreateVacancyAsync(VacancyCreateDto dto)
    {
        await EnsureActiveAsync<Department>(dto.DepartmentId);
        await EnsureActiveAsync<Designation>(dto.DesignationId);

        if (dto.ManpowerRequisitionId.HasValue)
        {
            var requisition = await LoadManpowerAsync(dto.ManpowerRequisitionId.Value);
            if (requisition.Status != MrApproved)
                throw new BadRequestException("The linked Manpower Requisition must be Approved.");

            if (requisition.DepartmentId != dto.DepartmentId || requisition.DesignationId != dto.DesignationId)
                throw new BadRequestException("Department and Designation must match the Manpower Requisition.");

            await EnsureWithinRequisitionAsync(requisition, dto.NumberOfVacancy, null);
        }

        var posted = (dto.PostedDate ?? DateTime.Now).Date;

        var vacancy = new JobVacancy
        {
            ManpowerRequisitionId = dto.ManpowerRequisitionId,
            Title = dto.Title.Trim(),
            DepartmentId = dto.DepartmentId,
            DesignationId = dto.DesignationId,
            NumberOfVacancy = dto.NumberOfVacancy,
            JobDescription = dto.JobDescription?.Trim(),
            PostedDate = posted,
            Deadline = dto.Deadline?.Date,
            Status = VOpen
        };

        await _uow.Repository<JobVacancy>().AddAsync(vacancy);
        await _uow.SaveChangesAsync();
        return await GetVacancyByIdAsync(vacancy.Id);
    }

    public async Task<VacancyResponseDto> UpdateVacancyAsync(int id, VacancyUpdateDto dto)
    {
        var vacancy = await LoadVacancyAsync(id);
        if (vacancy.Status != VOpen)
            throw new BadRequestException($"Only an Open vacancy can be edited. Current status: {vacancy.Status}.");

        var hired = (await _uow.Repository<Applicant>().FindAsync(a => a.JobVacancyId == id && a.Status == AHired)).Count;
        if (dto.NumberOfVacancy < hired)
            throw new BadRequestException($"Number of vacancies cannot be less than the already hired count ({hired}).");

        if (vacancy.ManpowerRequisitionId.HasValue)
            await EnsureWithinRequisitionAsync(await LoadManpowerAsync(vacancy.ManpowerRequisitionId.Value), dto.NumberOfVacancy, id);

        if (dto.Deadline.HasValue && dto.Deadline.Value.Date < vacancy.PostedDate.Date)
            throw new BadRequestException("Deadline cannot be before the Posted Date.");

        vacancy.Title = dto.Title.Trim();
        vacancy.NumberOfVacancy = dto.NumberOfVacancy;
        vacancy.JobDescription = dto.JobDescription?.Trim();
        vacancy.Deadline = dto.Deadline?.Date;
        vacancy.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<JobVacancy>().Update(vacancy);

        await _uow.SaveChangesAsync();
        return await GetVacancyByIdAsync(id);
    }

    public Task<VacancyResponseDto> CloseVacancyAsync(int id) => SetVacancyStatusAsync(id, VClosed);

    public Task<VacancyResponseDto> CancelVacancyAsync(int id) => SetVacancyStatusAsync(id, VCancelled);

    private async Task<VacancyResponseDto> SetVacancyStatusAsync(int id, string target)
    {
        var vacancy = await LoadVacancyAsync(id);
        if (vacancy.Status != VOpen)
            throw new BadRequestException($"Only an Open vacancy can be changed. Current status: {vacancy.Status}.");

        vacancy.Status = target;
        vacancy.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<JobVacancy>().Update(vacancy);

        await _uow.SaveChangesAsync();
        return await GetVacancyByIdAsync(id);
    }

    // =====================================================================
    // APPLICANT
    // =====================================================================
    public async Task<List<ApplicantResponseDto>> GetApplicantsAsync(int? vacancyId, string? status)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<ApplicantStatus>(status, "status");
        var items = await _uow.Repository<Applicant>().FindAsync(a =>
            (!vacancyId.HasValue || a.JobVacancyId == vacancyId.Value) && (statusText == null || a.Status == statusText));

        return await MapApplicantsAsync(items.OrderByDescending(a => a.Id).Take(500));
    }

    public async Task<ApplicantResponseDto> GetApplicantByIdAsync(int id)
        => (await MapApplicantsAsync(new[] { await LoadApplicantAsync(id) })).First();

    public async Task<ApplicantResponseDto> CreateApplicantAsync(ApplicantCreateDto dto)
    {
        var vacancy = await LoadVacancyAsync(dto.JobVacancyId);
        if (vacancy.Status != VOpen)
            throw new BadRequestException("This vacancy is not open for applications.");

        if (vacancy.Deadline.HasValue && vacancy.Deadline.Value.Date < DateTime.Now.Date)
            throw new BadRequestException("The application deadline of this vacancy has passed.");

        await EnsureNoDuplicateAsync(dto.JobVacancyId, dto.Email, dto.MobileNumber, null);

        var applicant = _mapper.Map<Applicant>(dto);
        applicant.FullName = dto.FullName.Trim();
        applicant.Status = AApplied;
        applicant.AppliedDate = DateTime.UtcNow;

        await _uow.Repository<Applicant>().AddAsync(applicant);
        await _uow.SaveChangesAsync();
        return await GetApplicantByIdAsync(applicant.Id);
    }

    public async Task<ApplicantResponseDto> UpdateApplicantAsync(int id, ApplicantUpdateDto dto)
    {
        var applicant = await LoadApplicantAsync(id);
        if (applicant.Status == AHired)
            throw new BadRequestException("A hired applicant cannot be edited.");

        await EnsureNoDuplicateAsync(applicant.JobVacancyId, dto.Email, dto.MobileNumber, id);

        _mapper.Map(dto, applicant);
        applicant.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Applicant>().Update(applicant);

        await _uow.SaveChangesAsync();
        return await GetApplicantByIdAsync(id);
    }

    public async Task<ApplicantResponseDto> ChangeApplicantStatusAsync(int id, ApplicantStatusDto dto)
    {
        var applicant = await LoadApplicantAsync(id);
        var target = EnumHelper.Normalize<ApplicantStatus>(dto.Status, "status");

        if (applicant.Status == AHired)
            throw new BadRequestException("A hired applicant's status cannot be changed.");

        if (target == AShortlisted)
        {
            if (applicant.Status != AApplied)
                throw new BadRequestException("Only an Applied applicant can be shortlisted.");
        }
        else if (target == ASelected)
        {
            if (applicant.Status != AInterviewed)
                throw new BadRequestException("Only an Interviewed applicant can be selected. Evaluate an interview first.");
        }
        else if (target == ARejected)
        {
            var activeOffer = await _uow.Repository<JobOffer>().AnyAsync(o => o.ApplicantId == id && (o.Status == "Sent" || o.Status == "Accepted"));
            if (activeOffer)
                throw new BadRequestException("This applicant has an active job offer. Reject or expire the offer first.");

            foreach (var interview in await _uow.Repository<Interview>().FindAsync(i => i.ApplicantId == id && i.Status == "Scheduled"))
            {
                interview.Status = InterviewStatus.Cancelled.ToString();
                interview.UpdatedDate = DateTime.UtcNow;
                _uow.Repository<Interview>().Update(interview);
            }
        }
        else
        {
            throw new BadRequestException("Allowed manual statuses: Shortlisted, Selected, Rejected.");
        }

        applicant.Status = target;
        applicant.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<Applicant>().Update(applicant);

        await _uow.SaveChangesAsync();
        return await GetApplicantByIdAsync(id);
    }

    // ---------------- Private Helpers ----------------

    private async Task EnsureActiveAsync<T>(int id) where T : BaseEntity
    {
        var entity = await _uow.Repository<T>().GetByIdAsync(id) ?? throw new NotFoundException(typeof(T).Name, id);
        if (!entity.IsActive)
            throw new BadRequestException($"{typeof(T).Name} is inactive.");
    }

    private async Task<ManpowerRequisition> LoadManpowerAsync(int id)
        => await _uow.Repository<ManpowerRequisition>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(ManpowerRequisition), id);

    private async Task<JobVacancy> LoadVacancyAsync(int id)
        => await _uow.Repository<JobVacancy>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(JobVacancy), id);

    private async Task<Applicant> LoadApplicantAsync(int id)
        => await _uow.Repository<Applicant>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(Applicant), id);

    private async Task EnsureWithinRequisitionAsync(ManpowerRequisition requisition, int newCount, int? excludeVacancyId)
    {
        var requisitionId = requisition.Id;
        var existing = await _uow.Repository<JobVacancy>().FindAsync(v => v.ManpowerRequisitionId == requisitionId && v.Status != VCancelled);
        var used = existing.Where(v => v.Id != (excludeVacancyId ?? 0)).Sum(v => v.NumberOfVacancy);

        if (used + newCount > requisition.RequiredCount)
            throw new BadRequestException($"Total vacancies ({used + newCount}) would exceed the approved requirement ({requisition.RequiredCount}).");
    }

    private async Task EnsureNoDuplicateAsync(int vacancyId, string? email, string? mobile, int? currentId)
    {
        var excludeId = currentId ?? 0;

        if (!string.IsNullOrWhiteSpace(email))
        {
            var e = email.Trim();
            if (await _uow.Repository<Applicant>().AnyAsync(a => a.JobVacancyId == vacancyId && a.Email == e && a.Id != excludeId))
                throw new BadRequestException("An applicant with this email already exists for the vacancy.");
        }

        if (!string.IsNullOrWhiteSpace(mobile))
        {
            var m = mobile.Trim();
            if (await _uow.Repository<Applicant>().AnyAsync(a => a.JobVacancyId == vacancyId && a.MobileNumber == m && a.Id != excludeId))
                throw new BadRequestException("An applicant with this mobile number already exists for the vacancy.");
        }
    }

    private async Task<List<ManpowerResponseDto>> MapManpowerAsync(IEnumerable<ManpowerRequisition> items)
    {
        var list = items.ToList();
        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);
        var designations = (await _uow.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);

        var ids = list.Select(m => m.Id).ToList();
        var vacancies = ids.Count == 0 ? new List<JobVacancy>()
            : await _uow.Repository<JobVacancy>().FindAsync(v => v.ManpowerRequisitionId != null && ids.Contains(v.ManpowerRequisitionId.Value) && v.Status != VCancelled);

        var result = _mapper.Map<List<ManpowerResponseDto>>(list);
        foreach (var dto in result)
        {
            dto.DepartmentName = departments.GetValueOrDefault(dto.DepartmentId);
            dto.DesignationName = designations.GetValueOrDefault(dto.DesignationId);
            dto.VacancyCount = vacancies.Where(v => v.ManpowerRequisitionId == dto.Id).Sum(v => v.NumberOfVacancy);
        }

        return result;
    }

    private async Task<List<VacancyResponseDto>> MapVacanciesAsync(IEnumerable<JobVacancy> items)
    {
        var list = items.ToList();
        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);
        var designations = (await _uow.Repository<Designation>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DesignationName);

        var ids = list.Select(v => v.Id).ToList();
        var applicants = ids.Count == 0 ? new List<Applicant>() : await _uow.Repository<Applicant>().FindAsync(a => ids.Contains(a.JobVacancyId));

        var result = _mapper.Map<List<VacancyResponseDto>>(list);
        foreach (var dto in result)
        {
            dto.DepartmentName = departments.GetValueOrDefault(dto.DepartmentId);
            dto.DesignationName = designations.GetValueOrDefault(dto.DesignationId);
            dto.ApplicantCount = applicants.Count(a => a.JobVacancyId == dto.Id);
            dto.HiredCount = applicants.Count(a => a.JobVacancyId == dto.Id && a.Status == AHired);
        }

        return result;
    }

    private async Task<List<ApplicantResponseDto>> MapApplicantsAsync(IEnumerable<Applicant> items)
    {
        var list = items.ToList();
        var vacancies = (await _uow.Repository<JobVacancy>().GetAllAsync()).ToDictionary(v => v.Id, v => v.Title);

        var result = _mapper.Map<List<ApplicantResponseDto>>(list);
        foreach (var dto in result)
            dto.VacancyTitle = vacancies.GetValueOrDefault(dto.JobVacancyId);

        return result;
    }
}
