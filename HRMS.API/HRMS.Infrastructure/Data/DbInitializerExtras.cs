using HRMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Infrastructure.Data;

public static class DbInitializerExtras
{
    public static async Task SeedAsync(AppDbContext context)
    {
        var requisitionTypes = new[]
        {
            ("Employee", "General employee requisition"),
            ("Tiffin", "Tiffin / meal requisition"),
            ("WH", "Warehouse requisition"),
            ("Office", "Office supply requisition")
        };

        foreach (var (name, description) in requisitionTypes)
        {
            if (!await context.RequisitionTypes.AnyAsync(t => t.TypeName == name))
                context.RequisitionTypes.Add(new RequisitionType { TypeName = name, Description = description });
        }

        foreach (var name in new[] { "Cash", "Bank Transfer", "Mobile Banking" })
        {
            if (!await context.PaymentTypes.AnyAsync(t => t.PaymentTypeName == name))
                context.PaymentTypes.Add(new PaymentType { PaymentTypeName = name });
        }

        await context.SaveChangesAsync();
    }
}
