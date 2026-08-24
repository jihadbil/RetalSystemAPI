using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class CustomerPhoneConfiguration : IEntityTypeConfiguration<CustomerPhone>
{
    public void Configure(EntityTypeBuilder<CustomerPhone> builder)
    {
        builder.ToTable("CustomerPhones");
        builder.HasKey(cp => cp.Id);

        builder.Property(cp => cp.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(cp => cp.ContactName)
            .HasMaxLength(100);

        builder.Property(cp => cp.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(cp => cp.Customer)
            .WithMany(c => c.CustomerPhones)
            .HasForeignKey(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cp => cp.Tenant)
            .WithMany()
            .HasForeignKey(cp => cp.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(cp => new { cp.TenantId, cp.PhoneNumber });
        builder.HasIndex(cp => cp.CustomerId);
    }
}
