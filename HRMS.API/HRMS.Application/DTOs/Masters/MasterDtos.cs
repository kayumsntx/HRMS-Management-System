namespace HRMS.Application.DTOs.Masters;

// ---- Requisition type ----
public class RequisitionTypeCreateDto { public string TypeName { get; set; } = default!; public string? Description { get; set; } }
public class RequisitionTypeUpdateDto : RequisitionTypeCreateDto { public bool IsActive { get; set; } = true; }
public class RequisitionTypeResponseDto { public int Id { get; set; } public string TypeName { get; set; } = default!; public string? Description { get; set; } public bool IsActive { get; set; } }

// ---- Training program ----
public class TrainingProgramCreateDto { public string ProgramName { get; set; } = default!; public string? Description { get; set; } public int? DurationHours { get; set; } }
public class TrainingProgramUpdateDto : TrainingProgramCreateDto { public bool IsActive { get; set; } = true; }
public class TrainingProgramResponseDto { public int Id { get; set; } public string ProgramName { get; set; } = default!; public string? Description { get; set; } public int? DurationHours { get; set; } public bool IsActive { get; set; } }

// ---- Trainer ----
public class TrainerCreateDto { public string TrainerName { get; set; } = default!; public int? EmployeeId { get; set; } public string? Organization { get; set; } public string? Mobile { get; set; } public string? Email { get; set; } }
public class TrainerUpdateDto : TrainerCreateDto { public bool IsActive { get; set; } = true; }
public class TrainerResponseDto { public int Id { get; set; } public string TrainerName { get; set; } = default!; public int? EmployeeId { get; set; } public string? Organization { get; set; } public string? Mobile { get; set; } public string? Email { get; set; } public bool IsActive { get; set; } }

// ---- Performance period ----
public class PerformancePeriodCreateDto { public string PeriodName { get; set; } = default!; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } }
public class PerformancePeriodUpdateDto : PerformancePeriodCreateDto { public bool IsActive { get; set; } = true; }
public class PerformancePeriodResponseDto { public int Id { get; set; } public string PeriodName { get; set; } = default!; public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public bool IsActive { get; set; } }

// ---- KPI ----
public class KpiCreateDto { public string KpiName { get; set; } = default!; public int? DepartmentId { get; set; } public int? DesignationId { get; set; } public decimal Weight { get; set; } public string? Description { get; set; } }
public class KpiUpdateDto : KpiCreateDto { public bool IsActive { get; set; } = true; }
public class KpiResponseDto { public int Id { get; set; } public string KpiName { get; set; } = default!; public int? DepartmentId { get; set; } public int? DesignationId { get; set; } public decimal Weight { get; set; } public string? Description { get; set; } public bool IsActive { get; set; } }
