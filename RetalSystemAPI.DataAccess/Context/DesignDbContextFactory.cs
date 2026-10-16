using System.IO;
using System.Reflection;
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
    /// <param name="args">وسائط سطر الأوامر الممررة من سطر الأوامر</param>
    /// <returns>نسخة مهيأة من AppDbContext صالحة لإنشاء وتطبيق الهجرات</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        if (!File.Exists(Path.Combine(basePath, "appsettings.json"))
            && File.Exists(Path.Combine(basePath, "RetalSystemAPI", "RetalSystemAPI.csproj")))
            basePath = Path.Combine(basePath, "RetalSystemAPI");

        using var overrides = (ConfigurationRoot)new ConfigurationBuilder().AddEnvironmentVariables().AddCommandLine(args).Build();
        var environment = overrides["environment"] ?? overrides["DOTNET_ENVIRONMENT"]
            ?? overrides["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var configurationBuilder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true);
        if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                configurationBuilder.AddUserSecrets(Assembly.Load("RetalSystemAPI"), optional: true);
            }
            catch (FileNotFoundException)
            {
                // The API assembly is optional when DataAccess is the startup project.
            }
        }
        using var configuration = (ConfigurationRoot)configurationBuilder.AddEnvironmentVariables().AddCommandLine(args).Build();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required for EF tools. Configure it through environment variables, user-secrets or command-line configuration.");

        // إنشاء كائن بناء خيارات DbContextOptionsBuilder الخاص بـ AppDbContext
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        // ضبط خيارات الاتصال بمحرك SQL Server وتحديد تجميعة الهجرات لطبقة DataAccess
        builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        // إرجاع كائن AppDbContext مهيأ بالخيارات مع تمرير null لخدمة المستأجر في وقت التصميم
        return new AppDbContext(builder.Options, null);
    }
}
