using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لأوامر وطلبات الشراء (PurchaseOrders).
/// </summary>
public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(po => po.Id);

        builder.Property(po => po.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(po => po.OrderDate)
            .IsRequired();

        builder.Property(po => po.TotalAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(po => po.Status)
            .IsRequired();

        builder.Property(po => po.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(po => po.Warehouse)
            .WithMany(w => w.PurchaseOrders)
            .HasForeignKey(po => po.WarehouseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(po => po.Branch)
            .WithMany(b => b.PurchaseOrders)
            .HasForeignKey(po => po.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Tenant)
            .WithMany()
            .HasForeignKey(po => po.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(po => po.Supplier)
            .WithMany()
            .HasForeignKey(po => po.SupplierId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(po => new { po.TenantId, po.OrderNumber }).IsUnique();
        builder.HasIndex(po => new { po.TenantId, po.SupplierId });
        builder.HasIndex(po => new { po.TenantId, po.Status });
        builder.HasIndex(po => new { po.TenantId, po.BranchId });
        builder.HasIndex(po => new { po.TenantId, po.IsDeleted });
        builder.HasIndex(po => po.OrderDate);
    }
}
