using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.CostPrice)
            .HasColumnType("decimal(18,4)");

        builder.Property(p => p.SalePrice)
            .HasColumnType("decimal(18,4)");

        builder.Property(p => p.AveragePrice)
            .HasColumnType("decimal(18,4)");

        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.TenantId);
        builder.HasIndex(p => new { p.TenantId, p.CategoryId });
        builder.HasIndex(p => new { p.TenantId, p.IsDeleted });
        builder.HasIndex(p => new { p.TenantId, p.IsDeleted, p.Name });
    }
}
