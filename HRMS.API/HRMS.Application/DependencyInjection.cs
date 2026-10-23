using HRMS.Application.Common.Interfaces;
using FluentValidation;
using HRMS.Application.Interfaces.Services;
using HRMS.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //AutoMapper  MappingProfile  auto
            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());

            //FluentValidation  Validator  auto 
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Existing Services
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IGradeService, GradeService>();
            services.AddScoped<IEmployeeStatusService, EmployeeStatusService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IEmployeeEducationService, EmployeeEducationService>();
            services.AddScoped<IEmployeeNomineeService, EmployeeNomineeService>();
            services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();
            services.AddScoped<IRoleService, RoleService>();

            // Added Services - Part I
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserRoleService, UserRoleService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IShiftService, ShiftService>();
            services.AddScoped<IShiftAssignmentService, ShiftAssignmentService>();
            services.AddScoped<ILeaveTypeService, LeaveTypeService>();
            services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
            services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<IBiometricDeviceService, BiometricDeviceService>();

            // Added Services - Part F
            services.AddScoped<IBiometricPunchService, BiometricPunchService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<ISalaryService, SalaryService>();
            services.AddScoped<IPayrollService, PayrollService>();

            // System & Notifications & Holidays & IdCards
            services.AddScoped<IHolidayService, HolidayService>();
            services.AddScoped<IAuditLogService, AuditLogService>();
            services.AddScoped<INotificationPublisher, NotificationPublisher>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IIdCardService, IdCardService>();
            // MySpace, TeamLeave, Dashboard, Reports
            services.AddScoped<IMyPayslipService, MyPayslipService>();
            services.AddScoped<ITeamLeaveService, TeamLeaveService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IReportService, ReportService>();
            // HR Modules - Requisition, Career, Separation, Recruitment, Performance, Training
            services.AddScoped<IRequisitionTypeService, RequisitionTypeService>();
            services.AddScoped<ITrainingProgramService, TrainingProgramService>();
            services.AddScoped<ITrainerService, TrainerService>();
            services.AddScoped<IPerformancePeriodService, PerformancePeriodService>();
            services.AddScoped<IKpiService, KpiService>();
            services.AddScoped<IRequisitionService, RequisitionService>();
            services.AddScoped<ICareerService, CareerService>();
            services.AddScoped<ISeparationService, SeparationService>();
            services.AddScoped<IRecruitmentService, RecruitmentService>();
            services.AddScoped<IHiringService, HiringService>();
            services.AddScoped<IPerformanceService, PerformanceService>();
            services.AddScoped<ITrainingService, TrainingService>();
            services.AddScoped<IFileService, FileService>();
            services.AddScoped<IIdCardDocumentService, IdCardDocumentService>();
            services.AddScoped<ITeamService, TeamService>();
            // Organization masters, Employee extras, Leave policies, Bonuses, Loans
            services.AddScoped<ISubSectionService, SubSectionService>();
            services.AddScoped<IJobLocationService, JobLocationService>();
            services.AddScoped<IStaffCategoryService, StaffCategoryService>();
            services.AddScoped<ISkillCategoryService, SkillCategoryService>();
            services.AddScoped<IPaymentTypeService, PaymentTypeService>();
            services.AddScoped<IBankService, BankService>();
            services.AddScoped<IEmployeeSkillService, EmployeeSkillService>();
            services.AddScoped<IEmployeePreviousJobService, EmployeePreviousJobService>();
            services.AddScoped<IEmployeeAddressService, EmployeeAddressService>();
            services.AddScoped<IEmployeeBankAccountService, EmployeeBankAccountService>();
            services.AddScoped<ILeavePolicyService, LeavePolicyService>();
            services.AddScoped<ILeaveBalanceGenerationService, LeaveBalanceGenerationService>();
            services.AddScoped<IBonusService, BonusService>();
            services.AddScoped<ILoanService, LoanService>();

            // Phase 8 - Operations & Security Extras
            services.AddScoped<ILoginHistoryService, LoginHistoryService>();
            services.AddScoped<IMenuService, MenuService>();
            services.AddScoped<IMenuPermissionService, MenuPermissionService>();
            services.AddScoped<IAttendanceLockService, AttendanceLockService>();
            services.AddScoped<IShortLeaveService, ShortLeaveService>();
            services.AddScoped<ISalaryComponentService, SalaryComponentService>();
            services.AddScoped<ISalaryBreakdownService, SalaryBreakdownService>();
            services.AddScoped<IBiometricSyncHistoryService, BiometricSyncHistoryService>();
            services.AddScoped<IEmployeeSearchService, EmployeeSearchService>();
            return services;
        }
    }
}
