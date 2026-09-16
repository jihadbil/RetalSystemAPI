using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود أوامر التحويل المخزني (StockTransferItems).
/// </summary>
public class StockTransferItemConfiguration : IEntityTypeConfiguration<StockTransferItem>
{
    public void Configure(EntityTypeBuilder<StockTransferItem> builder)
    {
        builder.ToTable("StockTransferItems");
        builder.HasKey(sti => sti.Id);

        builder.Property(sti => sti.Quantity)
            .IsRequired();

        builder.Property(sti => sti.Notes)
            .HasMaxLength(500);

        builder.Property(sti => sti.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(sti => sti.StockTransfer)
            .WithMany(st => st.Items)
            .HasForeignKey(sti => sti.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sti => sti.Product)
            .WithMany()
            .HasForeignKey(sti => sti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sti => sti.ProductBarCode)
            .WithMany()
            .HasForeignKey(sti => sti.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sti => sti.Tenant)
            .WithMany()
            .HasForeignKey(sti => sti.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(sti => sti.StockTransferId);
        builder.HasIndex(sti => sti.ProductId);
        builder.HasIndex(sti => sti.ProductBarCodeId);
        builder.HasIndex(sti => new { sti.TenantId, sti.IsDeleted });
    }
}
