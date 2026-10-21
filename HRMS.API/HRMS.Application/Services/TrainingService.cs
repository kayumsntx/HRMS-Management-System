using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.Common.Helpers;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.DTOs.Training;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class TrainingService : ITrainingService
{
    private static readonly string SPlanned = TrainingStatus.Planned.ToString();
    private static readonly string SRunning = TrainingStatus.Running.ToString();
    private static readonly string SCompleted = TrainingStatus.Completed.ToString();
    private static readonly string SCancelled = TrainingStatus.Cancelled.ToString();
    private static readonly string PEnrolled = ParticipantStatus.Enrolled.ToString();
    private static readonly string PCompleted = ParticipantStatus.Completed.ToString();
    private static readonly string PDropped = ParticipantStatus.Dropped.ToString();

    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly INotificationPublisher _notifier;

    public TrainingService(IUnitOfWork uow, IMapper mapper, INotificationPublisher notifier)
    {
        _uow = uow;
        _mapper = mapper;
        _notifier = notifier;
    }

    private static int DaysOf(TrainingSchedule s) => (s.EndDateTime.Date - s.StartDateTime.Date).Days + 1;

    // =====================================================================
    // SCHEDULE
    // =====================================================================
    public async Task<List<TrainingScheduleResponseDto>> GetSchedulesAsync(string? status, int? programId, DateTime? fromDate, DateTime? toDate)
    {
        var statusText = string.IsNullOrWhiteSpace(status) ? null : EnumHelper.Normalize<TrainingStatus>(status, "status");
        var from = fromDate?.Date;
        var to = toDate?.Date.AddDays(1);

        var items = await _uow.Repository<TrainingSchedule>().FindAsync(s =>
            (statusText == null || s.Status == statusText) &&
            (!programId.HasValue || s.TrainingProgramId == programId.Value) &&
            (!from.HasValue || s.StartDateTime >= from.Value) &&
            (!to.HasValue || s.StartDateTime < to.Value));

        return await MapSchedulesAsync(items.OrderByDescending(s => s.StartDateTime).Take(500));
    }

    public async Task<TrainingScheduleResponseDto> GetScheduleByIdAsync(int id)
        => (await MapSchedulesAsync(new[] { await LoadScheduleAsync(id) })).First();

    public async Task<TrainingScheduleResponseDto> CreateScheduleAsync(TrainingScheduleCreateDto dto)
    {
        var program = await _uow.Repository<TrainingProgram>().GetByIdAsync(dto.TrainingProgramId)
            ?? throw new NotFoundException(nameof(TrainingProgram), dto.TrainingProgramId);

        if (!program.IsActive)
            throw new BadRequestException("This Training Program is inactive.");

        await EnsureTrainerFreeAsync(dto.TrainerId, dto.StartDateTime, dto.EndDateTime, null);

        var schedule = new TrainingSchedule
        {
            TrainingProgramId = dto.TrainingProgramId,
            TrainerId = dto.TrainerId,
            StartDateTime = dto.StartDateTime,
            EndDateTime = dto.EndDateTime,
            Venue = dto.Venue?.Trim(),
            Cost = dto.Cost,
            Status = SPlanned
        };

        await _uow.Repository<TrainingSchedule>().AddAsync(schedule);
        await _uow.SaveChangesAsync();
        return await GetScheduleByIdAsync(schedule.Id);
    }

    public async Task<TrainingScheduleResponseDto> UpdateScheduleAsync(int id, TrainingScheduleUpdateDto dto)
    {
        var schedule = await LoadScheduleAsync(id);
        if (schedule.Status != SPlanned)
            throw new BadRequestException($"Only a Planned training can be edited. Current status: {schedule.Status}.");

        await EnsureTrainerFreeAsync(dto.TrainerId, dto.StartDateTime, dto.EndDateTime, id);

        schedule.TrainerId = dto.TrainerId;
        schedule.StartDateTime = dto.StartDateTime;
        schedule.EndDateTime = dto.EndDateTime;
        schedule.Venue = dto.Venue?.Trim();
        schedule.Cost = dto.Cost;
        schedule.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<TrainingSchedule>().Update(schedule);

        await _uow.SaveChangesAsync();
        return await GetScheduleByIdAsync(id);
    }

    public async Task<TrainingScheduleResponseDto> StartAsync(int id)
    {
        var schedule = await LoadScheduleAsync(id);
        if (schedule.Status != SPlanned)
            throw new BadRequestException($"Only a Planned training can be started. Current status: {schedule.Status}.");

        if (schedule.StartDateTime.Date > DateTime.Now.Date)
            throw new BadRequestException("This training is scheduled for a later date and cannot be started yet.");

        schedule.Status = SRunning;
        schedule.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<TrainingSchedule>().Update(schedule);

        await _uow.SaveChangesAsync();
        return await GetScheduleByIdAsync(id);
    }

    public async Task<TrainingScheduleResponseDto> CompleteAsync(int id)
    {
        var schedule = await LoadScheduleAsync(id);
        if (schedule.Status != SRunning)
            throw new BadRequestException($"Only a Running training can be completed. Current status: {schedule.Status}.");

        schedule.Status = SCompleted;
        schedule.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<TrainingSchedule>().Update(schedule);

        foreach (var participant in await _uow.Repository<TrainingParticipant>().FindAsync(p => p.TrainingScheduleId == id && p.Status == PEnrolled))
        {
            participant.Status = PCompleted;
            participant.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<TrainingParticipant>().Update(participant);
        }

        await _uow.SaveChangesAsync();
        return await GetScheduleByIdAsync(id);
    }

    public async Task<TrainingScheduleResponseDto> CancelAsync(int id)
    {
        var schedule = await LoadScheduleAsync(id);
        if (schedule.Status != SPlanned && schedule.Status != SRunning)
            throw new BadRequestException($"Only a Planned or Running training can be cancelled. Current status: {schedule.Status}.");

        schedule.Status = SCancelled;
        schedule.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<TrainingSchedule>().Update(schedule);

        var program = await _uow.Repository<TrainingProgram>().GetByIdAsync(schedule.TrainingProgramId);
        foreach (var participant in await _uow.Repository<TrainingParticipant>().FindAsync(p => p.TrainingScheduleId == id && p.Status == PEnrolled))
        {
            await _notifier.NotifyEmployeeAsync(participant.EmployeeId, NotificationCategory.General, "Training cancelled",
                $"The training '{program?.ProgramName}' scheduled from {schedule.StartDateTime:dd MMM yyyy} has been cancelled.", "/my/trainings");
        }

        await _uow.SaveChangesAsync();
        return await GetScheduleByIdAsync(id);
    }

    // =====================================================================
    // PARTICIPANTS
    // =====================================================================
    public async Task<List<TrainingParticipantResponseDto>> GetParticipantsAsync(int scheduleId)
    {
        await LoadScheduleAsync(scheduleId);
        var items = await _uow.Repository<TrainingParticipant>().FindAsync(p => p.TrainingScheduleId == scheduleId);
        return await MapParticipantsAsync(items);
    }

    public async Task<List<TrainingParticipantResponseDto>> EnrollAsync(int scheduleId, TrainingEnrollDto dto)
    {
        var schedule = await LoadScheduleAsync(scheduleId);
        if (schedule.Status != SPlanned && schedule.Status != SRunning)
            throw new BadRequestException($"Employees can be enrolled only in a Planned or Running training. Current status: {schedule.Status}.");

        var program = await _uow.Repository<TrainingProgram>().GetByIdAsync(schedule.TrainingProgramId);

        var ids = dto.EmployeeIds.Distinct().ToList();
        var employees = (await _uow.Repository<Employee>().FindAsync(e => ids.Contains(e.Id))).ToDictionary(e => e.Id);
        var existing = (await _uow.Repository<TrainingParticipant>().FindAsync(p => p.TrainingScheduleId == scheduleId && ids.Contains(p.EmployeeId)))
            .ToDictionary(p => p.EmployeeId);

        var toNotify = new List<int>();

        foreach (var employeeId in ids)
        {
            if (!employees.TryGetValue(employeeId, out var employee))
                throw new NotFoundException(nameof(Employee), employeeId);

            if (!employee.IsActive)
                throw new BadRequestException($"{employee.FullName} is inactive.");

            existing.TryGetValue(employeeId, out var current);
            if (current != null && current.Status != PDropped)
                throw new BadRequestException($"{employee.FullName} is already enrolled in this training.");

            // The same employee cannot attend two overlapping trainings.
            var otherScheduleIds = (await _uow.Repository<TrainingParticipant>()
                    .FindAsync(p => p.EmployeeId == employeeId && p.Status == PEnrolled && p.TrainingScheduleId != scheduleId))
                .Select(p => p.TrainingScheduleId).ToList();

            if (otherScheduleIds.Count > 0 && await _uow.Repository<TrainingSchedule>().AnyAsync(s =>
                    otherScheduleIds.Contains(s.Id) && s.Status != SCancelled &&
                    s.StartDateTime < schedule.EndDateTime && s.EndDateTime > schedule.StartDateTime))
                throw new BadRequestException($"{employee.FullName} is already enrolled in another training at the same time.");

            if (current != null)
            {
                current.Status = PEnrolled;
                current.UpdatedDate = DateTime.UtcNow;
                _uow.Repository<TrainingParticipant>().Update(current);
            }
            else
            {
                await _uow.Repository<TrainingParticipant>().AddAsync(new TrainingParticipant
                {
                    TrainingScheduleId = scheduleId,
                    EmployeeId = employeeId,
                    Status = PEnrolled
                });
            }

            toNotify.Add(employeeId);
        }

        foreach (var employeeId in toNotify)
        {
            await _notifier.NotifyEmployeeAsync(employeeId, NotificationCategory.General, "Training enrollment",
                $"You have been enrolled in '{program?.ProgramName}' from {schedule.StartDateTime:dd MMM yyyy} to {schedule.EndDateTime:dd MMM yyyy}.",
                "/my/trainings");
        }

        await _uow.SaveChangesAsync();
        return await GetParticipantsAsync(scheduleId);
    }

    public async Task<List<TrainingParticipantResponseDto>> DropParticipantAsync(int participantId)
    {
        var participant = await LoadParticipantAsync(participantId);
        var schedule = await LoadScheduleAsync(participant.TrainingScheduleId);

        if (schedule.Status == SCompleted || schedule.Status == SCancelled)
            throw new BadRequestException($"A participant cannot be dropped from a {schedule.Status} training.");

        if (participant.Status != PEnrolled)
            throw new BadRequestException($"Only an Enrolled participant can be dropped. Current status: {participant.Status}.");

        participant.Status = PDropped;
        participant.UpdatedDate = DateTime.UtcNow;
        _uow.Repository<TrainingParticipant>().Update(participant);

        await _uow.SaveChangesAsync();
        return await GetParticipantsAsync(schedule.Id);
    }

    // =====================================================================
    // ATTENDANCE
    // =====================================================================
    public async Task<List<TrainingAttendanceRowDto>> GetAttendanceAsync(int scheduleId, DateTime date)
    {
        var schedule = await LoadScheduleAsync(scheduleId);
        var day = date.Date;

        if (day < schedule.StartDateTime.Date || day > schedule.EndDateTime.Date)
            throw new BadRequestException("The date is outside the training period.");

        var participants = await _uow.Repository<TrainingParticipant>().FindAsync(p => p.TrainingScheduleId == scheduleId && p.Status != PDropped);
        var participantIds = participants.Select(p => p.Id).ToList();

        var marks = participantIds.Count == 0
            ? new Dictionary<int, TrainingAttendance>()
            : (await _uow.Repository<TrainingAttendance>().FindAsync(a => participantIds.Contains(a.TrainingParticipantId) && a.AttendanceDate == day))
                .ToDictionary(a => a.TrainingParticipantId);

        var employees = (await _uow.Repository<Employee>().GetAllAsync()).ToDictionary(e => e.Id);

        return participants
            .Where(p => employees.ContainsKey(p.EmployeeId))
            .Select(p => new TrainingAttendanceRowDto
            {
                ParticipantId = p.Id,
                EmployeeId = p.EmployeeId,
                EmployeeCode = employees[p.EmployeeId].EmployeeCode,
                EmployeeName = employees[p.EmployeeId].FullName,
                Date = day,
                IsPresent = marks.TryGetValue(p.Id, out var mark) ? mark.IsPresent : (bool?)null
            })
            .OrderBy(r => (r.EmployeeCode ?? "").Length).ThenBy(r => r.EmployeeCode)
            .ToList();
    }

    public async Task<List<TrainingAttendanceRowDto>> MarkAttendanceAsync(int scheduleId, TrainingAttendanceMarkDto dto)
    {
        var schedule = await LoadScheduleAsync(scheduleId);
        if (schedule.Status != SRunning)
            throw new BadRequestException("Attendance can be marked only while the training is Running.");

        var day = dto.Date.Date;
        if (day < schedule.StartDateTime.Date || day > schedule.EndDateTime.Date)
            throw new BadRequestException("The date is outside the training period.");

        if (day > DateTime.Now.Date)
            throw new BadRequestException("Attendance cannot be marked for a future date.");

        var participants = (await _uow.Repository<TrainingParticipant>().FindAsync(p => p.TrainingScheduleId == scheduleId && p.Status == PEnrolled))
            .ToDictionary(p => p.EmployeeId);

        var participantIds = participants.Values.Select(p => p.Id).ToList();
        var marks = participantIds.Count == 0
            ? new Dictionary<int, TrainingAttendance>()
            : (await _uow.Repository<TrainingAttendance>().FindAsync(a => participantIds.Contains(a.TrainingParticipantId) && a.AttendanceDate == day))
                .ToDictionary(a => a.TrainingParticipantId);

        foreach (var item in dto.Items)
        {
            if (!participants.TryGetValue(item.EmployeeId, out var participant))
                throw new BadRequestException($"Employee {item.EmployeeId} is not an enrolled participant of this training.");

            if (marks.TryGetValue(participant.Id, out var mark))
            {
                mark.IsPresent = item.IsPresent;
                mark.UpdatedDate = DateTime.UtcNow;
                _uow.Repository<TrainingAttendance>().Update(mark);
            }
            else
            {
                await _uow.Repository<TrainingAttendance>().AddAsync(new TrainingAttendance
                {
                    TrainingParticipantId = participant.Id,
                    AttendanceDate = day,
                    IsPresent = item.IsPresent
                });
            }
        }

        await _uow.SaveChangesAsync();
        return await GetAttendanceAsync(scheduleId, day);
    }

    // =====================================================================
    // EVALUATION (only for participants who completed a Completed training)
    // =====================================================================
    public async Task<TrainingParticipantResponseDto> SaveEvaluationAsync(int participantId, TrainingEvaluationSaveDto dto)
    {
        var participant = await LoadParticipantAsync(participantId);
        var schedule = await LoadScheduleAsync(participant.TrainingScheduleId);

        if (schedule.Status != SCompleted)
            throw new BadRequestException("A participant can be evaluated only after the training is Completed.");

        if (participant.Status != PCompleted)
            throw new BadRequestException("Only a participant who completed the training can be evaluated.");

        var evaluation = (await _uow.Repository<TrainingEvaluation>().FindAsync(e => e.TrainingParticipantId == participantId)).FirstOrDefault();
        if (evaluation == null)
        {
            evaluation = new TrainingEvaluation { TrainingParticipantId = participantId };
            await _uow.Repository<TrainingEvaluation>().AddAsync(evaluation);
        }
        else
        {
            evaluation.UpdatedDate = DateTime.UtcNow;
            _uow.Repository<TrainingEvaluation>().Update(evaluation);
        }

        evaluation.Score = dto.Score;
        evaluation.Feedback = dto.Feedback?.Trim();
        evaluation.EvaluatedDate = DateTime.UtcNow;

        await _uow.SaveChangesAsync();
        return (await MapParticipantsAsync(new[] { participant })).First();
    }

    // =====================================================================
    // EMPLOYEE TRAINING HISTORY
    // =====================================================================
    public async Task<List<TrainingHistoryDto>> GetEmployeeHistoryAsync(int employeeId)
    {
        if (await _uow.Repository<Employee>().GetByIdAsync(employeeId) is null)
            throw new NotFoundException(nameof(Employee), employeeId);

        var participants = await _uow.Repository<TrainingParticipant>().FindAsync(p => p.EmployeeId == employeeId);
        if (participants.Count == 0)
            return new List<TrainingHistoryDto>();

        var participantIds = participants.Select(p => p.Id).ToList();
        var scheduleIds = participants.Select(p => p.TrainingScheduleId).Distinct().ToList();

        var schedules = (await _uow.Repository<TrainingSchedule>().FindAsync(s => scheduleIds.Contains(s.Id))).ToDictionary(s => s.Id);
        var programs = (await _uow.Repository<TrainingProgram>().GetAllAsync()).ToDictionary(p => p.Id, p => p.ProgramName);
        var trainers = (await _uow.Repository<Trainer>().GetAllAsync()).ToDictionary(t => t.Id, t => t.TrainerName);
        var present = await _uow.Repository<TrainingAttendance>().FindAsync(a => participantIds.Contains(a.TrainingParticipantId) && a.IsPresent);
        var evaluations = (await _uow.Repository<TrainingEvaluation>().FindAsync(e => participantIds.Contains(e.TrainingParticipantId)))
            .ToDictionary(e => e.TrainingParticipantId);

        return participants
            .Where(p => schedules.ContainsKey(p.TrainingScheduleId))
            .Select(p =>
            {
                var schedule = schedules[p.TrainingScheduleId];
                var totalDays = DaysOf(schedule);
                var attended = present.Count(a => a.TrainingParticipantId == p.Id);
                evaluations.TryGetValue(p.Id, out var evaluation);

                return new TrainingHistoryDto
                {
                    ScheduleId = schedule.Id,
                    ProgramName = programs.GetValueOrDefault(schedule.TrainingProgramId),
                    TrainerName = schedule.TrainerId.HasValue ? trainers.GetValueOrDefault(schedule.TrainerId.Value) : null,
                    StartDateTime = schedule.StartDateTime,
                    EndDateTime = schedule.EndDateTime,
                    Venue = schedule.Venue,
                    ScheduleStatus = schedule.Status,
                    ParticipantStatus = p.Status,
                    AttendedDays = attended,
                    TotalDays = totalDays,
                    AttendancePercent = totalDays > 0 ? Math.Round((decimal)attended * 100m / totalDays, 1) : 0m,
                    Score = evaluation?.Score,
                    Feedback = evaluation?.Feedback
                };
            })
            .OrderByDescending(h => h.StartDateTime)
            .ToList();
    }

    // ---------------- Private Helpers ----------------

    private async Task<TrainingSchedule> LoadScheduleAsync(int id)
        => await _uow.Repository<TrainingSchedule>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(TrainingSchedule), id);

    private async Task<TrainingParticipant> LoadParticipantAsync(int id)
        => await _uow.Repository<TrainingParticipant>().GetByIdAsync(id) ?? throw new NotFoundException(nameof(TrainingParticipant), id);

    private async Task EnsureTrainerFreeAsync(int? trainerId, DateTime start, DateTime end, int? excludeScheduleId)
    {
        if (!trainerId.HasValue)
            return;

        var trainer = await _uow.Repository<Trainer>().GetByIdAsync(trainerId.Value)
            ?? throw new NotFoundException(nameof(Trainer), trainerId.Value);

        if (!trainer.IsActive)
            throw new BadRequestException("This Trainer is inactive.");

        var tid = trainerId.Value;
        var excludeId = excludeScheduleId ?? 0;
        if (await _uow.Repository<TrainingSchedule>().AnyAsync(s =>
                s.TrainerId == tid && s.Id != excludeId && s.Status != SCancelled && s.StartDateTime < end && s.EndDateTime > start))
            throw new BadRequestException("The trainer already has another training in this time range.");
    }

    private async Task<List<TrainingScheduleResponseDto>> MapSchedulesAsync(IEnumerable<TrainingSchedule> items)
    {
        var list = items.ToList();
        var programs = (await _uow.Repository<TrainingProgram>().GetAllAsync()).ToDictionary(p => p.Id, p => p.ProgramName);
        var trainers = (await _uow.Repository<Trainer>().GetAllAsync()).ToDictionary(t => t.Id, t => t.TrainerName);

        var ids = list.Select(s => s.Id).ToList();
        var participants = ids.Count == 0
            ? new List<TrainingParticipant>()
            : await _uow.Repository<TrainingParticipant>().FindAsync(p => ids.Contains(p.TrainingScheduleId) && p.Status != PDropped);

        var result = _mapper.Map<List<TrainingScheduleResponseDto>>(list);
        foreach (var dto in result)
        {
            dto.ProgramName = programs.GetValueOrDefault(dto.TrainingProgramId);
            dto.TrainerName = dto.TrainerId.HasValue ? trainers.GetValueOrDefault(dto.TrainerId.Value) : null;
            dto.TotalDays = (dto.EndDateTime.Date - dto.StartDateTime.Date).Days + 1;
            dto.ParticipantCount = participants.Count(p => p.TrainingScheduleId == dto.Id);
        }

        return result;
    }

    private async Task<List<TrainingParticipantResponseDto>> MapParticipantsAsync(IEnumerable<TrainingParticipant> items)
    {
        var list = items.ToList();
        var participantIds = list.Select(p => p.Id).ToList();
        var scheduleIds = list.Select(p => p.TrainingScheduleId).Distinct().ToList();

        var schedules = scheduleIds.Count == 0
            ? new Dictionary<int, TrainingSchedule>()
            : (await _uow.Repository<TrainingSchedule>().FindAsync(s => scheduleIds.Contains(s.Id))).ToDictionary(s => s.Id);

        var employees = (await _uow.Repository<Employee>().GetAllAsync()).ToDictionary(e => e.Id);
        var departments = (await _uow.Repository<Department>().GetAllAsync()).ToDictionary(d => d.Id, d => d.DepartmentName);

        var present = participantIds.Count == 0
            ? new List<TrainingAttendance>()
            : await _uow.Repository<TrainingAttendance>().FindAsync(a => participantIds.Contains(a.TrainingParticipantId) && a.IsPresent);

        var evaluations = participantIds.Count == 0
            ? new Dictionary<int, TrainingEvaluation>()
            : (await _uow.Repository<TrainingEvaluation>().FindAsync(e => participantIds.Contains(e.TrainingParticipantId))).ToDictionary(e => e.TrainingParticipantId);

        var result = _mapper.Map<List<TrainingParticipantResponseDto>>(list);
        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
                dto.DepartmentName = departments.GetValueOrDefault(employee.DepartmentId);
            }

            dto.TotalDays = schedules.TryGetValue(dto.TrainingScheduleId, out var schedule) ? DaysOf(schedule) : 0;
            dto.AttendedDays = present.Count(a => a.TrainingParticipantId == dto.Id);
            dto.AttendancePercent = dto.TotalDays > 0 ? Math.Round((decimal)dto.AttendedDays * 100m / dto.TotalDays, 1) : 0m;

            if (evaluations.TryGetValue(dto.Id, out var evaluation))
            {
                dto.Score = evaluation.Score;
                dto.Feedback = evaluation.Feedback;
            }
        }

        return result.OrderBy(r => (r.EmployeeCode ?? "").Length).ThenBy(r => r.EmployeeCode).ToList();
    }
}
