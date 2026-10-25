using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Leave;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.UnitTests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.UnitTests;

public class LeaveFlowTests
{
    private static async Task<(int EmployeeId, int LeaveTypeId)> SeedAsync(IServiceScope scope, decimal openingBalance)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var type = new LeaveType { LeaveTypeName = "Casual Leave", IsPaid = true };
        var employee = new Employee { EmployeeCode = "1001", FullName = "Test Employee", DateOfJoining = new DateTime(2025, 1, 1) };
        db.LeaveTypes.Add(type);
        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        db.EmployeeLeaveBalances.Add(new EmployeeLeaveBalance
        {
            EmployeeId = employee.Id,
            LeaveTypeId = type.Id,
            LeaveYear = 2026,
            OpeningBalance = openingBalance
        });
        await db.SaveChangesAsync();

        return (employee.Id, type.Id);
    }

    private static LeaveApplyDto Request(int employeeId, int leaveTypeId, int fromDay, int toDay) => new()
    {
        EmployeeId = employeeId,
        LeaveTypeId = leaveTypeId,
        FromDate = new DateTime(2026, 3, fromDay),
        ToDate = new DateTime(2026, 3, toDay)
    };

    [Fact]
    public async Task Approved_leave_uses_up_the_balance()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var (employeeId, typeId) = await SeedAsync(scope, 10m);
        var leave = scope.ServiceProvider.GetRequiredService<ILeaveService>();

        var applied = await leave.ApplyAsync(Request(employeeId, typeId, 2, 4));
        Assert.Equal("Pending", applied.Status);
        Assert.Equal(3m, applied.NumberOfDays);

        var approved = await leave.ApproveAsync(applied.Id);
        Assert.Equal("Approved", approved.Status);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var balance = await db.EmployeeLeaveBalances.SingleAsync();
        Assert.Equal(3m, balance.UsedDays);
        Assert.Equal(7m, balance.RemainingDays);
    }

    [Fact]
    public async Task Leave_is_refused_when_the_balance_is_too_low()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var (employeeId, typeId) = await SeedAsync(scope, 2m);
        var leave = scope.ServiceProvider.GetRequiredService<ILeaveService>();

        await Assert.ThrowsAsync<BadRequestException>(() => leave.ApplyAsync(Request(employeeId, typeId, 2, 4)));
    }

    [Fact]
    public async Task Overlapping_leave_is_refused()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var (employeeId, typeId) = await SeedAsync(scope, 10m);
        var leave = scope.ServiceProvider.GetRequiredService<ILeaveService>();

        await leave.ApplyAsync(Request(employeeId, typeId, 2, 4));

        await Assert.ThrowsAsync<BadRequestException>(() => leave.ApplyAsync(Request(employeeId, typeId, 4, 5)));
    }

    [Fact]
    public async Task Cancelling_an_approved_leave_gives_the_days_back()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var (employeeId, typeId) = await SeedAsync(scope, 10m);
        var leave = scope.ServiceProvider.GetRequiredService<ILeaveService>();

        var applied = await leave.ApplyAsync(Request(employeeId, typeId, 2, 4));
        await leave.ApproveAsync(applied.Id);
        var cancelled = await leave.CancelAsync(applied.Id);

        Assert.Equal("Cancelled", cancelled.Status);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Equal(0m, (await db.EmployeeLeaveBalances.SingleAsync()).UsedDays);
    }

    [Fact]
    public async Task A_decided_leave_cannot_be_approved_twice()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var (employeeId, typeId) = await SeedAsync(scope, 10m);
        var leave = scope.ServiceProvider.GetRequiredService<ILeaveService>();

        var applied = await leave.ApplyAsync(Request(employeeId, typeId, 2, 4));
        await leave.ApproveAsync(applied.Id);

        await Assert.ThrowsAsync<BadRequestException>(() => leave.ApproveAsync(applied.Id));
    }
}
