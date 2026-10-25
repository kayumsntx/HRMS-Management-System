using HRMS.Application.Interfaces.Services;
using HRMS.UnitTests.TestSupport;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.UnitTests;

public class DependencyInjectionTests
{
    // Fails if any registered service needs something that is not registered (or a scoped service is captured by a singleton).
    [Fact]
    public void Container_builds_and_validates()
    {
        using var provider = TestHost.Build(validate: true);
        Assert.NotNull(provider);
    }

    // Fails if you forgot a services.AddScoped<IXyzService, XyzService>() line.
    [Fact]
    public void Every_service_interface_has_a_registration()
    {
        using var provider = TestHost.Build();
        using var scope = provider.CreateScope();

        var interfaces = typeof(IAuthService).Assembly.GetTypes()
            .Where(t => t.IsInterface && !t.IsGenericType && t.Namespace == "HRMS.Application.Interfaces.Services");

        var missing = interfaces
            .Where(i => scope.ServiceProvider.GetService(i) is null)
            .Select(i => i.Name)
            .OrderBy(n => n)
            .ToList();

        Assert.True(missing.Count == 0, "Missing DI registrations: " + string.Join(", ", missing));
    }
}
