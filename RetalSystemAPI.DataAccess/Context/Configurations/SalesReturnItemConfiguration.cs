using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class SalesReturnItemConfiguration : IEntityTypeConfiguration<SalesReturnItem>
{
    public void Configure(EntityTypeBuilder<SalesReturnItem> builder)
    {
        builder.ToTable("SalesReturnItems");
        builder.HasKey(sri => sri.Id);

        builder.Property(sri => sri.Quantity)
            .IsRequired();

        builder.Property(sri => sri.UnitPrice)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(sri => sri.LineTotal)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        builder.Property(sri => sri.Notes)
            .HasMaxLength(500);

        builder.Property(sri => sri.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(sri => sri.SalesReturn)
            .WithMany(sr => sr.Items)
            .HasForeignKey(sri => sri.SalesReturnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sri => sri.Product)
            .WithMany()
            .HasForeignKey(sri => sri.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sri => sri.ProductBarCode)
            .WithMany()
            .HasForeignKey(sri => sri.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sri => sri.Tenant)
            .WithMany()
            .HasForeignKey(sri => sri.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(sri => sri.SalesReturnId);
        builder.HasIndex(sri => sri.ProductId);
        builder.HasIndex(sri => sri.ProductBarCodeId);
        builder.HasIndex(sri => new { sri.TenantId, sri.IsDeleted });
    }
}
