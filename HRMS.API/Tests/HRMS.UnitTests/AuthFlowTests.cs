using HRMS.Application.Common.Exceptions;
using HRMS.Application.DTOs.Users;
using HRMS.Application.Interfaces.Services;
using HRMS.Domain.Entities;
using HRMS.Infrastructure.Data;
using HRMS.UnitTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.UnitTests;

public class AuthFlowTests
{
    private static readonly UserLoginDto Correct = new() { UserName = "alice", Password = "Secret@123" };
    private static readonly UserLoginDto Wrong = new() { UserName = "alice", Password = "wrong" };

    private static async Task<IAuthService> PrepareAsync(IServiceScope scope)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(new User { UserName = "alice", PasswordHash = "hash:Secret@123" });
        await db.SaveChangesAsync();

        return scope.ServiceProvider.GetRequiredService<IAuthService>();
    }

    [Fact]
    public async Task Login_returns_access_and_refresh_tokens()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var auth = await PrepareAsync(scope);

        var result = await auth.LoginAsync(Correct, "127.0.0.1", "tests");

        Assert.Equal("token", result.Token);
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
    }

    [Fact]
    public async Task Wrong_password_is_rejected()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var auth = await PrepareAsync(scope);

        await Assert.ThrowsAsync<UnauthorizedException>(() => auth.LoginAsync(Wrong, "127.0.0.1", "tests"));
    }

    [Fact]
    public async Task Five_wrong_passwords_lock_the_account()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var auth = await PrepareAsync(scope);

        for (var i = 0; i < 5; i++)
            await Assert.ThrowsAsync<UnauthorizedException>(() => auth.LoginAsync(Wrong, "127.0.0.1", "tests"));

        // even the correct password is refused while the account is locked
        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() => auth.LoginAsync(Correct, "127.0.0.1", "tests"));
        Assert.Contains("Too many", ex.Message);
    }

    [Fact]
    public async Task Refresh_token_works_once_and_reuse_closes_all_sessions()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();
        var auth = await PrepareAsync(scope);

        var first = await auth.LoginAsync(Correct, "127.0.0.1", "tests");
        var second = await auth.RefreshAsync(first.RefreshToken, "127.0.0.1");

        Assert.NotEqual(first.RefreshToken, second.RefreshToken);

        // using the old token again means it may have been stolen
        await Assert.ThrowsAsync<UnauthorizedException>(() => auth.RefreshAsync(first.RefreshToken, "127.0.0.1"));

        // ... so even the newest token is dead now
        await Assert.ThrowsAsync<UnauthorizedException>(() => auth.RefreshAsync(second.RefreshToken, "127.0.0.1"));
    }
}
