using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لأوامر التحويل المخزني بين المستودعات والصالات (StockTransfers).
/// </summary>
public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "StockTransfers"
        builder.ToTable("StockTransfers");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(st => st.Id);

        // تكوين حقل رقم أمر التحويل كحقل إجباري بحد أقصى 50 حرفاً
        builder.Property(st => st.TransferNumber)
            .IsRequired()
            .HasMaxLength(50);

        // تعيين تاريخ تنفيذ أمر التحويل كحقل إجباري
        builder.Property(st => st.TransferDate)
            .IsRequired();

        // تعيين حالة التحويل المخزني (مسودة، بانتظار الموافقة، مكتمل..) كحقل إجباري
        builder.Property(st => st.Status)
            .IsRequired();

        // تكوين حقل الملاحظات بحد أقصى 1000 حرف
        builder.Property(st => st.Notes)
            .HasMaxLength(1000);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(st => st.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط أمر التحويل بالمستودع المصدر مع منع الحذف التلقائي Restrict
        builder.HasOne(st => st.FromWarehouse)
            .WithMany()
            .HasForeignKey(st => st.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط أمر التحويل بالمستودع الوجهة مع منع الحذف التلقائي Restrict
        builder.HasOne(st => st.ToWarehouse)
            .WithMany()
            .HasForeignKey(st => st.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط أمر التحويل بالمستأجر التابع له مع منع الحذف التلقائي Restrict
        builder.HasOne(st => st.Tenant)
            .WithMany()
            .HasForeignKey(st => st.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // إنشاء فهرس مركب فريد على (TenantId, TransferNumber) لمنع تكرار أرقام التحويل لنفس المستأجر
        builder.HasIndex(st => new { st.TenantId, st.TransferNumber }).IsUnique();
        // إنشاء فهرس على (TenantId, FromWarehouseId) لتسريع فلترة التحويلات الصادرة من المستودع
        builder.HasIndex(st => new { st.TenantId, st.FromWarehouseId });
        // إنشاء فهرس على (TenantId, ToWarehouseId) لتسريع فلترة التحويلات الواردة للمستودع
        builder.HasIndex(st => new { st.TenantId, st.ToWarehouseId });
        // إنشاء فهرس على (TenantId, Status) لتسريع فلترة التحويلات حسب حالتها
        builder.HasIndex(st => new { st.TenantId, st.Status });
        // إنشاء فهرس على (TenantId, IsDeleted) لتحسين أداء مرشحات الحذف الناعم
        builder.HasIndex(st => new { st.TenantId, st.IsDeleted });
        // إنشاء فهرس على تاريخ التحويل لتسريع التقارير الزمنية
        builder.HasIndex(st => st.TransferDate);
    }
}
