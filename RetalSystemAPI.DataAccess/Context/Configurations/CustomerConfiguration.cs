using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Code)
            .HasMaxLength(50);

        builder.Property(c => c.Email)
            .HasMaxLength(150);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.OpeningBalance)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(c => c.CreditLimit)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(c => c.IsActive)
            .HasDefaultValue(true);

        builder.Property(c => c.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(c => c.Tenant)
            .WithMany()
            .HasForeignKey(c => c.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => new { c.TenantId, c.Code });
        builder.HasIndex(c => new { c.TenantId, c.IsDeleted });
        builder.HasIndex(c => new { c.TenantId, c.IsDeleted, c.Name });
    }
}
