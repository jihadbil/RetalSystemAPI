using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Accounting;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class CashSessionConfiguration : IEntityTypeConfiguration<CashSession>
{
    public void Configure(EntityTypeBuilder<CashSession> builder)
    {
        builder.ToTable("CashSessions", table =>
        {
            table.HasCheckConstraint("CK_CashSessions_Status", "[Status] IN (1,2,3,4)");
            table.HasCheckConstraint("CK_CashSessions_Amounts", "[OpeningFloat] >= 0 AND [ExpectedClosingBalance] >= 0 AND ([CountedClosingBalance] IS NULL OR [CountedClosingBalance] >= 0)");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CashierUserId).IsRequired().HasMaxLength(450);
        builder.Property(x => x.ApprovedByUserId).HasMaxLength(450);
        builder.Property(x => x.OpeningFloat).HasColumnType("decimal(18,4)");
        builder.Property(x => x.ExpectedClosingBalance).HasColumnType("decimal(18,4)");
        builder.Property(x => x.CountedClosingBalance).HasColumnType("decimal(18,4)");
        builder.Property(x => x.Difference).HasColumnType("decimal(18,4)");
        builder.Property(x => x.ClosingNotes).HasMaxLength(500);
        builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        builder.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.PosTerminal).WithMany(x => x.CashSessions).HasForeignKey(x => x.PosTerminalId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.CashDrawer).WithMany(x => x.CashSessions).HasForeignKey(x => x.CashDrawerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => x.CashDrawerId).IsUnique().HasFilter("[Status] IN (1,2) AND [IsDeleted] = 0");
        builder.HasIndex(x => x.CashierUserId).IsUnique().HasFilter("[Status] IN (1,2) AND [IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.OpenedAt });
    }
}
