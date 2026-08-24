using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class StorgeStockConfiguration : IEntityTypeConfiguration<StorgeStock>
{
    public void Configure(EntityTypeBuilder<StorgeStock> builder)
    {
        builder.ToTable("StorgeStocks");
        builder.HasKey(ss => ss.Id);

        builder.Property(ss => ss.Quantity)
            .HasDefaultValue(0);

        builder.Property(ss => ss.MinStockLevel)
            .HasDefaultValue(0);

        builder.Property(ss => ss.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(ss => ss.Warehouse)
            .WithMany(w => w.WarehouseStocks)
            .HasForeignKey(ss => ss.WarehouseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ss => ss.ProductBarcode)
            .WithMany(pb => pb.StorgeStocks)
            .HasForeignKey(ss => ss.ProductBarcodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ss => ss.Tenant)
            .WithMany()
            .HasForeignKey(ss => ss.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(ss => new { ss.WarehouseId, ss.ProductBarcodeId })
            .IsUnique();

        builder.HasIndex(ss => ss.TenantId);
        builder.HasIndex(ss => ss.WarehouseId);
        builder.HasIndex(ss => new { ss.TenantId, ss.IsDeleted });
    }
}
