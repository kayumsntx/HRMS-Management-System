using HRMS.Application.DTOs.Companies;
using HRMS.Application.Interfaces.Services;
using HRMS.Infrastructure.Data;
using HRMS.UnitTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.UnitTests;

public class AuditLogTests
{
    [Fact]
    public async Task Creating_a_record_writes_an_audit_row()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var companies = scope.ServiceProvider.GetRequiredService<ICompanyService>();

        var created = await companies.CreateAsync(new CompanyCreateDto { CompanyCode = "C001", CompanyName = "Acme Ltd" });

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var log = await db.AuditLogs.SingleAsync(l => l.ActionType == "Create");

        Assert.Equal(created.Id.ToString(), log.RecordId);
        Assert.Equal("tester", log.UserName);
        Assert.Contains("Acme Ltd", log.NewValues);
    }
}
