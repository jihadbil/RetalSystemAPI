using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لأرصدة المستودعات الرئيسية (StorgeStocks) لكل باركود ونكهة ومخزن.
/// </summary>
public class StorgeStockConfiguration : IEntityTypeConfiguration<StorgeStock>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<StorgeStock> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "StorgeStocks"
        builder.ToTable("StorgeStocks");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(ss => ss.Id);

        // تعيين القيمة الافتراضية لرصيد الكمية بـ 0
        builder.Property(ss => ss.Quantity)
            .HasDefaultValue(0);

        // تعيين القيمة الافتراضية للحد الأدنى للطلب (Reorder Level) بـ 0
        builder.Property(ss => ss.MinStockLevel)
            .HasDefaultValue(0);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(ss => ss.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط رصيد المخزن بالمستودع مع تفعيل الحذف المتتالي Cascade
        builder.HasOne(ss => ss.Warehouse)
            .WithMany(w => w.WarehouseStocks)
            .HasForeignKey(ss => ss.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط الرصيد بباركود ونكهة الصنف مع منع الحذف التلقائي Restrict
        builder.HasOne(ss => ss.ProductBarcode)
            .WithMany(pb => pb.StorgeStocks)
            .HasForeignKey(ss => ss.ProductBarcodeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الرصيد بالمستأجر مع منع الإجراء NoAction لتفادي المسارات المتعددة
        builder.HasOne(ss => ss.Tenant)
            .WithMany()
            .HasForeignKey(ss => ss.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس مركب فريد على (WarehouseId, ProductBarcodeId) لضمان سجل رصيد واحد لكل نكهة داخل كل مستودع
        builder.HasIndex(ss => new { ss.WarehouseId, ss.ProductBarcodeId })
            .IsUnique();

        // إنشاء فهرس على عمود TenantId لتسريع تصفية الأرصدة حسب المستأجر
        builder.HasIndex(ss => ss.TenantId);
        // إنشاء فهرس على عمود WarehouseId لتسريع استعراض جرد المستودع بالكامل
        builder.HasIndex(ss => ss.WarehouseId);
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(ss => new { ss.TenantId, ss.IsDeleted });
    }
}
