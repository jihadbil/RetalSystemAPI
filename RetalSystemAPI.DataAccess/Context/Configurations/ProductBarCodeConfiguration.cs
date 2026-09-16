using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لباركودات ونكهات الأصناف (ProductBarCodes) وضمان فرادية الباركود لكل مستأجر.
/// </summary>
public class ProductBarCodeConfiguration : IEntityTypeConfiguration<ProductBarCode>
{
    public void Configure(EntityTypeBuilder<ProductBarCode> builder)
    {
        builder.ToTable("ProductBarCodes");
        builder.HasKey(pb => pb.Id);

        builder.Property(pb => pb.BarCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(pb => pb.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(pb => pb.Description)
            .HasMaxLength(1000);

        builder.Property(pb => pb.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(pb => pb.Product)
            .WithMany(p => p.ProductBarCodes)
            .HasForeignKey(pb => pb.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pb => pb.Tenant)
            .WithMany()
            .HasForeignKey(pb => pb.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // فهرس فريد مفلتر: يمنع تكرار الباركود بين الأصناف النشطة فقط،
        // ويسمح بإعادة استخدام باركود أصناف حُذفت حذفاً منطقياً (Soft Delete)
        builder.HasIndex(pb => new { pb.TenantId, pb.BarCode })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
