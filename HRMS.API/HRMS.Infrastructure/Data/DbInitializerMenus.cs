using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

// Default sidebar, default role access and default salary components. Runs only when the tables are empty.
public static class DbInitializerMenus
{
    private sealed record M(string Name, string? Url, string Icon, string? Parent, int Sort);

    public static async Task SeedAsync(AppDbContext context)
    {
        var items = new List<M>();

        void Leaf(string name, string url, string icon, int sort) => items.Add(new M(name, url, icon, null, sort));
        void Group(string name, string icon, int sort, params (string Name, string Url)[] children)
        {
            items.Add(new M(name, null, icon, null, sort));
            var i = 1;
            foreach (var c in children)
                items.Add(new M(c.Name, c.Url, "circle", name, i++));
        }

        Leaf("Dashboard", "/dashboard", "dashboard", 1);
        Group("Organization", "business", 2, ("Companies", "/organization/companies"), ("Units", "/organization/units"),
            ("Departments", "/organization/departments"), ("Sections", "/organization/sections"),
            ("Designations", "/organization/designations"), ("Grades", "/organization/grades"));
        Group("Employees", "people", 3, ("Employee List", "/employees"), ("ID Cards", "/employees/id-cards"));
        Group("Attendance", "schedule", 4, ("Daily Attendance", "/attendance/daily"), ("Biometric Devices", "/attendance/devices"),
            ("Shifts", "/attendance/shifts"), ("Holidays", "/attendance/holidays"));
        Group("Leave", "event", 5, ("Leave Requests", "/leave/requests"), ("Leave Types", "/leave/types"), ("Leave Policies", "/leave/policies"));
        Group("Payroll", "payments", 6, ("Salaries", "/payroll/salaries"), ("Payroll Runs", "/payroll/runs"),
            ("Loans", "/payroll/loans"), ("Bonuses", "/payroll/bonuses"));
        Group("Recruitment", "person_add", 7, ("Vacancies", "/recruitment/vacancies"), ("Applicants", "/recruitment/applicants"));
        Group("Performance", "insights", 8, ("Evaluations", "/performance/evaluations"), ("KPIs", "/performance/kpis"));
        Group("Training", "school", 9, ("Training Schedules", "/training/schedules"), ("Training Programs", "/training/programs"));
        Leaf("Reports", "/reports", "assessment", 10);
        Group("Security", "security", 11, ("Users", "/security/users"), ("Roles", "/security/roles"),
            ("Menu Permissions", "/security/menu-permissions"), ("Audit Log", "/security/audit-log"), ("Login History", "/security/login-history"));
        Group("Team", "groups", 12, ("Team Leaves", "/team/leaves"), ("Team Evaluations", "/team/evaluations"));
        Group("My Space", "account_circle", 13, ("My Profile", "/my/profile"), ("My Attendance", "/my/attendance"),
            ("My Leave", "/my/leave"), ("My Payslips", "/my/payslips"), ("My Loans", "/my/loans"));

        // ---- Menus ----
        if (!await context.Menus.AnyAsync())
        {
            var created = new Dictionary<string, Menu>();

            foreach (var m in items.Where(i => i.Parent == null))
            {
                var menu = new Menu { MenuName = m.Name, MenuUrl = m.Url, IconClass = m.Icon, SortOrder = m.Sort };
                context.Menus.Add(menu);
                created[m.Name] = menu;
            }
            await context.SaveChangesAsync();

            foreach (var m in items.Where(i => i.Parent != null))
            {
                var menu = new Menu { MenuName = m.Name, MenuUrl = m.Url, IconClass = m.Icon, SortOrder = m.Sort, ParentMenuId = created[m.Parent!].Id };
                context.Menus.Add(menu);
                created[m.Name] = menu;
            }
            await context.SaveChangesAsync();
        }

        // ---- Default role access (Admin always has full access anyway; HR sees everything except Security) ----
        var hrBlocked = new HashSet<string> { "Security", "Users", "Roles", "Menu Permissions", "Audit Log", "Login History" };
        var mySpace = new[] { "My Space", "My Profile", "My Attendance", "My Leave", "My Payslips", "My Loans" };
        var managerSet = new HashSet<string>(mySpace.Concat(new[] { "Team", "Team Leaves", "Team Evaluations" }));
        var employeeSet = new HashSet<string>(mySpace);

        var rules = new (string Role, Func<string, bool> Allowed, bool Full)[]
        {
            ("Admin", _ => true, true),
            ("HR", n => !hrBlocked.Contains(n), true),
            ("Manager", n => managerSet.Contains(n), false),
            ("Employee", n => employeeSet.Contains(n), false)
        };

        var allMenus = await context.Menus.ToListAsync();

        foreach (var (roleName, allowed, full) in rules)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
            if (role is null || await context.MenuPermissions.AnyAsync(p => p.RoleId == role.Id))
                continue;

            foreach (var menu in allMenus.Where(m => allowed(m.MenuName)))
            {
                context.MenuPermissions.Add(new MenuPermission
                {
                    RoleId = role.Id,
                    MenuId = menu.Id,
                    CanView = true,
                    CanCreate = full,
                    CanEdit = full,
                    CanDelete = full
                });
            }
        }
        await context.SaveChangesAsync();

        // ---- Default salary components ----
        var components = new[]
        {
            ("House Rent", "Allowance"), ("Medical Allowance", "Allowance"), ("Conveyance", "Allowance"),
            ("Provident Fund", "Deduction"), ("Income Tax", "Deduction")
        };

        foreach (var (name, type) in components)
        {
            if (!await context.SalaryComponents.AnyAsync(c => c.ComponentName == name))
                context.SalaryComponents.Add(new SalaryComponent { ComponentName = name, ComponentType = type });
        }
        await context.SaveChangesAsync();
    }
}
