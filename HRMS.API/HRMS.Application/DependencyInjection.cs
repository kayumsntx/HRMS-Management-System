using FluentValidation;
using HRMS.Application.Interfaces.Services;
using HRMS.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            //AutoMapper  MappingProfile  auto

            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());

            //FluentValidation  Validator  auto 
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            // Services
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IDepartmentService, DepartmentService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<IDesignationService, DesignationService>();
            services.AddScoped<IGradeService, GradeService>();
            return services;
        }
    }
}
