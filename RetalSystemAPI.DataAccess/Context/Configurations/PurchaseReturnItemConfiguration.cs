using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود فواتير مرتجعات المشتريات (PurchaseReturnItems).
/// </summary>
public class PurchaseReturnItemConfiguration : IEntityTypeConfiguration<PurchaseReturnItem>
{
    public void Configure(EntityTypeBuilder<PurchaseReturnItem> builder)
    {
        builder.ToTable("PurchaseReturnItems");
        builder.HasKey(pri => pri.Id);

        builder.Property(pri => pri.Quantity)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(pri => pri.UnitPrice)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(pri => pri.LineTotal)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(pri => pri.Notes)
            .HasMaxLength(500);

        builder.Property(pri => pri.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(pri => pri.PurchaseReturn)
            .WithMany(pr => pr.Items)
            .HasForeignKey(pri => pri.PurchaseReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pri => pri.Product)
            .WithMany()
            .HasForeignKey(pri => pri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pri => pri.ProductBarCode)
            .WithMany()
            .HasForeignKey(pri => pri.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pri => pri.Tenant)
            .WithMany()
            .HasForeignKey(pri => pri.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(pri => pri.PurchaseReturnId);
        builder.HasIndex(pri => pri.ProductId);
        builder.HasIndex(pri => pri.ProductBarCodeId);
        builder.HasIndex(pri => new { pri.TenantId, pri.IsDeleted });
    }
}
