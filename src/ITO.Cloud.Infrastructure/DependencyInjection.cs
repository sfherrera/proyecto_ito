using ITO.Cloud.Application.Common.Interfaces;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Domain.Interfaces;
using ITO.Cloud.Domain.Interfaces.Services;
using ITO.Cloud.Infrastructure.Reports;
using ITO.Cloud.Infrastructure.AI;
using ITO.Cloud.Infrastructure.Identity;
using ITO.Cloud.Infrastructure.Persistence;
using ITO.Cloud.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ITO.Cloud.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // HttpContextAccessor
        services.AddHttpContextAccessor();

        // Interceptor de auditoría
        services.AddScoped<AuditInterceptor>();

        // Servicios de identidad
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantContext, TenantContextService>();
        services.AddScoped<JwtTokenService>();

        // EF Core + PostgreSQL
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(3);
                })
                .UseSnakeCaseNamingConvention();

            options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // ASP.NET Identity — usar AddIdentityCore para no sobrescribir el esquema JWT
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit           = true;
            options.Password.RequiredLength         = 8;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase       = true;
            options.User.RequireUniqueEmail         = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager();

        // IApplicationDbContext — abstracción para la capa Application
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Reportes
        services.AddScoped<IReportService, ReportService>();

        // IA — stub hasta implementar OpenAI
        services.AddScoped<IAIService, AIServiceStub>();

        return services;
    }
}
