using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("StockTransfers");
        builder.HasKey(st => st.Id);

        builder.Property(st => st.TransferNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(st => st.TransferDate)
            .IsRequired();

        builder.Property(st => st.Status)
            .IsRequired();

        builder.Property(st => st.Notes)
            .HasMaxLength(1000);

        builder.Property(st => st.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(st => st.FromWarehouse)
            .WithMany()
            .HasForeignKey(st => st.FromWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.ToWarehouse)
            .WithMany()
            .HasForeignKey(st => st.ToWarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.Tenant)
            .WithMany()
            .HasForeignKey(st => st.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(st => new { st.TenantId, st.TransferNumber }).IsUnique();
        builder.HasIndex(st => new { st.TenantId, st.FromWarehouseId });
        builder.HasIndex(st => new { st.TenantId, st.ToWarehouseId });
        builder.HasIndex(st => new { st.TenantId, st.Status });
        builder.HasIndex(st => new { st.TenantId, st.IsDeleted });
        builder.HasIndex(st => st.TransferDate);
    }
}
