using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود فواتير المشتريات المجمعة (PurchaseInvoiceItems).
/// </summary>
public class PurchaseInvoiceItemConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItem>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItem> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "PurchaseInvoiceItems"
        builder.ToTable("PurchaseInvoiceItems");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(p => p.Id);

        // ضبط نوع العمود لكمية البند المشتراة بدقة عشرية (18, 2)
        builder.Property(p => p.Quantity)
            .HasPrecision(18, 2);

        // ضبط نوع العمود لسعر شراء الوحدة بدقة عشرية (18, 2)
        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        // ضبط نوع العمود لقيمة الخصم الممنوح على البند بدقة عشرية (18, 2)
        builder.Property(p => p.DiscountAmount)
            .HasPrecision(18, 2);

        // ضبط نوع العمود لإجمالي قيمة السطر بدقة عشرية (18, 2)
        builder.Property(p => p.LineTotal)
            .HasPrecision(18, 2);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ── العلاقات ──────────────────────────────────────────
        // ربط بند الشراء بالمنتج الأساسي مع منع الحذف التلقائي Restrict
        builder.HasOne(p => p.Product)
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط بند الشراء بالباركود أو النكهة اختيارياً مع منع الحذف Restrict
        builder.HasOne(p => p.ProductBarCode)
            .WithMany()
            .HasForeignKey(p => p.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند بالمستأجر مع منع الإجراء NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── الفهارس ───────────────────────────────────────────
        // إنشاء فهرس على عمود PurchaseInvoiceId لتسريع جلب بنود الفاتورة
        builder.HasIndex(p => p.PurchaseInvoiceId);
        // إنشاء فهرس على عمود ProductId لتسريع تتبع مشتريات الصنف
        builder.HasIndex(p => p.ProductId);
        // إنشاء فهرس على عمود ProductBarCodeId لتسريع تتبع مشتريات النكهة
        builder.HasIndex(p => p.ProductBarCodeId);
    }
}
