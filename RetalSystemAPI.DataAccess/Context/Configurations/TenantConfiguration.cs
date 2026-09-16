using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبيانات المستأجرين (Tenants) وضمان فرادية اسم المستأجر في النظام بالكامل.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");
        builder.HasKey(t => t.Id);
        builder.Ignore(t => t.Tenant);

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

        builder.Property(t => t.CommercialName)
            .HasMaxLength(250);

        builder.Property(t => t.TaxNumber)
            .HasMaxLength(50);

        builder.Property(t => t.CommercialRegistrationNumber)
            .HasMaxLength(50);

        builder.Property(t => t.BusinessType)
            .HasMaxLength(100);

        builder.Property(t => t.AdditionalPhone)
            .HasMaxLength(30);

        builder.Property(t => t.WebsiteUrl)
            .HasMaxLength(250);

        builder.Property(t => t.City)
            .HasMaxLength(100);

        builder.Property(t => t.Country)
            .HasMaxLength(100);

        builder.Property(t => t.PostalCode)
            .HasMaxLength(20);

        builder.Property(t => t.InvoiceHeaderNote)
            .HasMaxLength(1000);

        builder.Property(t => t.InvoiceFooterNote)
            .HasMaxLength(2000);

        builder.Property(t => t.BankDetails)
            .HasMaxLength(2000);

        builder.Property(t => t.DefaultCurrency)
            .HasMaxLength(50);

        builder.Property(t => t.DefaultCurrencySymbol)
            .HasMaxLength(20);

        builder.Property(t => t.DefaultTaxRate)
            .HasPrecision(18, 2);

        builder.Property(t => t.Timezone)
            .HasMaxLength(100);

        builder.Property(t => t.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
