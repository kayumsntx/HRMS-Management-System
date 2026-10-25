using HRMS.Application.Common.Interfaces;
using HRMS.Application.Interfaces.Repositories;
using HRMS.Infrastructure.Data;
using HRMS.Infrastructure.Documents;
using HRMS.Infrastructure.Identity;
using HRMS.Infrastructure.Repositories;
using HRMS.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ?? configuration.GetConnectionString("con")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<IIdCardPdfGenerator, IdCardPdfGenerator>();

        return services;
    }
}
