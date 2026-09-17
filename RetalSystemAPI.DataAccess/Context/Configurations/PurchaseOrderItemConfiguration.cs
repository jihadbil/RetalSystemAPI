using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود أوامر الشراء (PurchaseOrderItems).
/// </summary>
public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "PurchaseOrderItems"
        builder.ToTable("PurchaseOrderItems");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(poi => poi.Id);

        // ضبط نوع العمود لكمية البند المطلوبة بدقة مالية decimal(18,4)
        builder.Property(poi => poi.Quantity)
            .HasColumnType("decimal(18,4)");

        // ضبط نوع العمود لسعر شراء الوحدة التقديري بدقة مالية decimal(18,4)
        builder.Property(poi => poi.UnitPrice)
            .HasColumnType("decimal(18,4)");

        // ضبط نوع العمود لإجمالي قيمة السطر بدقة مالية decimal(18,4)
        builder.Property(poi => poi.LineTotal)
            .HasColumnType("decimal(18,4)");

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(poi => poi.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط البند بأمر الشراء الرئيسي مع تفعيل الحذف المتتالي Cascade لحذف البنود عند حذف أمر الشراء
        builder.HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط البند بباركود ونكهة الصنف مع منع الحذف التلقائي Restrict
        builder.HasOne(poi => poi.ProductBarCode)
            .WithMany(pb => pb.PurchaseOrderItems)
            .HasForeignKey(poi => poi.ProductBarCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط البند بالمستأجر مع منع الإجراء NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(poi => poi.Tenant)
            .WithMany()
            .HasForeignKey(poi => poi.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس على عمود PurchaseOrderId لتسريع جلب بنود أمر الشراء
        builder.HasIndex(poi => poi.PurchaseOrderId);
        // إنشاء فهرس على عمود ProductBarCodeId لتسريع تتبع طلبات الشراء لنكهة معينة
        builder.HasIndex(poi => poi.ProductBarCodeId);
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(poi => new { poi.TenantId, poi.IsDeleted });
    }
}
