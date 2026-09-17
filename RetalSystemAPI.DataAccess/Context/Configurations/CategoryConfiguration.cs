using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API للتصنيفات الشجرية (Categories) والعلاقات الذاتية والتنظيمية.
/// </summary>
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "Categories"
        builder.ToTable("Categories");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(c => c.Id);

        // تكوين حقل اسم التصنيف كحقل إجباري بحد أقصى 150 حرفاً
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(c => c.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // تكوين العلاقة الذاتية للتصنيف الأب والفرعي مع منع الحذف التلقائي Restrict لحماية الهيكل الشجري
        builder.HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط التصنيف بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس على عمود TenantId لتسريع تصفية التصنيفات حسب المستأجر
        builder.HasIndex(c => c.TenantId);
        // إنشاء فهرس مركب على (TenantId, ParentCategoryId) لتسريع استعلامات الفئات الفرعية
        builder.HasIndex(c => new { c.TenantId, c.ParentCategoryId });
    }
}
