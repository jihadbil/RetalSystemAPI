using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لفواتير مرتجعات المشتريات (PurchaseReturns) والربط بالمورد والفاتورة الأصلية.
/// </summary>
public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "PurchaseReturns"
        builder.ToTable("PurchaseReturns");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(pr => pr.Id);

        // تكوين حقل رقم مرتجع المشتريات كحقل إجباري بحد أقصى 50 حرفاً
        builder.Property(pr => pr.ReturnNumber)
            .IsRequired()
            .HasMaxLength(50);

        // تعيين تاريخ المرتجع كحقل إجباري
        builder.Property(pr => pr.ReturnDate)
            .IsRequired();

        // ضبط نوع إجمالي قيمة مرتجع الشراء بدقة مالية (18, 4) مع قيمة افتراضية صفر
        builder.Property(pr => pr.TotalAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        // تعيين سبب الإرجاع كحقل إجباري
        builder.Property(pr => pr.Reason)
            .IsRequired();

        // تعيين طريقة تسوية المرتجع (نقدي، خصم من الرصيد..) كحقل إجباري
        builder.Property(pr => pr.PaymentMethod)
            .IsRequired();

        // تكوين حقل الملاحظات بحد أقصى 1000 حرف
        builder.Property(pr => pr.Notes)
            .HasMaxLength(1000);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(pr => pr.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط المرتجع بفاتورة الشراء الأصلية اختيارياً مع ضبط القيمة إلى Null عند حذف فاتورة الشراء
        builder.HasOne(pr => pr.PurchaseInvoice)
            .WithMany(pi => pi.Returns)
            .HasForeignKey(pr => pr.PurchaseInvoiceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ربط المرتجع بالمورد مع منع الحذف التلقائي Restrict
        builder.HasOne(pr => pr.Supplier)
            .WithMany()
            .HasForeignKey(pr => pr.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المرتجع بالفرع مع منع الحذف التلقائي Restrict
        builder.HasOne(pr => pr.Branch)
            .WithMany()
            .HasForeignKey(pr => pr.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المرتجع بالمستودع الذي تم إخراج البضاعة منه مع منع الحذف Restrict
        builder.HasOne(pr => pr.Warehouse)
            .WithMany()
            .HasForeignKey(pr => pr.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المرتجع بالمستأجر مع منع الإجراء NoAction لتفادي المسارات المتعددة
        builder.HasOne(pr => pr.Tenant)
            .WithMany()
            .HasForeignKey(pr => pr.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس مركب فريد على (TenantId, ReturnNumber) لمنع تكرار أرقام المرتجعات لنفس المستأجر
        builder.HasIndex(pr => new { pr.TenantId, pr.ReturnNumber }).IsUnique();
        // إنشاء فهرس على (TenantId, BranchId) لتسريع فلترة المرتجعات حسب الفرع
        builder.HasIndex(pr => new { pr.TenantId, pr.BranchId });
        // إنشاء فهرس على (TenantId, WarehouseId) لتسريع فلترة المرتجعات حسب المستودع
        builder.HasIndex(pr => new { pr.TenantId, pr.WarehouseId });
        // إنشاء فهرس على (TenantId, SupplierId) لتسريع استعراض مرتجعات المورد
        builder.HasIndex(pr => new { pr.TenantId, pr.SupplierId });
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(pr => new { pr.TenantId, pr.IsDeleted });
        // إنشاء فهرس على تاريخ المرتجع للتقارير الدورية
        builder.HasIndex(pr => pr.ReturnDate);
    }
}
