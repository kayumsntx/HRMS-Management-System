using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

public static class DbInitializerExtras
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var types = new[]
        {
            ("Employee", "General employee requisition"),
            ("Tiffin", "Tiffin / meal requisition"),
            ("WH", "Warehouse requisition"),
            ("Office", "Office supply requisition")
        };

        foreach (var (name, description) in types)
        {
            if (!await context.RequisitionTypes.AnyAsync(t => t.TypeName == name))
                context.RequisitionTypes.Add(new RequisitionType { TypeName = name, Description = description });
        }

        await context.SaveChangesAsync();
    }
}
