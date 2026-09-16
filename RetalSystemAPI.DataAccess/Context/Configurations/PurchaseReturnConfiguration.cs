using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لفواتير مرتجعات المشتريات (PurchaseReturns) والربط بالمورد والفاتورة الأصلية.
/// </summary>
public class PurchaseReturnConfiguration : IEntityTypeConfiguration<PurchaseReturn>
{
    public void Configure(EntityTypeBuilder<PurchaseReturn> builder)
    {
        builder.ToTable("PurchaseReturns");
        builder.HasKey(pr => pr.Id);

        builder.Property(pr => pr.ReturnNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(pr => pr.ReturnDate)
            .IsRequired();

        builder.Property(pr => pr.TotalAmount)
            .HasPrecision(18, 4)
            .HasDefaultValue(0);

        builder.Property(pr => pr.Reason)
            .IsRequired();

        builder.Property(pr => pr.PaymentMethod)
            .IsRequired();

        builder.Property(pr => pr.Notes)
            .HasMaxLength(1000);

        builder.Property(pr => pr.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(pr => pr.PurchaseInvoice)
            .WithMany(pi => pi.Returns)
            .HasForeignKey(pr => pr.PurchaseInvoiceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(pr => pr.Supplier)
            .WithMany()
            .HasForeignKey(pr => pr.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Branch)
            .WithMany()
            .HasForeignKey(pr => pr.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Warehouse)
            .WithMany()
            .HasForeignKey(pr => pr.WarehouseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pr => pr.Tenant)
            .WithMany()
            .HasForeignKey(pr => pr.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(pr => new { pr.TenantId, pr.ReturnNumber }).IsUnique();
        builder.HasIndex(pr => new { pr.TenantId, pr.BranchId });
        builder.HasIndex(pr => new { pr.TenantId, pr.WarehouseId });
        builder.HasIndex(pr => new { pr.TenantId, pr.SupplierId });
        builder.HasIndex(pr => new { pr.TenantId, pr.IsDeleted });
        builder.HasIndex(pr => pr.ReturnDate);
    }
}
