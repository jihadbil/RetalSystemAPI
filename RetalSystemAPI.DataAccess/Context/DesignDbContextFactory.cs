using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RetalSystemAPI.DataAccess.Context;

/// <summary>
/// مصنع للـ DbContext يُستخدم فقط في وقت التصميم (Design-Time) لتوليد الهجرات (Migrations) عبر أدوات EF Core CLI.
/// </summary>
public class DesignDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <summary>
    /// إنشاء كائن AppDbContext باستخدام سلسلة الاتصال المحددة في وقت التصميم.
    /// </summary>
    /// <param name="args">وسائط سطر الأوامر</param>
    /// <returns>نسخة مهيأة من AppDbContext</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        // محاولة قراءة appsettings.json من مشروع API أو المجلد الحالي
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(local);Database=RetalSystemDB;Trusted_Connection=True;MultipleActiveResultSets=true";

        var builder = new DbContextOptionsBuilder<AppDbContext>();
        builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        return new AppDbContext(builder.Options, null);
    }
}
