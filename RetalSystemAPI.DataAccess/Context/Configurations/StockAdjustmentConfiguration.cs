using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class StockAdjustmentConfiguration : IEntityTypeConfiguration<StockAdjustment>
{
    public void Configure(EntityTypeBuilder<StockAdjustment> builder)
    {
        builder.ToTable("StockAdjustments");
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.AdjustmentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sa => sa.AdjustmentDate)
            .IsRequired();

        builder.Property(sa => sa.Reason)
            .IsRequired();

        builder.Property(sa => sa.Notes)
            .HasMaxLength(1000);

        builder.Property(sa => sa.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(sa => sa.Warehouse)
            .WithMany()
            .HasForeignKey(sa => sa.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sa => sa.Tenant)
            .WithMany()
            .HasForeignKey(sa => sa.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sa => new { sa.TenantId, sa.AdjustmentNumber }).IsUnique();
        builder.HasIndex(sa => new { sa.TenantId, sa.WarehouseId });
        builder.HasIndex(sa => new { sa.TenantId, sa.Reason });
        builder.HasIndex(sa => new { sa.TenantId, sa.IsDeleted });
        builder.HasIndex(sa => sa.AdjustmentDate);
    }
}
