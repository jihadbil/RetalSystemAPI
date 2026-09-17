using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لعمليات تسوية الجرد المخزني (StockAdjustments).
/// </summary>
public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "StockAdjustments"
        builder.ToTable("StockAdjustments");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(sa => sa.Id);

        // تكوين حقل رقم مستند التسوية كحقل إجباري بحد أقصى 50 حرفاً
        builder.Property(sa => sa.AdjustmentNumber)
            .IsRequired()
            .HasMaxLength(50);

        // تعيين تاريخ التسوية المخزنية كحقل إجباري
        builder.Property(sa => sa.AdjustmentDate)
            .IsRequired();

        // تعيين سبب التسوية (عجز، فائض، تلف..) كحقل إجباري
        builder.Property(sa => sa.Reason)
            .IsRequired();

        // تكوين حقل الملاحظات بحد أقصى 1000 حرف
        builder.Property(sa => sa.Notes)
            .HasMaxLength(1000);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(sa => sa.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط تسوية الجرد بالمستودع الذي تمت فيه التسوية مع منع الحذف التلقائي Restrict
        builder.HasOne(sa => sa.Warehouse)
            .WithMany()
            .HasForeignKey(sa => sa.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط التسوية بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(sa => sa.Tenant)
            .WithMany()
            .HasForeignKey(sa => sa.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس مركب فريد على (TenantId, AdjustmentNumber) لمنع تكرار أرقام التسوية لنفس المستأجر
        builder.HasIndex(sa => new { sa.TenantId, sa.AdjustmentNumber }).IsUnique();
        // إنشاء فهرس على (TenantId, WarehouseId) لتسريع فلترة التسويات حسب المستودع
        builder.HasIndex(sa => new { sa.TenantId, sa.WarehouseId });
        // إنشاء فهرس على (TenantId, Reason) لتسريع تحليل أسباب الفروقات الجردية
        builder.HasIndex(sa => new { sa.TenantId, sa.Reason });
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(sa => new { sa.TenantId, sa.IsDeleted });
        // إنشاء فهرس على تاريخ التسوية للتقارير الجردية الدورية
        builder.HasIndex(sa => sa.AdjustmentDate);
    }
}
