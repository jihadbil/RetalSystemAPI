using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لجدول تفصيل النكهات والباركودات المستلمة في فواتير المشتريات (PurchaseInvoiceItemBreakdowns).
/// </summary>
public class PurchaseInvoiceItemBreakdownConfiguration : IEntityTypeConfiguration<PurchaseInvoiceItemBreakdown>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceItemBreakdown> builder)
    {
        builder.ToTable("PurchaseInvoiceItemBreakdowns");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.PackageQuantity)
            .HasPrecision(18, 2);

        builder.Property(b => b.Quantity)
            .HasPrecision(18, 2);

        builder.Property(b => b.UnitPrice)
            .HasPrecision(18, 4);

        builder.Property(b => b.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ── العلاقات ──────────────────────────────────────────
        builder.HasOne(b => b.PurchaseInvoiceItem)
            .WithMany(i => i.Breakdowns)
            .HasForeignKey(b => b.PurchaseInvoiceItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(b => b.ProductBarCode)
            .WithMany()
            .HasForeignKey(b => b.ProductBarCodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Tenant)
            .WithMany()
            .HasForeignKey(b => b.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── الفهارس ───────────────────────────────────────────
        builder.HasIndex(b => new { b.TenantId, b.PurchaseInvoiceItemId, b.ProductBarCodeId });
        builder.HasIndex(b => b.PurchaseInvoiceItemId);
        builder.HasIndex(b => b.ProductBarCodeId);
    }
}
