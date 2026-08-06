using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

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

        builder.HasIndex(pb => new { pb.TenantId, pb.BarCode })
            .IsUnique();
    }
}
