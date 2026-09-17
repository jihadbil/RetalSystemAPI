using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لفواتير المبيعات (SalesInvoices) وطرق الدفع والارتباط بالعميل والمخزن.
/// </summary>
public class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "SalesInvoices"
        builder.ToTable("SalesInvoices");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(si => si.Id);

        // تكوين حقل رقم الفاتورة كحقل إجباري بحد أقصى 50 حرفاً
        builder.Property(si => si.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        // تعيين تاريخ الفاتورة كحقل إجباري
        builder.Property(si => si.InvoiceDate)
            .IsRequired();

        // تعيين حالة الفاتورة (قيد الانتظار، مدفوعة، ملغاة..) كحقل إجباري
        builder.Property(si => si.Status)
            .IsRequired();

        // تعيين طريقة الدفع (نقدي، بطاقة، آجل..) كحقل إجباري
        builder.Property(si => si.PaymentMethod)
            .IsRequired();

        // ضبط نوع الإجمالي الفرعي بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(si => si.SubTotal)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع قيمة الخصم بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(si => si.DiscountAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع الإجمالي الكلي للفاتورة بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(si => si.TotalAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع المبلغ المدفوع بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(si => si.PaidAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // ضبط نوع المبلغ المتبقي بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(si => si.RemainingAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // تكوين حقل الملاحظات بحد أقصى 1000 حرف
        builder.Property(si => si.Notes)
            .HasMaxLength(1000);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(si => si.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الفاتورة بالفرع مع منع الحذف التلقائي Restrict
        builder.HasOne(si => si.Branch)
            .WithMany()
            .HasForeignKey(si => si.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الفاتورة بالمستودع الذي تم الصرف منه مع منع الحذف التلقائي Restrict
        builder.HasOne(si => si.Warehouse)
            .WithMany()
            .HasForeignKey(si => si.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الفاتورة بالعميل اختيارياً (IsRequired = false للبيع النقدي العام) مع منع الحذف Restrict
        builder.HasOne(si => si.Customer)
            .WithMany(c => c.SalesInvoices)
            .HasForeignKey(si => si.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الفاتورة بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(si => si.Tenant)
            .WithMany()
            .HasForeignKey(si => si.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس مركب فريد على (TenantId, InvoiceNumber) لضمان عدم تكرار أرقام الفواتير لنفس المستأجر
        builder.HasIndex(si => new { si.TenantId, si.InvoiceNumber }).IsUnique();
        // إنشاء فهرس على (TenantId, BranchId) لتسريع فلترة فواتير كل فرع
        builder.HasIndex(si => new { si.TenantId, si.BranchId });
        // إنشاء فهرس على (TenantId, WarehouseId) لتسريع فلترة الفواتير حسب المستودع
        builder.HasIndex(si => new { si.TenantId, si.WarehouseId });
        // إنشاء فهرس على (TenantId, CustomerId) لتسريع استعراض كشف حساب وفواتير العميل
        builder.HasIndex(si => new { si.TenantId, si.CustomerId });
        // إنشاء فهرس على (TenantId, Status) لتسريع فلترة الفواتير حسب حالتها التشغيلية
        builder.HasIndex(si => new { si.TenantId, si.Status });
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء استعلامات الحذف الناعم
        builder.HasIndex(si => new { si.TenantId, si.IsDeleted });
        // إنشاء فهرس على تاريخ الفاتورة لتسريع التقارير المالية واليومية
        builder.HasIndex(si => si.InvoiceDate);
    }
}
