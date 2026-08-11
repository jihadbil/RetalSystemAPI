using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class SupplierPhoneConfiguration : IEntityTypeConfiguration<SupplierPhone>
{
    public void Configure(EntityTypeBuilder<SupplierPhone> builder)
    {
        builder.ToTable("SupplierPhones");
        builder.HasKey(sp => sp.Id);

        builder.Property(sp => sp.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(sp => sp.Name)
            .HasMaxLength(100);

        builder.Property(sp => sp.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(sp => sp.Supplier)
            .WithMany(s => s.SupplierPhones)
            .HasForeignKey(sp => sp.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sp => sp.Tenant)
            .WithMany()
            .HasForeignKey(sp => sp.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(sp => new { sp.TenantId, sp.PhoneNumber }).IsUnique();
        builder.HasIndex(sp => sp.SupplierId);
    }
}
