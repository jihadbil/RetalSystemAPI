using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class PurchaseInvoiceConfiguration : IEntityTypeConfiguration<PurchaseInvoice>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoice> builder)
    {
        builder.ToTable("PurchaseInvoices");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.SubTotal)
            .HasPrecision(18, 2);

        builder.Property(p => p.DiscountAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.TaxAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.PaidAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.RemainingAmount)
            .HasPrecision(18, 2);

        builder.Property(p => p.Notes)
            .HasMaxLength(500);

        builder.Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ── العلاقات ──────────────────────────────────────────
        builder.HasOne(p => p.Supplier)
            .WithMany()
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Warehouse)
            .WithMany()
            .HasForeignKey(p => p.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.PurchaseOrder)
            .WithMany()
            .HasForeignKey(p => p.PurchaseOrderId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Items)
            .WithOne(i => i.PurchaseInvoice)
            .HasForeignKey(i => i.PurchaseInvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Tenant)
            .WithMany()
            .HasForeignKey(p => p.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // ── الفهارس ───────────────────────────────────────────
        builder.HasIndex(p => new { p.TenantId, p.InvoiceNumber })
            .IsUnique();

        builder.HasIndex(p => p.SupplierId);
        builder.HasIndex(p => p.BranchId);
        builder.HasIndex(p => p.WarehouseId);
        builder.HasIndex(p => p.InvoiceDate);
    }
}
