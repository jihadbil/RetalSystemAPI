using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.ToTable("SalesReturns");
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.ReturnNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(sr => sr.ReturnDate)
            .IsRequired();

        builder.Property(sr => sr.TotalAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(sr => sr.Reason)
            .IsRequired();

        builder.Property(sr => sr.Notes)
            .HasMaxLength(1000);

        builder.Property(sr => sr.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(sr => sr.OriginalInvoice)
            .WithMany(si => si.Returns)
            .HasForeignKey(sr => sr.OriginalInvoiceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(sr => sr.Branch)
            .WithMany()
            .HasForeignKey(sr => sr.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.Warehouse)
            .WithMany()
            .HasForeignKey(sr => sr.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.Customer)
            .WithMany()
            .HasForeignKey(sr => sr.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sr => sr.Tenant)
            .WithMany()
            .HasForeignKey(sr => sr.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sr => new { sr.TenantId, sr.ReturnNumber }).IsUnique();
        builder.HasIndex(sr => new { sr.TenantId, sr.BranchId });
        builder.HasIndex(sr => new { sr.TenantId, sr.WarehouseId });
        builder.HasIndex(sr => new { sr.TenantId, sr.CustomerId });
        builder.HasIndex(sr => new { sr.TenantId, sr.IsDeleted });
        builder.HasIndex(sr => sr.ReturnDate);
    }
}
