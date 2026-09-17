using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود فواتير مردودات المبيعات (SalesReturnItems).
/// </summary>
public class SalesReturnItemConfiguration : IEntityTypeConfiguration<SalesReturnItem>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<SalesReturnItem> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "SalesReturnItems"
        builder.ToTable("SalesReturnItems");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(sri => sri.Id);

        // تكوين حقل كمية البند المرتجع كحقل إجباري
        builder.Property(sri => sri.Quantity)
            .IsRequired();

        // ضبط نوع سعر إرجاع الوحدة بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(sri => sri.UnitPrice)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع إجمالي قيمة السطر المرتجع بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(sri => sri.LineTotal)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // تكوين حقل ملاحظات سطر الإرجاع بحد أقصى 500 حرف
        builder.Property(sri => sri.Notes)
            .HasMaxLength(500);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(sri => sri.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط السطر بفاتورة مرتجع المبيعات الرئيسية مع تفعيل الحذف المتتالي Cascade لحذف البنود عند حذف فاتورة المرتجع
        builder.HasOne(sri => sri.SalesReturn)
            .WithMany(sr => sr.Items)
            .HasForeignKey(sri => sri.SalesReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط البند المرتجع بالمنتج مع منع الحذف التلقائي Restrict
        builder.HasOne(sri => sri.Product)
            .WithMany()
            .HasForeignKey(sri => sri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند المرتجع بالباركود أو النكهة اختيارياً مع منع الحذف Restrict
        builder.HasOne(sri => sri.ProductBarCode)
            .WithMany()
            .HasForeignKey(sri => sri.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند بالمستأجر مع منع الإجراء NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(sri => sri.Tenant)
            .WithMany()
            .HasForeignKey(sri => sri.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس على عمود SalesReturnId لتسريع استعراض بنود المرتجع
        builder.HasIndex(sri => sri.SalesReturnId);
        // إنشاء فهرس على عمود ProductId لتسريع تقارير مرتجعات المنتجات
        builder.HasIndex(sri => sri.ProductId);
        // إنشاء فهرس على عمود ProductBarCodeId لتسريع تقارير مرتجعات النكهات والباركودات
        builder.HasIndex(sri => sri.ProductBarCodeId);
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(sri => new { sri.TenantId, sri.IsDeleted });
    }
}
