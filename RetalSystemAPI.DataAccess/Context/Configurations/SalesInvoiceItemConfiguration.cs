using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود فواتير المبيعات (SalesInvoiceItems) وتكاليف وهوامش ربح المبيعات.
/// </summary>
public class SalesInvoiceItemConfiguration : IEntityTypeConfiguration<SalesInvoiceItem>
{
    public void Configure(EntityTypeBuilder<SalesInvoiceItem> builder)
    {
        builder.ToTable("SalesInvoiceItems");
        builder.HasKey(sii => sii.Id);

        builder.Property(sii => sii.Quantity)
            .IsRequired();

        builder.Property(sii => sii.UnitPrice)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(sii => sii.UnitCost)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(sii => sii.DiscountAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(sii => sii.LineTotal)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(sii => sii.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(sii => sii.SalesInvoice)
            .WithMany(si => si.Items)
            .HasForeignKey(sii => sii.SalesInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sii => sii.Product)
            .WithMany()
            .HasForeignKey(sii => sii.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sii => sii.ProductBarCode)
            .WithMany()
            .HasForeignKey(sii => sii.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sii => sii.Tenant)
            .WithMany()
            .HasForeignKey(sii => sii.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(sii => sii.SalesInvoiceId);
        builder.HasIndex(sii => sii.ProductId);
        builder.HasIndex(sii => sii.ProductBarCodeId);
        builder.HasIndex(sii => new { sii.TenantId, sii.IsDeleted });
    }
}
