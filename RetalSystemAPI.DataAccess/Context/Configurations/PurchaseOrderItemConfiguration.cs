using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.HasKey(poi => poi.Id);

        builder.Property(poi => poi.Quantity)
            .HasColumnType("decimal(18,4)");

        builder.Property(poi => poi.UnitPrice)
            .HasColumnType("decimal(18,4)");

        builder.Property(poi => poi.LineTotal)
            .HasColumnType("decimal(18,4)");

        builder.Property(poi => poi.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(poi => poi.PurchaseOrder)
            .WithMany(po => po.Items)
            .HasForeignKey(poi => poi.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(poi => poi.ProductBarCode)
            .WithMany(pb => pb.PurchaseOrderItems)
            .HasForeignKey(poi => poi.ProductBarCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(poi => poi.Tenant)
            .WithMany()
            .HasForeignKey(poi => poi.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(poi => poi.PurchaseOrderId);
        builder.HasIndex(poi => poi.ProductBarCodeId);
        builder.HasIndex(poi => new { poi.TenantId, poi.IsDeleted });
    }
}
