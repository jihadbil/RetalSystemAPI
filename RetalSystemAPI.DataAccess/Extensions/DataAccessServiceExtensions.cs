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
        // تسجيل IHttpContextAccessor للتمكن من قراءة بيانات الطلب والمستخدم الحالي من أي مكان
        services.AddHttpContextAccessor();
        // تسجيل خدمة استخراج معرف المستأجر بنطاق الطلب Scoped
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        // تسجيل خدمة استخراج معرف المستخدم بنطاق الطلب Scoped
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── Interceptors ──────────────────────────────────────
        // تسجيل معترض التدقيق لحقن التواريخ والمستخدمين تلقائياً عند حفظ التغييرات
        services.AddScoped<AuditInterceptor>();

        // ── DbContext ─────────────────────────────────────────
        // تسجيل سياق قاعدة البيانات AppDbContext مع ضبط خيارات الاتصال ومحرك SQL Server
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            // استخراج كائن AuditInterceptor المحقون من حاوية الخدمات
            var interceptor = sp.GetRequiredService<AuditInterceptor>();
            // جلب سلسلة الاتصال بقاعدة البيانات من ملف التكوين
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // تكوين خيارات الاتصال بمحرك SQL Server
            options
                .UseSqlServer(
                    connectionString,
                    sql =>
                    {
                        // تحديد تجميعة الهجرات لطبقة DataAccess لتوليد ملفات الـ Migrations فيها
                        sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                        // تفعيل تقسيم الاستعلامات لتجنب التضخم الديكارتي (Cartesian Explosion) عند تعدد الـ Includes
                        sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                    }
                )
                // إضافة معترض التدقيق لسياق البيانات
                .AddInterceptors(interceptor)
                // تجاهل تحذيرات التغييرات المعلقة في النموذج لتفادي تعطيل التشغيل
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // ── Identity ──────────────────────────────────────────
        // تسجيل الهوية الأساسية ApplicationUser مع ضبط سياسات كلمات المرور المخففة
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            // عدم إلزام وجود أرقام في كلمة المرور
            options.Password.RequireDigit = false;
            // عدم إلزام وجود أحرف صغيرة
            options.Password.RequireLowercase = false;
            // عدم إلزام وجود رموز خاصة
            options.Password.RequireNonAlphanumeric = false;
            // عدم إلزام وجود أحرف كبيرة
            options.Password.RequireUppercase = false;
            // تحديد الحد الأدنى لطول كلمة المرور بـ 6 خانات
            options.Password.RequiredLength = 6;
            // عدم إلزام فرادة البريد الإلكتروني لدعم أسماء المستخدمين المخصصة
            options.User.RequireUniqueEmail = false;
        })
        // إضافة دعم إدارة الأدوار ومصفوفات الصلاحيات
        .AddRoles<IdentityRole>()
        // ربط مخزن الهوية بسياق قاعدة البيانات AppDbContext
        .AddEntityFrameworkStores<AppDbContext>()
        // إضافة مزودي الرموز الافتراضية لاستعادة كلمات المرور وتأكيد الحسابات
        .AddDefaultTokenProviders();

        // ── Generic Repository & Unit of Work ─────────────────
        // تسجيل واجهة المستودع العام المفتوح IRepository<> مع تنفيذه Repository<>
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        // تسجيل واجهة وحدة العمل IUnitOfWork مع تنفيذها UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // إرجاع مجموعة الخدمات لدعم نمط الربط المتسلسل (Fluent Chaining)
        return services;
    }
}
