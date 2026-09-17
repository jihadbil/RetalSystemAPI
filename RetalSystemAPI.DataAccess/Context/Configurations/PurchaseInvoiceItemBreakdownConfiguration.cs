using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لجدول تفصيل النكهات والباركودات المستلمة في فواتير المشتريات (PurchaseInvoiceItemBreakdowns).
/// </summary>
public class PurchaseInvoiceItemBreakdownConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItemBreakdown>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItemBreakdown> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "PurchaseInvoiceItemBreakdowns"
        builder.ToTable("PurchaseInvoiceItemBreakdowns");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(b => b.Id);

        // ضبط نوع العمود لكمية العبوات المستلمة بدقة عشرية (18, 2)
        builder.Property(b => b.PackageQuantity)
            .HasPrecision(18, 2);

        // ضبط نوع العمود لإجمالي الكمية بالوحدة الصغرى بدقة عشرية (18, 2)
        builder.Property(b => b.Quantity)
            .HasPrecision(18, 2);

        // ضبط نوع العمود لسعر شراء الوحدة بدقة مالية متقدمة (18, 4)
        builder.Property(b => b.UnitPrice)
            .HasPrecision(18, 4);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(b => b.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ── العلاقات ──────────────────────────────────────────
        // ربط تفصيل النكهة ببند فاتورة الشراء الأب مع تفعيل الحذف المتتالي Cascade
        builder.HasOne(b => b.PurchaseInvoiceItem)
            .WithMany(i => i.Breakdowns)
            .HasForeignKey(b => b.PurchaseInvoiceItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط التفصيل بباركود ونكهة الصنف مع منع الحذف التلقائي Restrict
        builder.HasOne(b => b.ProductBarCode)
            .WithMany()
            .HasForeignKey(b => b.ProductBarCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط السجل بالمستأجر مع منع الإجراء NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── الفهارس ───────────────────────────────────────────
        // إنشاء فهرس مركب على (TenantId, PurchaseInvoiceItemId, ProductBarCodeId) لتسريع استعراض تفاصيل البنود
        builder.HasIndex(b => new { b.TenantId, b.PurchaseInvoiceItemId, b.ProductBarCodeId });
        // إنشاء فهرس على عمود PurchaseInvoiceItemId لربط السجلات بالبند الرئيسي
        builder.HasIndex(b => b.PurchaseInvoiceItemId);
        // إنشاء فهرس على عمود ProductBarCodeId لتتبع استلامات كل باركود
        builder.HasIndex(b => b.ProductBarCodeId);
    }
}
