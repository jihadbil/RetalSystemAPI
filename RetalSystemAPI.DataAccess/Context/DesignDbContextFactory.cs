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
    /// <param name="args">وسائط سطر الأوامر الممررة من سطر الأوامر</param>
    /// <returns>نسخة مهيأة من AppDbContext صالحة لإنشاء وتطبيق الهجرات</returns>
    public AppDbContext CreateDbContext(string[] args)
    {
        // استخراج المسار الحالي لمجلد العمل قيد التنفيذ
        var basePath = Directory.GetCurrentDirectory();

        // بناء كائن قراءة الإعدادات والملفات لتحديد ملف appsettings.json
        var configuration = new ConfigurationBuilder()
            // تحديد المجلد الأساسي للبحث عن ملفات الإعدادات
            .SetBasePath(basePath)
            // قراءة ملف الإعدادات الأساسي appsettings.json إن وجد مع تفعيل إعادة التحميل التلقائي
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            // قراءة ملف إعدادات بيئة التطوير إن وجد لتجاوز القيم السابقة
            .AddJsonFile("appsettings.Development.json", optional: true)
            // تجميع وبناء كائن التكوين النهائي
            .Build();

        // قراءة سلسلة اتصال قاعدة البيانات أو استخدام السلسلة المحلية الافتراضية كاحتياط
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=(local);Database=RetalSystemDB;Trusted_Connection=True;MultipleActiveResultSets=true";

        // إنشاء كائن بناء خيارات DbContextOptionsBuilder الخاص بـ AppDbContext
        var builder = new DbContextOptionsBuilder<AppDbContext>();
        // ضبط خيارات الاتصال بمحرك SQL Server وتحديد تجميعة الهجرات لطبقة DataAccess
        builder.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));

        // إرجاع كائن AppDbContext مهيأ بالخيارات مع تمرير null لخدمة المستأجر في وقت التصميم
        return new AppDbContext(builder.Options, null);
    }
}
