using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لكيان الأصناف (Products) وتحديد دقة الحقول المالية والارتباط بالتصنيف.
/// </summary>
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "Products"
        builder.ToTable("Products");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(p => p.Id);

        // تكوين حقل اسم الصنف كحقل إجباري بحد أقصى 200 حرف
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        // تكوين حقل وصف الصنف بحد أقصى 2000 حرف
        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        // ضبط نوع العمود لسعر التكلفة بدقة مالية متقدمة decimal(18,4)
        builder.Property(p => p.CostPrice)
            .HasColumnType("decimal(18,4)");

        // ضبط نوع العمود لسعر البيع بدقة مالية متقدمة decimal(18,4)
        builder.Property(p => p.SalePrice)
            .HasColumnType("decimal(18,4)");

        // ضبط نوع العمود لمتوسط سعر التكلفة المرجح بدقة مالية متقدمة decimal(18,4)
        builder.Property(p => p.AveragePrice)
            .HasColumnType("decimal(18,4)");

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل لمنع التعديلات المتضاربة
        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الصنف بالتصنيف التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الصنف بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس على عمود TenantId لتسريع تصفية المنتجات حسب المستأجر
        builder.HasIndex(p => p.TenantId);
        // إنشاء فهرس مركب على (TenantId, CategoryId) لتسريع فلترة المنتجات حسب التصنيف
        builder.HasIndex(p => new { p.TenantId, p.CategoryId });
        // إنشاء فهرس مركب على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(p => new { p.TenantId, p.IsDeleted });
        // إنشاء فهرس مركب على (TenantId, IsDeleted, Name) لتسريع البحث بالاسم للأصناف النشطة
        builder.HasIndex(p => new { p.TenantId, p.IsDeleted, p.Name });
    }
}
