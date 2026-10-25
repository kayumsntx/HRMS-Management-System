using HRMS.Application;
using HRMS.Application.Common.Interfaces;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.UnitTests.TestSupport;

public static class TestHost
{
    // Builds the same service container as production, but with an in-memory database and fake infrastructure.
    public static ServiceProvider Build(bool validate = false)
    {
        var databaseName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddApplication();

        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(databaseName));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICurrentUserService, FakeCurrentUser>();
        services.AddScoped<IPasswordHasher, FakePasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, FakeJwt>();
        services.AddSingleton<IFileStorage, FakeFileStorage>();
        services.AddSingleton<IIdCardPdfGenerator, FakePdfGenerator>();

        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = validate, ValidateScopes = validate });
    }
}
