using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لجدول الوحدات الأساسية (Units) وضمان عدم تكرار اسم الوحدة للمستأجر نفسه.
/// </summary>
public class UnitConfiguration : IEntityTypeConfiguration<Unit>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<Unit> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "Units"
        builder.ToTable("Units");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(u => u.Id);

        // تكوين حقل اسم الوحدة كحقل إجباري بحد أقصى 100 حرف
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        // تكوين حقل وصف الوحدة بحد أقصى 500 حرف
        builder.Property(u => u.Description)
            .HasMaxLength(500);

        // تعيين معامل التعبئة الافتراضي للوحدة بـ 1
        builder.Property(u => u.UnitPackage)
            .HasDefaultValue(1);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(u => u.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الوحدة بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس على عمود TenantId لتسريع تصفية الوحدات حسب المستأجر
        builder.HasIndex(u => u.TenantId);
        // إنشاء فهرس مركب فريد على (TenantId, Name) لمنع تكرار اسم الوحدة لنفس المستأجر
        builder.HasIndex(u => new { u.TenantId, u.Name })
            .IsUnique();
    }
}
