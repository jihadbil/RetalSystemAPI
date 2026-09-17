using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لفواتير مردودات ومسترجعات المبيعات (SalesReturns) والربط بالفاتورة الأصلية.
/// </summary>
public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "SalesReturns"
        builder.ToTable("SalesReturns");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(sr => sr.Id);

        // تكوين حقل رقم مستند المرتجع كحقل إجباري بحد أقصى 50 حرفاً
        builder.Property(sr => sr.ReturnNumber)
            .IsRequired()
            .HasMaxLength(50);

        // تعيين تاريخ المرتجع كحقل إجباري
        builder.Property(sr => sr.ReturnDate)
            .IsRequired();

        // ضبط نوع إجمالي قيمة المرتجع بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(sr => sr.TotalAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // تعيين سبب الإرجاع كحقل إجباري
        builder.Property(sr => sr.Reason)
            .IsRequired();

        // تكوين حقل الملاحظات بحد أقصى 1000 حرف
        builder.Property(sr => sr.Notes)
            .HasMaxLength(1000);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(sr => sr.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط المرتجع بفاتورة المبيعات الأصلية اختيارياً مع تعيين القيمة إلى Null عند حذف الفاتورة الأصلية
        builder.HasOne(sr => sr.OriginalInvoice)
            .WithMany(si => si.Returns)
            .HasForeignKey(sr => sr.OriginalInvoiceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ربط المرتجع بالفرع مع منع الحذف التلقائي Restrict
        builder.HasOne(sr => sr.Branch)
            .WithMany()
            .HasForeignKey(sr => sr.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المرتجع بالمستودع الذي تم إعادة البضاعة إليه مع منع الحذف التلقائي Restrict
        builder.HasOne(sr => sr.Warehouse)
            .WithMany()
            .HasForeignKey(sr => sr.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المرتجع بالعميل اختيارياً مع منع الحذف التلقائي Restrict
        builder.HasOne(sr => sr.Customer)
            .WithMany()
            .HasForeignKey(sr => sr.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المرتجع بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(sr => sr.Tenant)
            .WithMany()
            .HasForeignKey(sr => sr.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس مركب فريد على (TenantId, ReturnNumber) لضمان عدم تكرار أرقام المرتجعات للمستأجر
        builder.HasIndex(sr => new { sr.TenantId, sr.ReturnNumber }).IsUnique();
        // إنشاء فهرس على (TenantId, BranchId) لتسريع فلترة المرتجعات حسب الفرع
        builder.HasIndex(sr => new { sr.TenantId, sr.BranchId });
        // إنشاء فهرس على (TenantId, WarehouseId) لتسريع فلترة المرتجعات حسب المستودع
        builder.HasIndex(sr => new { sr.TenantId, sr.WarehouseId });
        // إنشاء فهرس على (TenantId, CustomerId) لتسريع استعراض مرتجعات العميل
        builder.HasIndex(sr => new { sr.TenantId, sr.CustomerId });
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(sr => new { sr.TenantId, sr.IsDeleted });
        // إنشاء فهرس على تاريخ الإرجاع لتسريع التقارير الزمنية
        builder.HasIndex(sr => sr.ReturnDate);
    }
}
