using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبنود تسوية الجرد المخزني (StockAdjustmentItems) وفروقات الجرد.
/// </summary>
public class StockAdjustmentItemConfiguration : IEntityTypeConfiguration<StockAdjustmentItem>
{
    public void Configure(EntityTypeBuilder<StockAdjustmentItem> builder)
    {
        builder.ToTable("StockAdjustmentItems");
        builder.HasKey(sai => sai.Id);

        builder.Property(sai => sai.SystemQuantity)
            .IsRequired();

        builder.Property(sai => sai.ActualQuantity)
            .IsRequired();

        builder.Property(sai => sai.DifferenceQuantity)
            .IsRequired();

        builder.Property(sai => sai.UnitCost)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // سبب البند يبدأ من 1 (جرد دوري) — الـ Enum لا يحتوي قيمة 0.
        // القيمة الافتراضية تُضبط في سكربت الـ Migration فقط (EF لا يحوّل int افتراضي إلى Enum)
        builder.Property(sai => sai.Reason)
            .HasConversion<int>();

        builder.Property(sai => sai.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(sai => sai.StockAdjustment)
            .WithMany(sa => sa.Items)
            .HasForeignKey(sai => sai.StockAdjustmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sai => sai.Product)
            .WithMany()
            .HasForeignKey(sai => sai.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sai => sai.ProductBarCode)
            .WithMany()
            .HasForeignKey(sai => sai.ProductBarCodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sai => sai.Tenant)
            .WithMany()
            .HasForeignKey(sai => sai.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(sai => sai.StockAdjustmentId);
        builder.HasIndex(sai => sai.ProductId);
        builder.HasIndex(sai => sai.ProductBarCodeId);
        builder.HasIndex(sai => new { sai.TenantId, sai.IsDeleted });
    }
}
