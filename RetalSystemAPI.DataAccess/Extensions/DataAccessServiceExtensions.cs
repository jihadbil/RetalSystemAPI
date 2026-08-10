using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Interceptors;
using RetalSystemAPI.Models;
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
                .AddInterceptors(interceptor)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // ── Identity ──────────────────────────────────────────
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = false;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // ── Generic Repository & Unit of Work ─────────────────
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
