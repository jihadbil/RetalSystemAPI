using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لهواتف الفروع (BranchPhones) مع ضبط الحذف المتتالي والفهارس الفريدة.
/// </summary>
public class BranchPhoneConfiguration : IEntityTypeConfiguration<BranchPhone>
{
    public void Configure(EntityTypeBuilder<BranchPhone> builder)
    {
        builder.ToTable("BranchPhones");
        builder.HasKey(bp => bp.Id);

        builder.Property(bp => bp.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(bp => bp.Name)
            .HasMaxLength(100);

        builder.Property(bp => bp.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasOne(bp => bp.Branch)
            .WithMany(b => b.BranchPhones)
            .HasForeignKey(bp => bp.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(bp => bp.Tenant)
            .WithMany()
            .HasForeignKey(bp => bp.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(bp => new { bp.TenantId, bp.PhoneNumber })
            .IsUnique();
    }
}
