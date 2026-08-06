using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Interceptors;
using RetalSystemAPI.DataAccess.Repositories.Implementations;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Services;

namespace RetalSystemAPI.DataAccess.Extensions;

/// <summary>
/// امتدادات تسجيل خدمات DataAccess في كاوية الإعتمادية (Dependency Injection Container).
/// </summary>
public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── HttpContext & User/Tenant Services ────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── Interceptors ──────────────────────────────────────
        services.AddScoped<AuditInterceptor>();

        // ── DbContext ─────────────────────────────────────────
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditInterceptor>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            options
                .UseSqlServer(
                    connectionString,
                    sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
                )
                .AddInterceptors(interceptor);
        });

        // ── Generic Repository & Unit of Work ─────────────────
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
