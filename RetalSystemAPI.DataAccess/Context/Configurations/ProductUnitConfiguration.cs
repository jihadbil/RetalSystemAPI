using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لوحدات قياس الأصناف (ProductUnits) والربط مع الوحدة الأساسية.
/// </summary>
public class ProductUnitConfiguration : IEntityTypeConfiguration<ProductUnit>
{
    public void Configure(EntityTypeBuilder<ProductUnit> builder)
    {
        builder.ToTable("ProductUnits");
        builder.HasKey(pu => pu.Id);

        builder.Property(pu => pu.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(pu => pu.Product)
            .WithMany(p => p.ProductUnits)
            .HasForeignKey(pu => pu.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pu => pu.Unit)
            .WithMany(u => u.ProductUnits)
            .HasForeignKey(pu => pu.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pu => pu.Tenant)
            .WithMany()
            .HasForeignKey(pu => pu.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(pu => new { pu.ProductId, pu.UnitId, pu.TenantId })
            .IsUnique();
    }
}
