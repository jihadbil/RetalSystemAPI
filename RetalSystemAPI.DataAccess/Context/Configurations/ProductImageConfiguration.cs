using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لصور الأصناف والباركودات (ProductImages).
/// </summary>
public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        builder.HasKey(pi => pi.Id);

        builder.Property(pi => pi.ImageUrl)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(pi => pi.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(pi => pi.Product)
            .WithMany(p => p.ProductImages)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pi => pi.BarcodeCode)
            .WithMany(pb => pb.ProductImages)
            .HasForeignKey(pi => pi.BarcodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(pi => pi.Tenant)
            .WithMany()
            .HasForeignKey(pi => pi.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(pi => pi.ProductId);
        builder.HasIndex(pi => pi.BarcodeId);
    }
}
