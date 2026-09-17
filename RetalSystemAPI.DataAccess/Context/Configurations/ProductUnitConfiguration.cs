using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لوحدات قياس الأصناف (ProductUnits) والربط مع الوحدة الأساسية.
/// </summary>
public class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnit>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<ProductUnit> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "ProductUnits"
        builder.ToTable("ProductUnits");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(pu => pu.Id);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(pu => pu.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط وحدة الصنف بالمنتج الرئيسي مع تفعيل الحذف المتتالي Cascade لحذف الوحدات عند حذف المنتج
        builder.HasOne(pu => pu.Product)
            .WithMany(p => p.ProductUnits)
            .HasForeignKey(pu => pu.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط السجل بالوحدة الأساسية (Unit) مع منع الحذف التلقائي Restrict
        builder.HasOne(pu => pu.Unit)
            .WithMany(u => u.ProductUnits)
            .HasForeignKey(pu => pu.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط السجل بالمستأجر مع منع الإجراء NoAction لتفادي مشاكل المسارات المتعددة للحذف
        builder.HasOne(pu => pu.Tenant)
            .WithMany()
            .HasForeignKey(pu => pu.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس مركب فريد على (ProductId, UnitId, TenantId) لمنع تكرار نفس الوحدة للصنف الواحد
        builder.HasIndex(pu => new { pu.ProductId, pu.UnitId, pu.TenantId })
            .IsUnique();
    }
}
