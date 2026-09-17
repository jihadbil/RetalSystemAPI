using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود فواتير المبيعات (SalesInvoiceItems) وتكاليف وهوامش ربح المبيعات.
/// </summary>
public class SalesInvoiceItemConfiguration : IEntityTypeConfiguration<SalesInvoiceItem>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<SalesInvoiceItem> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "SalesInvoiceItems"
        builder.ToTable("SalesInvoiceItems");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(sii => sii.Id);

        // تكوين حقل كمية البند المباعة كحقل إجباري
        builder.Property(sii => sii.Quantity)
            .IsRequired();

        // ضبط نوع سعر بيع الوحدة بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(sii => sii.UnitPrice)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع تكلفة الوحدة وقت البيع بدقة مالية decimal(18,4) لحساب هامش الربح
        builder.Property(sii => sii.UnitCost)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع قيمة الخصم الممنوح على هذا السطر بدقة مالية decimal(18,4)
        builder.Property(sii => sii.DiscountAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع إجمالي قيمة السطر (بعد الخصم) بدقة مالية decimal(18,4)
        builder.Property(sii => sii.LineTotal)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(sii => sii.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط البند بفاتورة المبيعات الرئيسية مع تفعيل الحذف المتتالي Cascade لحذف البنود عند حذف الفاتورة
        builder.HasOne(sii => sii.SalesInvoice)
            .WithMany(si => si.Items)
            .HasForeignKey(sii => sii.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط البند بالمنتج المباع مع منع الحذف التلقائي Restrict
        builder.HasOne(sii => sii.Product)
            .WithMany()
            .HasForeignKey(sii => sii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند بالباركود أو النكهة المحددة اختيارياً مع منع الحذف Restrict
        builder.HasOne(sii => sii.ProductBarCode)
            .WithMany()
            .HasForeignKey(sii => sii.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند بالمستأجر مع سلوك NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(sii => sii.Tenant)
            .WithMany()
            .HasForeignKey(sii => sii.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس على عمود SalesInvoiceId لتسريع جلب بنود الفاتورة
        builder.HasIndex(sii => sii.SalesInvoiceId);
        // إنشاء فهرس على عمود ProductId لتسريع تقارير مبيعات الأصناف
        builder.HasIndex(sii => sii.ProductId);
        // إنشاء فهرس على عمود ProductBarCodeId لتسريع تقارير مبيعات النكهات والباركودات
        builder.HasIndex(sii => sii.ProductBarCodeId);
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء الفلترة حسب المستأجر والحذف الناعم
        builder.HasIndex(sii => new { sii.TenantId, sii.IsDeleted });
    }
}
