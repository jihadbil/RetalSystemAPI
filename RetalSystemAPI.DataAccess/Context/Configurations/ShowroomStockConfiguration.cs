using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لأرصدة صالات العرض (ShowroomStocks) لكل صنف ومخزن.
/// </summary>
public class ShowroomStockConfiguration : IEntityTypeConfiguration<ShowroomStock>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<ShowroomStock> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "ShowroomStocks"
        builder.ToTable("ShowroomStocks");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(ss => ss.Id);

        // تعيين القيمة الافتراضية لرصيد صالة العرض بـ 0
        builder.Property(ss => ss.Quantity)
            .HasDefaultValue(0);

        // تعيين القيمة الافتراضية للحد الأدنى للعرض بـ 0
        builder.Property(ss => ss.MinStockLevel)
            .HasDefaultValue(0);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(ss => ss.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط رصيد الصالة بالمستودع أو الصالة التابع لها مع تفعيل الحذف المتتالي Cascade
        builder.HasOne(ss => ss.Warehouse)
            .WithMany(w => w.ShowroomStocks)
            .HasForeignKey(ss => ss.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط الرصيد بالمنتج مع منع الحذف التلقائي Restrict
        builder.HasOne(ss => ss.Product)
            .WithMany(p => p.ShowroomStocks)
            .HasForeignKey(ss => ss.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط السجل بالمستأجر مع منع الإجراء NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(ss => ss.Tenant)
            .WithMany()
            .HasForeignKey(ss => ss.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس مركب فريد على (WarehouseId, ProductId) لضمان سجل رصيد واحد لكل صنف في كل صالة عرض
        builder.HasIndex(ss => new { ss.WarehouseId, ss.ProductId })
            .IsUnique();

        // إنشاء فهرس على عمود TenantId لتسريع تصفية أرصدة الصالات حسب المستأجر
        builder.HasIndex(ss => ss.TenantId);
        // إنشاء فهرس على عمود WarehouseId لتسريع استعراض معروضات صالة العرض بالكامل
        builder.HasIndex(ss => ss.WarehouseId);
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(ss => new { ss.TenantId, ss.IsDeleted });
    }
}
