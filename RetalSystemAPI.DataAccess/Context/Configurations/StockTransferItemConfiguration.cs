using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود أوامر التحويل المخزني (StockTransferItems).
/// </summary>
public class StockTransferItemConfiguration : IEntityTypeConfiguration<StockTransferItem>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<StockTransferItem> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "StockTransferItems"
        builder.ToTable("StockTransferItems");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(sti => sti.Id);

        // تكوين حقل كمية البند المحول كحقل إجباري
        builder.Property(sti => sti.Quantity)
            .IsRequired();

        // تكوين حقل ملاحظات سطر التحويل بحد أقصى 500 حرف
        builder.Property(sti => sti.Notes)
            .HasMaxLength(500);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(sti => sti.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط السطر بأمر التحويل الرئيسي مع تفعيل الحذف المتتالي Cascade لحذف البنود عند حذف أمر التحويل
        builder.HasOne(sti => sti.StockTransfer)
            .WithMany(st => st.Items)
            .HasForeignKey(sti => sti.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط البند المحول بالمنتج مع منع الحذف التلقائي Restrict
        builder.HasOne(sti => sti.Product)
            .WithMany()
            .HasForeignKey(sti => sti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند المحول بالباركود أو النكهة اختيارياً مع منع الحذف Restrict
        builder.HasOne(sti => sti.ProductBarCode)
            .WithMany()
            .HasForeignKey(sti => sti.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند بالمستأجر مع منع الإجراء NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(sti => sti.Tenant)
            .WithMany()
            .HasForeignKey(sti => sti.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس على عمود StockTransferId لتسريع جلب بنود أمر التحويل
        builder.HasIndex(sti => sti.StockTransferId);
        // إنشاء فهرس على عمود ProductId لتسريع تتبع تحويلات المنتج
        builder.HasIndex(sti => sti.ProductId);
        // إنشاء فهرس على عمود ProductBarCodeId لتسريع تتبع تحويلات النكهة
        builder.HasIndex(sti => sti.ProductBarCodeId);
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(sti => new { sti.TenantId, sti.IsDeleted });
    }
}
