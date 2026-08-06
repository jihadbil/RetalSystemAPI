using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        builder.Property(t => t.Address)
            .HasMaxLength(500);

        builder.Property(t => t.LogoUrl)
            .HasMaxLength(2000);

        builder.Property(t => t.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
