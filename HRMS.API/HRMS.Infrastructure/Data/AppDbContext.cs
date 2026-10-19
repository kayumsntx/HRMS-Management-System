using System.Text.Json;
using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Common;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUser;

    public AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService? currentUser = null) : base(options)
    {
        _currentUser = currentUser;
    }

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
    public DbSet<Holiday> Holidays => Set<Holiday>();

    // Leave
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances => Set<EmployeeLeaveBalance>();
    public DbSet<EmployeeLeave> EmployeeLeaves => Set<EmployeeLeave>();

    // Payroll
    public DbSet<EmployeeSalary> EmployeeSalaries => Set<EmployeeSalary>();
    public DbSet<Payroll> Payrolls => Set<Payroll>();
    public DbSet<PayrollDetail> PayrollDetails => Set<PayrollDetail>();

    // System
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<IdCard> IdCards => Set<IdCard>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // RemainingDays is a computed property, not a database column.
        builder.Entity<EmployeeLeaveBalance>().Ignore(x => x.RemainingDays);

        base.OnModelCreating(builder);
    }

    // =====================================================================
    // AUTOMATIC AUDIT LOG
    // Every Create / Update / Delete made through SaveChangesAsync is recorded.
    // =====================================================================

    private static readonly HashSet<Type> AuditExcludedTypes = new()
    {
        typeof(AuditLog),        // never audit the audit log itself
        typeof(Notification),    // too noisy
        typeof(BiometricPunch)   // very high volume
    };

    private static readonly HashSet<string> SensitiveProperties = new() { "PasswordHash" };   // value is masked
    private static readonly HashSet<string> NoiseProperties = new() { "UpdatedDate", "LastLoginDate" };
    private const int BulkThreshold = 200;   // above this, one summary row is written instead of one row per record

    private sealed class AuditInfo
    {
        public BaseEntity? Entity { get; set; }
        public string TableName { get; set; } = default!;
        public string ActionType { get; set; } = default!;
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; set; } = new();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var pending = CollectAuditEntries();

        var result = await base.SaveChangesAsync(cancellationToken);

        if (pending.Count > 0)
        {
            WriteAuditLogs(pending);          // ids of newly created rows are known now
            await base.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    private List<AuditInfo> CollectAuditEntries()
    {
        ChangeTracker.DetectChanges();

        var infos = new List<AuditInfo>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified && entry.State != EntityState.Deleted)
                continue;

            if (AuditExcludedTypes.Contains(entry.Entity.GetType()))
                continue;

            var info = new AuditInfo
            {
                Entity = entry.Entity,
                TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                ActionType = entry.State == EntityState.Added ? "Create"
                           : entry.State == EntityState.Deleted ? "Delete"
                           : "Update"
            };

            foreach (var property in entry.Properties)
            {
                var name = property.Metadata.Name;
                var sensitive = SensitiveProperties.Contains(name);

                if (entry.State == EntityState.Added)
                {
                    if (name == "Id") continue;   // temporary value before save
                    info.NewValues[name] = sensitive ? "***" : property.CurrentValue;
                }
                else if (entry.State == EntityState.Deleted)
                {
                    info.OldValues[name] = sensitive ? "***" : property.OriginalValue;
                }
                else if (property.IsModified && !NoiseProperties.Contains(name) &&
                         !Equals(property.OriginalValue, property.CurrentValue))
                {
                    info.OldValues[name] = sensitive ? "***" : property.OriginalValue;
                    info.NewValues[name] = sensitive ? "***" : property.CurrentValue;
                }
            }

            if (entry.State == EntityState.Modified && info.NewValues.Count == 0)
                continue;   // nothing really changed

            infos.Add(info);
        }

        if (infos.Count > BulkThreshold)
        {
            var summary = infos
                .GroupBy(i => i.TableName + " (" + i.ActionType + ")")
                .ToDictionary(g => g.Key, g => (object?)g.Count());

            return new List<AuditInfo>
            {
                new AuditInfo
                {
                    Entity = null,
                    TableName = string.Join(",", infos.Select(i => i.TableName).Distinct().Take(5)),
                    ActionType = "Bulk",
                    NewValues = summary
                }
            };
        }

        return infos;
    }

    private void WriteAuditLogs(List<AuditInfo> infos)
    {
        var now = DateTime.UtcNow;

        foreach (var info in infos)
        {
            AuditLogs.Add(new AuditLog
            {
                UserId = _currentUser?.UserId,
                UserName = _currentUser?.UserName,
                ActionType = info.ActionType,
                TableName = info.TableName,
                RecordId = info.Entity?.Id.ToString(),
                OldValues = info.OldValues.Count > 0 ? JsonSerializer.Serialize(info.OldValues) : null,
                NewValues = info.NewValues.Count > 0 ? JsonSerializer.Serialize(info.NewValues) : null,
                IpAddress = _currentUser?.IpAddress,
                ActivityDate = now
            });
        }
    }
}
