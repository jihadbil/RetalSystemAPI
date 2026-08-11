using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(w => w.Type)
            .IsRequired();

        builder.Property(w => w.IsActive)
            .HasDefaultValue(true);

        builder.Property(w => w.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(w => w.Branch)
            .WithMany(b => b.Warehouses)
            .HasForeignKey(w => w.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(w => w.Tenant)
            .WithMany()
            .HasForeignKey(w => w.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => w.TenantId);
        builder.HasIndex(w => new { w.TenantId, w.BranchId });
        builder.HasIndex(w => new { w.TenantId, w.IsActive });
        builder.HasIndex(w => new { w.TenantId, w.IsDeleted });
    }
}
