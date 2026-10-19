using AutoMapper;
using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Attendance;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Domain.Enums;

namespace HRMS.Application.Services;

public class AttendanceService : IAttendanceService
{
    // ---------------- Business rules: change these if your company policy differs ----------------
    private const DayOfWeek WeekendDay = DayOfWeek.Friday;     // weekly off day
    private const int MinGapBetweenPunchesMinutes = 10;        // closer punches are treated as one (double punch)
    private const int MinOvertimeMinutes = 30;                 // overtime below this is ignored
    private const decimal HalfDayThresholdHours = 4m;          // worked less than this = HalfDay
    private const int PunchWindowBufferHours = 4;              // night-shift punch window buffer
    private const string ManualTag = "[Manual]";               // marks manually corrected records

    private static readonly string StatusPresent = AttendanceDayStatus.Present.ToString();
    private static readonly string StatusAbsent = AttendanceDayStatus.Absent.ToString();
    private static readonly string StatusLate = AttendanceDayStatus.Late.ToString();
    private static readonly string StatusLeave = AttendanceDayStatus.Leave.ToString();
    private static readonly string StatusWeekend = AttendanceDayStatus.Weekend.ToString();
    private static readonly string StatusHoliday = AttendanceDayStatus.Holiday.ToString();
    private static readonly string StatusHalfDay = AttendanceDayStatus.HalfDay.ToString();

    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AttendanceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    // =====================================================================
    // PROCESS: raw punches -> daily attendance
    // =====================================================================
    public async Task<AttendanceProcessResultDto> ProcessAsync(AttendanceProcessDto dto)
    {
        var fromDate = dto.FromDate.Date;
        var toDate = dto.ToDate.Date;

        // ---- Employees ----
        List<Employee> employees;
        if (dto.EmployeeId.HasValue)
        {
            var single = await _unitOfWork.Repository<Employee>().GetByIdAsync(dto.EmployeeId.Value)
                ?? throw new NotFoundException(nameof(Employee), dto.EmployeeId.Value);
            employees = new List<Employee> { single };
        }
        else
        {
            employees = await _unitOfWork.Repository<Employee>().FindAsync(e => e.IsActive);
        }

        // ---- Reference data (loaded once, filtered in memory per employee/day) ----
        var shifts = (await _unitOfWork.Repository<Shift>().GetAllAsync()).ToDictionary(s => s.Id);

        var assignmentsByEmployee = (await _unitOfWork.Repository<ShiftAssignment>().FindAsync(a =>
                a.EffectiveFrom <= toDate && (a.EffectiveTo == null || a.EffectiveTo >= fromDate)))
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var approvedText = LeaveStatus.Approved.ToString();
        var leavesByEmployee = (await _unitOfWork.Repository<EmployeeLeave>().FindAsync(l =>
                l.Status == approvedText && l.FromDate <= toDate && l.ToDate >= fromDate))
            .GroupBy(l => l.EmployeeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var existingByKey = (await _unitOfWork.Repository<EmployeeAttendance>().FindAsync(a =>
                a.WorkingDate >= fromDate && a.WorkingDate <= toDate))
            .ToDictionary(a => (a.EmployeeId, a.WorkingDate.Date));

        var holidayDates = (await _unitOfWork.Repository<Holiday>().FindAsync(h =>
                h.IsActive && h.HolidayDate >= fromDate && h.HolidayDate <= toDate))
            .Select(h => h.HolidayDate.Date)
            .ToHashSet();

        // ---- Punches (range widened to cover night shifts) ----
        var punchStart = fromDate.AddDays(-1);
        var punchEnd = toDate.AddDays(3);
        var allPunches = await _unitOfWork.Repository<BiometricPunch>().FindAsync(p =>
            p.PunchDateTime >= punchStart && p.PunchDateTime < punchEnd);

        var fingerprintMap = employees
            .Where(e => !string.IsNullOrEmpty(e.FingerprintUserId))
            .GroupBy(e => e.FingerprintUserId!)
            .ToDictionary(g => g.Key, g => g.First().Id);

        var employeeIdSet = employees.Select(e => e.Id).ToHashSet();

        var punchesByEmployee = new Dictionary<int, List<BiometricPunch>>();
        foreach (var punch in allPunches)
        {
            var empId = punch.EmployeeId ?? (fingerprintMap.TryGetValue(punch.FingerprintUserId, out var mapped) ? mapped : (int?)null);
            if (empId is null || !employeeIdSet.Contains(empId.Value))
                continue;

            if (!punchesByEmployee.TryGetValue(empId.Value, out var bucket))
            {
                bucket = new List<BiometricPunch>();
                punchesByEmployee[empId.Value] = bucket;
            }
            bucket.Add(punch);
        }
        foreach (var bucket in punchesByEmployee.Values)
            bucket.Sort((a, b) => a.PunchDateTime.CompareTo(b.PunchDateTime));

        // ---- Main loop: employee x day ----
        var result = new AttendanceProcessResultDto { FromDate = fromDate, ToDate = toDate };

        foreach (var employee in employees)
        {
            var empAssignments = assignmentsByEmployee.GetValueOrDefault(employee.Id);
            var empLeaves = leavesByEmployee.GetValueOrDefault(employee.Id);
            var empPunches = punchesByEmployee.GetValueOrDefault(employee.Id) ?? new List<BiometricPunch>();
            var touched = false;

            for (var date = fromDate; date <= toDate; date = date.AddDays(1))
            {
                if (date < employee.DateOfJoining.Date)
                    continue;

                var assignment = empAssignments?
                    .Where(a => a.EffectiveFrom.Date <= date && (a.EffectiveTo == null || a.EffectiveTo.Value.Date >= date))
                    .OrderByDescending(a => a.EffectiveFrom)
                    .FirstOrDefault();

                if (assignment == null)
                {
                    result.SkippedNoShift++;
                    continue;
                }

                if (!shifts.TryGetValue(assignment.ShiftId, out var shift))
                {
                    result.SkippedNoShift++;
                    continue;
                }

                existingByKey.TryGetValue((employee.Id, date), out var existing);

                if (existing != null && existing.Remarks != null && existing.Remarks.StartsWith(ManualTag))
                {
                    result.SkippedManual++;
                    continue;
                }

                var (windowStart, windowEnd) = GetPunchWindow(date, shift);
                var dayPunches = empPunches.Where(p => p.PunchDateTime >= windowStart && p.PunchDateTime < windowEnd).ToList();

                DateTime? firstIn = null;
                DateTime? lastOut = null;
                if (dayPunches.Count > 0)
                {
                    firstIn = dayPunches[0].PunchDateTime;
                    var last = dayPunches[dayPunches.Count - 1].PunchDateTime;
                    if ((last - firstIn.Value).TotalMinutes >= MinGapBetweenPunchesMinutes)
                        lastOut = last;
                }

                var onLeave = empLeaves?.Any(l => l.FromDate.Date <= date && l.ToDate.Date >= date) ?? false;
                var metrics = Calculate(employee, shift, date, firstIn, lastOut, onLeave, holidayDates.Contains(date));

                if (existing == null)
                {
                    var record = new EmployeeAttendance { EmployeeId = employee.Id, WorkingDate = date, ShiftId = shift.Id };
                    ApplyMetrics(record, metrics);
                    await _unitOfWork.Repository<EmployeeAttendance>().AddAsync(record);
                    result.Created++;
                }
                else
                {
                    existing.ShiftId = shift.Id;
                    ApplyMetrics(existing, metrics);
                    existing.UpdatedDate = DateTime.UtcNow;
                    _unitOfWork.Repository<EmployeeAttendance>().Update(existing);
                    result.Updated++;
                }

                foreach (var punch in dayPunches.Where(p => !p.IsProcessed))
                {
                    punch.IsProcessed = true;
                    _unitOfWork.Repository<BiometricPunch>().Update(punch);
                }

                touched = true;
            }

            if (touched)
                result.EmployeesProcessed++;
        }

        await _unitOfWork.SaveChangesAsync();
        return result;
    }

    // =====================================================================
    // QUERY
    // =====================================================================
    public async Task<List<AttendanceResponseDto>> GetAsync(AttendanceFilterDto filter)
    {
        var today = DateTime.Now.Date;
        var startDate = filter.FromDate?.Date ?? (filter.ToDate.HasValue ? filter.ToDate.Value.Date.AddDays(-30) : new DateTime(today.Year, today.Month, 1));
        var endDate = filter.ToDate?.Date ?? today;
        var employeeId = filter.EmployeeId;
        var statusText = string.IsNullOrWhiteSpace(filter.DayStatus) ? null : ParseDayStatus(filter.DayStatus);

        var records = await _unitOfWork.Repository<EmployeeAttendance>().FindAsync(a =>
            a.WorkingDate >= startDate && a.WorkingDate <= endDate &&
            (!employeeId.HasValue || a.EmployeeId == employeeId) &&
            (statusText == null || a.DayStatus == statusText));

        if (filter.DepartmentId.HasValue)
        {
            var departmentId = filter.DepartmentId.Value;
            var departmentEmployeeIds = (await _unitOfWork.Repository<Employee>()
                .FindAsync(e => e.DepartmentId == departmentId)).Select(e => e.Id).ToHashSet();
            records = records.Where(r => departmentEmployeeIds.Contains(r.EmployeeId)).ToList();
        }

        return await MapAsync(records);
    }

    public async Task<AttendanceResponseDto> GetByIdAsync(int id)
    {
        var record = await _unitOfWork.Repository<EmployeeAttendance>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeAttendance), id);

