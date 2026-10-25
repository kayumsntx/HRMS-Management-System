using HRMS.Application.Common.Interfaces;
using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, IPasswordHasher passwordHasher, string? adminPassword)
    {
        await context.Database.MigrateAsync();

        // ---- Roles ----
        var roles = new Dictionary<string, string>
        {
            { "Admin", "System administrator" },
            { "HR", "HR staff" },
            { "Manager", "Line manager" },
            { "Employee", "General employee" }
        };

        foreach (var role in roles)
        {
            if (!await context.Roles.AnyAsync(r => r.RoleName == role.Key))
                context.Roles.Add(new Role { RoleName = role.Key, Description = role.Value });
        }
        await context.SaveChangesAsync();

        // ---- First admin user (created only when it does not exist yet) ----
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
        if (adminUser is null)
        {
            if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword.Length < 8)
                throw new InvalidOperationException(
                    "No admin user exists yet. Configure 'Seed:AdminPassword' (at least 8 characters) to create the first admin account.");

            adminUser = new User
            {
                UserName = "admin",
                Email = "admin@hrms.local",
                PasswordHash = passwordHasher.Hash(adminPassword)
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

        var adminRole = await context.Roles.FirstAsync(r => r.RoleName == "Admin");
        if (!await context.UserRoles.AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id))
        {
            context.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
            await context.SaveChangesAsync();
        }

        // ---- Employee statuses ----
        foreach (var name in new[] { "Probation", "Confirmed", "Contractual", "Resigned", "Terminated" })
        {
            if (!await context.EmployeeStatuses.AnyAsync(s => s.StatusName == name))
                context.EmployeeStatuses.Add(new EmployeeStatus { StatusName = name });
        }

        // ---- Leave types ----
        var leaveTypes = new (string Name, bool IsPaid)[]
        {
            ("Casual Leave", true),
            ("Sick Leave", true),
            ("Annual Leave", true),
            ("Maternity Leave", true),
            ("Leave Without Pay", false)
        };
        foreach (var lt in leaveTypes)
        {
            if (!await context.LeaveTypes.AnyAsync(l => l.LeaveTypeName == lt.Name))
                context.LeaveTypes.Add(new LeaveType { LeaveTypeName = lt.Name, IsPaid = lt.IsPaid });
        }

        await context.SaveChangesAsync();
    }
}
