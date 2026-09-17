using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API للمخازن والمستودعات والصالات (Warehouses) وأنواعها وربطها بالفروع.
/// </summary>
public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "Warehouses"
        builder.ToTable("Warehouses");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(w => w.Id);

        // تكوين حقل اسم المخزن كحقل إجباري بحد أقصى 150 حرفاً
        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(150);

        // تعيين نوع المخزن (رئيسي، فرعي، صالة عرض..) كحقل إجباري
        builder.Property(w => w.Type)
            .IsRequired();

        // تفعيل حالة المستودع افتراضياً لتكون true
        builder.Property(w => w.IsActive)
            .HasDefaultValue(true);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(w => w.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط المستودع بالفرع التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(w => w.Branch)
            .WithMany(b => b.Warehouses)
            .HasForeignKey(w => w.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المستودع بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(w => w.Tenant)
            .WithMany()
            .HasForeignKey(w => w.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس على عمود TenantId لتسريع تصفية المستودعات حسب المستأجر
        builder.HasIndex(w => w.TenantId);
        // إنشاء فهرس مركب على (TenantId, BranchId) لتسريع استعراض مستودعات الفرع
        builder.HasIndex(w => new { w.TenantId, w.BranchId });
        // إنشاء فهرس مركب على (TenantId, IsActive) لتسريع جلب المستودعات النشطة
        builder.HasIndex(w => new { w.TenantId, w.IsActive });
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(w => new { w.TenantId, w.IsDeleted });
    }
}
