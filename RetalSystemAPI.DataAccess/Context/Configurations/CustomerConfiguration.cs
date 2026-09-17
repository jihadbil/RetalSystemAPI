using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبيانات الزبائن والعملاء (Customers) والحدود الائتمانية والأرصدة الافتتاحية.
/// </summary>
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "Customers"
        builder.ToTable("Customers");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(c => c.Id);

        // تكوين حقل اسم العميل كحقل إجباري بحد أقصى 200 حرف
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        // تكوين حقل كود العميل بحد أقصى 50 حرفاً
        builder.Property(c => c.Code)
            .HasMaxLength(50);

        // تكوين حقل البريد الإلكتروني بحد أقصى 150 حرفاً
        builder.Property(c => c.Email)
            .HasMaxLength(150);

        // تكوين حقل عنوان العميل بحد أقصى 500 حرف
        builder.Property(c => c.Address)
            .HasMaxLength(500);

        // ضبط نوع العمود للرصيد الافتتاحي بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(c => c.OpeningBalance)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع العمود للحد الائتماني المسموح به بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(c => c.CreditLimit)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // تفعيل حالة العميل افتراضياً لتكون true
        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(c => c.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط العميل بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس على عمود TenantId لتسريع تصفية العملاء حسب المستأجر
        builder.HasIndex(c => c.TenantId);
        // إنشاء فهرس مركب على (TenantId, Code) لتسريع البحث بكود العميل
        builder.HasIndex(c => new { c.TenantId, c.Code });
        // إنشاء فهرس مركب على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(c => new { c.TenantId, c.IsDeleted });
        // إنشاء فهرس مركب على (TenantId, IsDeleted, Name) لتسريع البحث بالاسم للعملاء النشطين
        builder.HasIndex(c => new { c.TenantId, c.IsDeleted, c.Name });
    }
}
