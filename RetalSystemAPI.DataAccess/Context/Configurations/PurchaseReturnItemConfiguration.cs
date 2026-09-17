using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود فواتير مرتجعات المشتريات (PurchaseReturnItems).
/// </summary>
public class PurchaseReturnItemConfiguration : IEntityTypeConfiguration<PurchaseReturnItem>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "PurchaseReturnItems"
        builder.ToTable("PurchaseReturnItems");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(pri => pri.Id);

        // ضبط نوع العمود لكمية البند المرتجع كحقل إجباري بدقة عشرية (18, 2)
        builder.Property(pri => pri.Quantity)
            .HasPrecision(18, 2)
            .IsRequired();

        // ضبط نوع العمود لسعر إرجاع الوحدة بدقة مالية (18, 4) مع قيمة افتراضية صفر
        builder.Property(pri => pri.UnitPrice)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        // ضبط نوع العمود لإجمالي قيمة السطر المرتجع بدقة مالية (18, 4) مع قيمة افتراضية صفر
        builder.Property(pri => pri.LineTotal)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        // تكوين حقل ملاحظات سطر المرتجع بحد أقصى 500 حرف
        builder.Property(pri => pri.Notes)
            .HasMaxLength(500);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(pri => pri.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط السطر بفاتورة مرتجع المشتريات الرئيسية مع تفعيل الحذف المتتالي Cascade
        builder.HasOne(pri => pri.PurchaseReturn)
            .WithMany(pr => pr.Items)
            .HasForeignKey(pri => pri.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط البند المرتجع بالمنتج مع منع الحذف التلقائي Restrict
        builder.HasOne(pri => pri.Product)
            .WithMany()
            .HasForeignKey(pri => pri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند المرتجع بالباركود أو النكهة اختيارياً مع منع الحذف Restrict
        builder.HasOne(pri => pri.ProductBarCode)
            .WithMany()
            .HasForeignKey(pri => pri.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند بالمستأجر مع منع الإجراء NoAction لتفادي المسارات المتعددة
        builder.HasOne(pri => pri.Tenant)
            .WithMany()
            .HasForeignKey(pri => pri.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس على عمود PurchaseReturnId لتسريع جلب بنود المرتجع
        builder.HasIndex(pri => pri.PurchaseReturnId);
        // إنشاء فهرس على عمود ProductId لتسريع تتبع مرتجعات المنتج
        builder.HasIndex(pri => pri.ProductId);
        // إنشاء فهرس على عمود ProductBarCodeId لتسريع تتبع مرتجعات الباركود
        builder.HasIndex(pri => pri.ProductBarCodeId);
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(pri => new { pri.TenantId, pri.IsDeleted });
    }
}
