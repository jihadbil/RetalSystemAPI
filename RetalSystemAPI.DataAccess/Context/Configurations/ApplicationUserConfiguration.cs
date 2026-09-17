using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لكيان المستخدم (ApplicationUser) لربطه بالفرع والمستأجر والفهارس.
/// </summary>
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        // تكوين علاقة المستخدم بالفرع (واحد إلى متعدد) مع منع الحذف التلقائي Restrict
        builder.HasOne(u => u.Branch)
            .WithMany(b => b.ApplicationUsers)
            .HasForeignKey(u => u.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // تكوين علاقة المستخدم بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس على عمود TenantId لتسريع عمليات الفلترة والاستعلام حسب المستأجر
        builder.HasIndex(u => u.TenantId);
        // إنشاء فهرس على عمود BranchId لتسريع الاستعلام عن مستخدمي كل فرع
        builder.HasIndex(u => u.BranchId);
    }
}