        return (await MapAsync(new[] { record })).First();
    }

    // =====================================================================
    // MANUAL CORRECTION
    // =====================================================================
    public async Task<AttendanceResponseDto> CorrectAsync(int id, AttendanceCorrectionDto dto)
    {
        var attendance = await _unitOfWork.Repository<EmployeeAttendance>().GetByIdAsync(id)
            ?? throw new NotFoundException(nameof(EmployeeAttendance), id);

        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(attendance.EmployeeId)
            ?? throw new NotFoundException(nameof(Employee), attendance.EmployeeId);

        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(attendance.ShiftId)
            ?? throw new NotFoundException(nameof(Shift), attendance.ShiftId);

        var date = attendance.WorkingDate.Date;
        var firstIn = dto.FirstInTime ?? attendance.FirstInTime;
        var lastOut = dto.LastOutTime ?? attendance.LastOutTime;

        if (lastOut.HasValue && !firstIn.HasValue)
            throw new BadRequestException("Last Out Time requires a First In Time.");

        if (firstIn.HasValue && (firstIn.Value < date.AddDays(-1) || firstIn.Value >= date.AddDays(2)))
            throw new BadRequestException("First In Time must be within one day of the Working Date.");

        if (lastOut.HasValue && (lastOut.Value < date.AddDays(-1) || lastOut.Value >= date.AddDays(3)))
            throw new BadRequestException("Last Out Time must be within two days of the Working Date.");

        if (firstIn.HasValue && lastOut.HasValue && lastOut.Value <= firstIn.Value)
            throw new BadRequestException("Last Out Time must be after First In Time.");

        var isHoliday = await _unitOfWork.Repository<Holiday>().AnyAsync(h => h.IsActive && h.HolidayDate == date);
        var metrics = Calculate(employee, shift, date, firstIn, lastOut, false, isHoliday);
        ApplyMetrics(attendance, metrics);

        if (!string.IsNullOrWhiteSpace(dto.DayStatus))
            attendance.DayStatus = ParseDayStatus(dto.DayStatus);

        attendance.Remarks = $"{ManualTag} {dto.Remarks.Trim()}";
        attendance.UpdatedDate = DateTime.UtcNow;

        _unitOfWork.Repository<EmployeeAttendance>().Update(attendance);
        await _unitOfWork.SaveChangesAsync();

        return (await MapAsync(new[] { attendance })).First();
    }

    // =====================================================================
    // MONTHLY SUMMARY
    // =====================================================================
    public async Task<AttendanceSummaryDto> GetMonthlySummaryAsync(int employeeId, int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new BadRequestException("Please provide a valid year.");

        if (month < 1 || month > 12)
            throw new BadRequestException("Month must be between 1 and 12.");

        var employee = await _unitOfWork.Repository<Employee>().GetByIdAsync(employeeId)
            ?? throw new NotFoundException(nameof(Employee), employeeId);

        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var records = await _unitOfWork.Repository<EmployeeAttendance>().FindAsync(a =>
            a.EmployeeId == employeeId && a.WorkingDate >= monthStart && a.WorkingDate <= monthEnd);

        return new AttendanceSummaryDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullName,
            Year = year,
            Month = month,
            DaysInMonth = DateTime.DaysInMonth(year, month),
            ProcessedDays = records.Count,
            PresentDays = records.Count(r => r.DayStatus == StatusPresent),
            LateDays = records.Count(r => r.DayStatus == StatusLate),
            HalfDays = records.Count(r => r.DayStatus == StatusHalfDay),
            AbsentDays = records.Count(r => r.DayStatus == StatusAbsent),
            LeaveDays = records.Count(r => r.DayStatus == StatusLeave),
            WeekendDays = records.Count(r => r.DayStatus == StatusWeekend),
            TotalWorkingHours = records.Sum(r => r.WorkingHours ?? 0m),
            TotalLateHours = records.Sum(r => r.LateHours ?? 0m),
            TotalOvertimeHours = records.Sum(r => r.OTHours ?? 0m)
        };
    }

    // =====================================================================
    // PRIVATE HELPERS
    // =====================================================================
    private sealed class AttendanceMetrics
    {
        public DateTime? FirstIn { get; set; }
        public DateTime? LastOut { get; set; }
        public decimal WorkingHours { get; set; }
        public decimal LateHours { get; set; }
        public decimal EarlyOutHours { get; set; }
        public decimal OTHours { get; set; }
        public string DayStatus { get; set; } = default!;
        public string? Remarks { get; set; }
    }

    private static bool CrossesMidnight(Shift shift)
        => shift.IsNightShift || shift.EndTime <= shift.StartTime;

    private static (DateTime Start, DateTime End) GetPunchWindow(DateTime date, Shift shift)
    {
        if (!CrossesMidnight(shift))
            return (date, date.AddDays(1));

        var start = date + shift.StartTime - TimeSpan.FromHours(PunchWindowBufferHours);
        var end = date.AddDays(1) + shift.EndTime + TimeSpan.FromHours(PunchWindowBufferHours);
        return (start, end);
    }

    private static decimal ToHours(TimeSpan span) => Math.Round((decimal)span.TotalHours, 2);

    private static AttendanceMetrics Calculate(
        Employee employee, Shift shift, DateTime date, DateTime? firstIn, DateTime? lastOut, bool onLeave, bool isHoliday)
    {
        var shiftStart = date + shift.StartTime;
        var shiftEnd = (CrossesMidnight(shift) ? date.AddDays(1) : date) + shift.EndTime;

        var metrics = new AttendanceMetrics { FirstIn = firstIn, LastOut = lastOut };

        // ---- No punch at all ----
        if (firstIn is null)
        {
            if (isHoliday)
                metrics.DayStatus = StatusHoliday;
            else if (onLeave)
                metrics.DayStatus = StatusLeave;
            else if (date.DayOfWeek == WeekendDay)
                metrics.DayStatus = StatusWeekend;
            else
                metrics.DayStatus = StatusAbsent;

            return metrics;
        }

        var remarks = new List<string>();

        // ---- Late ----
        var isLate = firstIn.Value > shiftStart.AddMinutes(shift.GraceTimeMinutes);
        metrics.DayStatus = isLate ? StatusLate : StatusPresent;
        if (isLate)
            metrics.LateHours = ToHours(firstIn.Value - shiftStart);

        if (onLeave)
            remarks.Add("Punch recorded on an approved leave day.");

        // ---- Missing out punch ----
        if (lastOut is null)
        {
            remarks.Add("Missing out punch.");
            metrics.Remarks = string.Join(" ", remarks);
            return metrics;
        }

        // ---- Working / early out / half day / overtime ----
        metrics.WorkingHours = ToHours(lastOut.Value - firstIn.Value);

        if (lastOut.Value < shiftEnd)
            metrics.EarlyOutHours = ToHours(shiftEnd - lastOut.Value);

        if (metrics.WorkingHours < HalfDayThresholdHours)
            metrics.DayStatus = StatusHalfDay;

        if (employee.IsOvertimeEligible && lastOut.Value > shiftEnd)
        {
            var overtimeMinutes = (lastOut.Value - shiftEnd).TotalMinutes;
            if (overtimeMinutes >= MinOvertimeMinutes)
                metrics.OTHours = Math.Round((decimal)overtimeMinutes / 60m, 2);
        }

        metrics.Remarks = remarks.Count > 0 ? string.Join(" ", remarks) : null;
        return metrics;
    }

    private static void ApplyMetrics(EmployeeAttendance record, AttendanceMetrics metrics)
    {
        record.FirstInTime = metrics.FirstIn;
        record.LastOutTime = metrics.LastOut;
        record.WorkingHours = metrics.WorkingHours;
        record.LateHours = metrics.LateHours;
        record.EarlyOutHours = metrics.EarlyOutHours;
        record.OTHours = metrics.OTHours;
        record.DayStatus = metrics.DayStatus;
        record.Remarks = metrics.Remarks;
    }

    private static string ParseDayStatus(string value)
    {
        if (Enum.TryParse<AttendanceDayStatus>(value, true, out var parsed) &&
            Enum.IsDefined(typeof(AttendanceDayStatus), parsed))
            return parsed.ToString();

        throw new BadRequestException("Invalid Day Status. Allowed: " + string.Join(", ", Enum.GetNames<AttendanceDayStatus>()) + ".");
    }

    private async Task<List<AttendanceResponseDto>> MapAsync(IEnumerable<EmployeeAttendance> records)
    {
        var employees = (await _unitOfWork.Repository<Employee>().GetAllAsync()).ToDictionary(e => e.Id);
        var shifts = (await _unitOfWork.Repository<Shift>().GetAllAsync()).ToDictionary(s => s.Id, s => s.ShiftName);

        var result = _mapper.Map<List<AttendanceResponseDto>>(records.ToList());
        foreach (var dto in result)
        {
            if (employees.TryGetValue(dto.EmployeeId, out var employee))
            {
                dto.EmployeeCode = employee.EmployeeCode;
                dto.EmployeeName = employee.FullName;
            }
            dto.ShiftName = shifts.GetValueOrDefault(dto.ShiftId);
        }

        return result.OrderByDescending(r => r.WorkingDate).ThenBy(r => r.EmployeeCode).ToList();
    }
}
