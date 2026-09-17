using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لكيان الفروع (Branches) وإعداد الأطوال والفهارس والربط مع المستأجر.
/// </summary>
public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "Branches"
        builder.ToTable("Branches");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(b => b.Id);

        // تكوين حقل اسم الفرع كحقل إجباري بحد أقصى 150 حرفاً
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(150);

        // تكوين حقل عنوان الفرع بحد أقصى 500 حرف
        builder.Property(b => b.Address)
            .HasMaxLength(500);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(b => b.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // تكوين علاقة الفرع بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس على عمود TenantId لتسريع عمليات الفلترة حسب المستأجر
        builder.HasIndex(b => b.TenantId);
        // إنشاء فهرس مركب على (TenantId, IsActive) لتسريع جلب الفروع النشطة لكل مستأجر
        builder.HasIndex(b => new { b.TenantId, b.IsActive });
    }
}
