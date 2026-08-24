using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class SalesInvoiceConfiguration : IEntityTypeConfiguration<SalesInvoice>
{
    public void Configure(EntityTypeBuilder<SalesInvoice> builder)
    {
        builder.ToTable("SalesInvoices");
        builder.HasKey(si => si.Id);

        builder.Property(si => si.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(si => si.InvoiceDate)
            .IsRequired();

        builder.Property(si => si.Status)
            .IsRequired();

        builder.Property(si => si.PaymentMethod)
            .IsRequired();

        builder.Property(si => si.SubTotal)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(si => si.DiscountAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(si => si.TotalAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(si => si.PaidAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(si => si.RemainingAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(si => si.Notes)
            .HasMaxLength(1000);

        builder.Property(si => si.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(si => si.Branch)
            .WithMany()
            .HasForeignKey(si => si.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.Warehouse)
            .WithMany()
            .HasForeignKey(si => si.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.Customer)
            .WithMany(c => c.SalesInvoices)
            .HasForeignKey(si => si.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(si => si.Tenant)
            .WithMany()
            .HasForeignKey(si => si.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(si => new { si.TenantId, si.InvoiceNumber }).IsUnique();
        builder.HasIndex(si => new { si.TenantId, si.BranchId });
        builder.HasIndex(si => new { si.TenantId, si.WarehouseId });
        builder.HasIndex(si => new { si.TenantId, si.CustomerId });
        builder.HasIndex(si => new { si.TenantId, si.Status });
        builder.HasIndex(si => new { si.TenantId, si.IsDeleted });
        builder.HasIndex(si => si.InvoiceDate);
    }
}
