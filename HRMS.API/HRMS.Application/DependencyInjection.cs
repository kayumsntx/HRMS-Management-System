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
            return services;
        }
    }
}
