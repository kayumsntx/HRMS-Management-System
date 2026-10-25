using System.Reflection;
using FluentValidation;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Interfaces.Services;
using HRMS.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // ---- Organization ----
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IDesignationService, DesignationService>();
        services.AddScoped<IGradeService, GradeService>();
        services.AddScoped<IEmployeeStatusService, EmployeeStatusService>();
        services.AddScoped<ISubSectionService, SubSectionService>();
        services.AddScoped<IJobLocationService, JobLocationService>();
        services.AddScoped<IStaffCategoryService, StaffCategoryService>();
        services.AddScoped<ISkillCategoryService, SkillCategoryService>();
        services.AddScoped<IPaymentTypeService, PaymentTypeService>();
        services.AddScoped<IBankService, BankService>();

        // ---- Employee ----
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IEmployeeSearchService, EmployeeSearchService>();
        services.AddScoped<IEmployeeEducationService, EmployeeEducationService>();
        services.AddScoped<IEmployeeNomineeService, EmployeeNomineeService>();
        services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();
        services.AddScoped<IEmployeeSkillService, EmployeeSkillService>();
        services.AddScoped<IEmployeePreviousJobService, EmployeePreviousJobService>();
        services.AddScoped<IEmployeeAddressService, EmployeeAddressService>();
        services.AddScoped<IEmployeeBankAccountService, EmployeeBankAccountService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IIdCardService, IdCardService>();
        services.AddScoped<IIdCardDocumentService, IdCardDocumentService>();

        // ---- Security ----
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILoginHistoryService, LoginHistoryService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IMenuPermissionService, MenuPermissionService>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        // ---- Biometric / Shift / Attendance ----
        services.AddScoped<IBiometricDeviceService, BiometricDeviceService>();
        services.AddScoped<IBiometricPunchService, BiometricPunchService>();
        services.AddScoped<IBiometricSyncHistoryService, BiometricSyncHistoryService>();
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IShiftAssignmentService, ShiftAssignmentService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IAttendanceLockService, AttendanceLockService>();
        services.AddScoped<IShortLeaveService, ShortLeaveService>();

        // ---- Leave ----
        services.AddScoped<ILeaveTypeService, LeaveTypeService>();
        services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
        services.AddScoped<ILeaveService, LeaveService>();
        services.AddScoped<ILeavePolicyService, LeavePolicyService>();
        services.AddScoped<ILeaveBalanceGenerationService, LeaveBalanceGenerationService>();
        services.AddScoped<ITeamLeaveService, TeamLeaveService>();

        // ---- Payroll ----
        services.AddScoped<ISalaryService, SalaryService>();
        services.AddScoped<ISalaryComponentService, SalaryComponentService>();
        services.AddScoped<ISalaryBreakdownService, SalaryBreakdownService>();
        services.AddScoped<IBonusService, BonusService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IPayrollService, PayrollService>();
        services.AddScoped<IMyPayslipService, MyPayslipService>();

        // ---- HR processes ----
        services.AddScoped<IRequisitionTypeService, RequisitionTypeService>();
        services.AddScoped<IRequisitionService, RequisitionService>();
        services.AddScoped<ICareerService, CareerService>();
        services.AddScoped<ISeparationService, SeparationService>();
        services.AddScoped<IRecruitmentService, RecruitmentService>();
        services.AddScoped<IHiringService, HiringService>();
        services.AddScoped<IPerformancePeriodService, PerformancePeriodService>();
        services.AddScoped<IKpiService, KpiService>();
        services.AddScoped<IPerformanceService, PerformanceService>();
        services.AddScoped<ITrainingProgramService, TrainingProgramService>();
        services.AddScoped<ITrainerService, TrainerService>();
        services.AddScoped<ITrainingService, TrainingService>();
        services.AddScoped<ITeamService, TeamService>();

        // ---- Notification / Dashboard / Reports ----
        services.AddScoped<INotificationPublisher, NotificationPublisher>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
