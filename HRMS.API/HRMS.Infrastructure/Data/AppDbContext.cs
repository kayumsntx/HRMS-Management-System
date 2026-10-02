using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Organization Setup
        public DbSet<Company> Companies => Set<Company>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Section> Sections => Set<Section>();
        public DbSet<Designation> Designations => Set<Designation>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<EmployeeStatus> EmployeeStatuses => Set<EmployeeStatus>();

        // Employee
        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<EmployeeEducation> EmployeeEducations => Set<EmployeeEducation>();
        public DbSet<EmployeeNominee> EmployeeNominees => Set<EmployeeNominee>();
        public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();

        // Security
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        // Biometric
        public DbSet<BiometricDevice> BiometricDevices => Set<BiometricDevice>();
        public DbSet<BiometricPunch> BiometricPunches => Set<BiometricPunch>();

        // Shift
        public DbSet<Shift> Shifts => Set<Shift>();
        public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();

        // Attendance
        public DbSet<EmployeeAttendance> EmployeeAttendances => Set<EmployeeAttendance>();

        // Leave
        public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
        public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances => Set<EmployeeLeaveBalance>();
        public DbSet<EmployeeLeave> EmployeeLeaves => Set<EmployeeLeave>();

        // Payroll
        public DbSet<EmployeeSalary> EmployeeSalaries => Set<EmployeeSalary>();
        public DbSet<Payroll> Payrolls => Set<Payroll>();
        public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // EmployeeLeaveBalance.RemainingDays computed property (entity =>  )
            // not DB column, Set EF Core  ignore 
            builder.Entity<EmployeeLeaveBalance>().Ignore(x => x.RemainingDays);

            base.OnModelCreating(builder);
        }
    }
}
