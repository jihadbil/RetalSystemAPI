using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لصور الأصناف والباركودات (ProductImages).
/// </summary>
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "ProductImages"
        builder.ToTable("ProductImages");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(pi => pi.Id);

        // تكوين حقل مسار أو رابط الصورة كحقل إجباري بحد أقصى 1000 حرف
        builder.Property(pi => pi.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(pi => pi.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الصورة بالمنتج مع تفعيل الحذف المتتالي Cascade لحذف الصور عند مسح المنتج
        builder.HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط الصورة اختيارياً بنكهة أو باركود محدد مع سلوك NoAction
        builder.HasOne(pi => pi.BarcodeCode)
            .WithMany(pb => pb.ProductImages)
            .HasForeignKey(pi => pi.BarcodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        // ربط الصورة بالمستأجر مع منع الإجراء NoAction لتفادي مشاكل المسارات المتعددة للحذف
        builder.HasOne(pi => pi.Tenant)
            .WithMany()
            .HasForeignKey(pi => pi.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس على عمود ProductId لتسريع استعراض صور المنتج
        builder.HasIndex(pi => pi.ProductId);
        // إنشاء فهرس على عمود BarcodeId لتسريع استعراض الصور الخاصة بنكهة معينة
        builder.HasIndex(pi => pi.BarcodeId);
    }
}
