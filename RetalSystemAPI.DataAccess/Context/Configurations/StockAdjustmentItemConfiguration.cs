using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود تسوية الجرد المخزني (StockAdjustmentItems) وفروقات الجرد.
/// </summary>
public class StockAdjustmentItemConfiguration : IEntityTypeConfiguration<StockAdjustmentItem>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<StockAdjustmentItem> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "StockAdjustmentItems"
        builder.ToTable("StockAdjustmentItems");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(sai => sai.Id);

        // تكوين حقل الكمية المسجلة دفترياً في النظام كحقل إجباري
        builder.Property(sai => sai.SystemQuantity)
            .IsRequired();

        // تكوين حقل الكمية الفعلية المحصورة أثناء الجرد كحقل إجباري
        builder.Property(sai => sai.ActualQuantity)
            .IsRequired();

        // تكوين حقل فارق الجرد (الفعلي - الدفتري) كحقل إجباري
        builder.Property(sai => sai.DifferenceQuantity)
            .IsRequired();

        // ضبط نوع العمود لتكلفة الوحدة بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(sai => sai.UnitCost)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // تحويل قيمة الـ Enum لسبب الفارق الجردي إلى عدد صحيح int في قاعدة البيانات
        builder.Property(sai => sai.Reason)
            .HasConversion<int>();

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(sai => sai.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط سطر التسوية بمستند تسوية الجرد الرئيسي مع تفعيل الحذف المتتالي Cascade
        builder.HasOne(sai => sai.StockAdjustment)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sai => sai.StockAdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط بند التسوية بالمنتج مع منع الحذف التلقائي Restrict
        builder.HasOne(sai => sai.Product)
            .WithMany()
            .HasForeignKey(sai => sai.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط بند التسوية بالباركود أو النكهة اختيارياً مع منع الحذف Restrict
        builder.HasOne(sai => sai.ProductBarCode)
            .WithMany()
            .HasForeignKey(sai => sai.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند بالمستأجر مع منع الإجراء NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(sai => sai.Tenant)
            .WithMany()
            .HasForeignKey(sai => sai.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس على عمود StockAdjustmentId لتسريع جلب بنود التسوية
        builder.HasIndex(sai => sai.StockAdjustmentId);
        // إنشاء فهرس على عمود ProductId لتسريع تتبع فروقات المنتج
        builder.HasIndex(sai => sai.ProductId);
        // إنشاء فهرس على عمود ProductBarCodeId لتسريع تتبع فروقات النكهة
        builder.HasIndex(sai => sai.ProductBarCodeId);
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(sai => new { sai.TenantId, sai.IsDeleted });
    }
}
