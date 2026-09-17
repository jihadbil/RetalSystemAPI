using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لفواتير المشتريات (PurchaseInvoices) وتحديد الدقة المالية والفهارس الفريدة.
/// </summary>
public class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "PurchaseInvoices"
        builder.ToTable("PurchaseInvoices");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(p => p.Id);

        // تكوين حقل رقم فاتورة الشراء كحقل إجباري بحد أقصى 50 حرفاً
        builder.Property(p => p.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        // ضبط نوع العمود للإجمالي الفرعي بدقة عشرية (18, 2)
        builder.Property(p => p.SubTotal)
            .HasPrecision(18, 2);

        // ضبط نوع العمود لقيمة الخصم بدقة عشرية (18, 2)
        builder.Property(p => p.DiscountAmount)
            .HasPrecision(18, 2);

        // ضبط نوع العمود لمبلغ الضريبة بدقة عشرية (18, 2)
        builder.Property(p => p.TaxAmount)
            .HasPrecision(18, 2);

        // ضبط نوع العمود للإجمالي النهائي لفاتورة الشراء بدقة عشرية (18, 2)
        builder.Property(p => p.TotalAmount)
            .HasPrecision(18, 2);

        // ضبط نوع العمود للمبلغ المدفوع للمورد بدقة عشرية (18, 2)
        builder.Property(p => p.PaidAmount)
            .HasPrecision(18, 2);

        // ضبط نوع العمود للمبلغ المتبقي لصالح المورد بدقة عشرية (18, 2)
        builder.Property(p => p.RemainingAmount)
            .HasPrecision(18, 2);

        // تكوين حقل الملاحظات بحد أقصى 500 حرف
        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ── العلاقات ──────────────────────────────────────────
        // ربط فاتورة الشراء بالمورد مع منع الحذف التلقائي Restrict
        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط فاتورة الشراء بالفرع مع منع الحذف التلقائي Restrict
        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط فاتورة الشراء بالمستودع الذي تم استلام البضاعة فيه مع منع الحذف Restrict
        builder.HasOne(p => p.Warehouse)
            .WithMany()
            .HasForeignKey(p => p.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط فاتورة الشراء بأمر الشراء اختيارياً مع تعيين القيمة إلى Null عند حذف أمر الشراء
        builder.HasOne(p => p.PurchaseOrder)
            .WithMany()
            .HasForeignKey(p => p.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        // ربط فاتورة الشراء ببنودها مع تفعيل الحذف المتتالي Cascade لحذف البنود عند حذف الفاتورة
        builder.HasMany(p => p.Items)
            .WithOne(i => i.PurchaseInvoice)
            .HasForeignKey(i => i.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط الفاتورة بالمستأجر مع منع الإجراء NoAction لتفادي المسارات المتعددة
        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── الفهارس ───────────────────────────────────────────
        // إنشاء فهرس مركب فريد على (TenantId, InvoiceNumber) لمنع تكرار رقم فاتورة الشراء لنفس المستأجر
        builder.HasIndex(p => new { p.TenantId, p.InvoiceNumber })
            .IsUnique();

        // إنشاء فهرس على عمود SupplierId لتسريع كشف حساب المورد
        builder.HasIndex(p => p.SupplierId);
        // إنشاء فهرس على عمود BranchId لتسريع استعراض فواتير مشتريات الفرع
        builder.HasIndex(p => p.BranchId);
        // إنشاء فهرس على عمود WarehouseId لتسريع تتبع مشتريات المستودع
        builder.HasIndex(p => p.WarehouseId);
        // إنشاء فهرس على تاريخ الفاتورة للتقارير الدورية
        builder.HasIndex(p => p.InvoiceDate);
    }
}
