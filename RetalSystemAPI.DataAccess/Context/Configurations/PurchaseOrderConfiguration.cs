using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لأوامر وطلبات الشراء (PurchaseOrders).
/// </summary>
public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "PurchaseOrders"
        builder.ToTable("PurchaseOrders");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(po => po.Id);

        // تكوين حقل رقم طلب الشراء كحقل إجباري بحد أقصى 50 حرفاً
        builder.Property(po => po.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        // تعيين تاريخ طلب الشراء كحقل إجباري
        builder.Property(po => po.OrderDate)
            .IsRequired();

        // ضبط نوع إجمالي قيمة أمر الشراء بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(po => po.TotalAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // تعيين حالة أمر الشراء كحقل إجباري
        builder.Property(po => po.Status)
            .IsRequired();

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(po => po.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط أمر الشراء بالمستودع المستهدف اختيارياً مع ضبط القيمة إلى Null عند حذف المستودع
        builder.HasOne(po => po.Warehouse)
            .WithMany(w => w.PurchaseOrders)
            .HasForeignKey(po => po.WarehouseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // ربط أمر الشراء بالفرع مع منع الحذف التلقائي Restrict
        builder.HasOne(po => po.Branch)
            .WithMany(b => b.PurchaseOrders)
            .HasForeignKey(po => po.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط أمر الشراء بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(po => po.Tenant)
            .WithMany()
            .HasForeignKey(po => po.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط أمر الشراء بالمورد اختيارياً مع منع الحذف التلقائي Restrict
        builder.HasOne(po => po.Supplier)
            .WithMany()
            .HasForeignKey(po => po.SupplierId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس مركب فريد على (TenantId, OrderNumber) لضمان عدم تكرار أرقام أوامر الشراء للمستأجر
        builder.HasIndex(po => new { po.TenantId, po.OrderNumber }).IsUnique();
        // إنشاء فهرس على (TenantId, SupplierId) لتسريع فلترة أوامر الشراء حسب المورد
        builder.HasIndex(po => new { po.TenantId, po.SupplierId });
        // إنشاء فهرس على (TenantId, Status) لتسريع استعراض الطلبات حسب حالتها (معلقة، مقبولة..)
        builder.HasIndex(po => new { po.TenantId, po.Status });
        // إنشاء فهرس على (TenantId, BranchId) لتسريع استعراض طلبات شراء كل فرع
        builder.HasIndex(po => new { po.TenantId, po.BranchId });
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(po => new { po.TenantId, po.IsDeleted });
        // إنشاء فهرس على تاريخ الطلب للتقارير الزمنية
        builder.HasIndex(po => po.OrderDate);
    }
}
