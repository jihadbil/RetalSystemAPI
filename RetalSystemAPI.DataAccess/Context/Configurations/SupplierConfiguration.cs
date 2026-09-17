using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبيانات الموردين (Suppliers) والأرصدة الافتتاحية والفهارس.
/// </summary>
public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "Suppliers"
        builder.ToTable("Suppliers");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(s => s.Id);

        // تكوين حقل اسم المورد كحقل إجباري بحد أقصى 200 حرف
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        // تكوين حقل عنوان المورد بحد أقصى 500 حرف
        builder.Property(s => s.Address)
            .HasMaxLength(500);

        // ضبط نوع العمود للرصيد الافتتاحي بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(s => s.OpeningBalance)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(s => s.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط المورد بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس على عمود TenantId لتسريع تصفية الموردين حسب المستأجر
        builder.HasIndex(s => s.TenantId);
        // إنشاء فهرس مركب على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(s => new { s.TenantId, s.IsDeleted });
        // إنشاء فهرس مركب على (TenantId, IsDeleted, Name) لتسريع البحث بالاسم للموردين النشطين
        builder.HasIndex(s => new { s.TenantId, s.IsDeleted, s.Name });
    }
}
