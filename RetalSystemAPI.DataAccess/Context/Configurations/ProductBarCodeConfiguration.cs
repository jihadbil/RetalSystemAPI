using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لباركودات ونكهات الأصناف (ProductBarCodes) وضمان فرادية الباركود لكل مستأجر.
/// </summary>
public class ProductBarCodeConfiguration : IEntityTypeConfiguration<ProductBarCode>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<ProductBarCode> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "ProductBarCodes"
        builder.ToTable("ProductBarCodes");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(pb => pb.Id);

        // تكوين حقل الباركود كحقل إجباري بحد أقصى 100 حرف
        builder.Property(pb => pb.BarCode)
            .IsRequired()
            .HasMaxLength(100);

        // تكوين حقل عنوان أو اسم النكهة/الباركود كحقل إجباري بحد أقصى 300 حرف
        builder.Property(pb => pb.Title)
            .IsRequired()
            .HasMaxLength(300);

        // تكوين حقل الوصف الإضافي للباركود بحد أقصى 1000 حرف
        builder.Property(pb => pb.Description)
            .HasMaxLength(1000);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(pb => pb.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الباركود بالصنف الرئيسي مع تفعيل الحذف المتتالي Cascade لحذف الباركودات عند حذف الصنف
        builder.HasOne(pb => pb.Product)
            .WithMany(p => p.ProductBarCodes)
            .HasForeignKey(pb => pb.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط الباركود بالمستأجر مع منع الإجراء NoAction لتفادي مشاكل الحذف المتعدد
        builder.HasOne(pb => pb.Tenant)
            .WithMany()
            .HasForeignKey(pb => pb.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // فهرس فريد مفلتر: يمنع تكرار الباركود بين الأصناف النشطة فقط، ويسمح بإعادة استخدام باركود أصناف حُذفت منطقياً
        builder.HasIndex(pb => new { pb.TenantId, pb.BarCode })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
