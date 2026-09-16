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
/// فئة امتدادات (Extension Methods) لتسجيل وتكوين كافة خدمات طبقة الوصول للبيانات (DataAccess) في حاوية حقن الاعتماديات (DI Container).
/// </summary>
public static class DataAccessServiceExtensions
{
    /// <summary>
    /// تسجيل خدمات AppDbContext و SQL Server و Identity و AuditInterceptor و IRepository و IUnitOfWork.
    /// </summary>
    /// <param name="services">مجموعة خدمات التطبيق IServiceCollection</param>
    /// <param name="configuration">إعدادات التكوين IConfiguration لجلب سلاسل الاتصال</param>
    /// <returns>مجموعة الخدمات بعد التسجيل</returns>
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
                    sql =>
                    {
                        sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
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
