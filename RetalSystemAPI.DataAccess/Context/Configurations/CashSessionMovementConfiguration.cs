using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Accounting;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

public class CashSessionMovementConfiguration : IEntityTypeConfiguration<CashSessionMovement>
{
    public void Configure(EntityTypeBuilder<CashSessionMovement> b)
    {
        b.ToTable("CashSessionMovements", t =>
        {
            t.HasCheckConstraint("CK_CashSessionMovements_Amounts", "[Amount] > 0 AND [ReversedAmount] >= 0 AND [ReversedAmount] <= [Amount]");
            t.HasCheckConstraint("CK_CashSessionMovements_Method", "[PaymentMethod] IN (1,2,3,5)");
            t.HasCheckConstraint("CK_CashSessionMovements_Direction", "[Direction] IN (1,2)");
            t.HasCheckConstraint("CK_CashSessionMovements_Tendered", "[TenderedAmount] IS NULL OR ([PaymentMethod] = 1 AND [TenderedAmount] >= [Amount])");
        });
        b.HasKey(x => x.Id);
        b.Property(x => x.Amount).HasColumnType("decimal(18,4)");
        b.Property(x => x.TenderedAmount).HasColumnType("decimal(18,4)");
        b.Property(x => x.ReversedAmount).HasColumnType("decimal(18,4)");
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        b.HasOne(x => x.CashSession).WithMany().HasForeignKey(x => x.CashSessionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SalesInvoice).WithMany().HasForeignKey(x => x.SalesInvoiceId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.OriginalMovement).WithMany().HasForeignKey(x => x.OriginalMovementId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Settlement).WithMany().HasForeignKey(x => x.SettlementId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.SettlementSafe).WithMany().HasForeignKey(x => x.SettlementSafeId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TenantId, x.DocumentType, x.DocumentId, x.ClientMovementId }).IsUnique();
        b.HasIndex(x => new { x.TenantId, x.CashSessionId, x.SettlementId });
        b.HasIndex(x => new { x.TenantId, x.SalesInvoiceId });
    }
}

public class CashSessionPaymentSettlementConfiguration : IEntityTypeConfiguration<CashSessionPaymentSettlement>
{
    public void Configure(EntityTypeBuilder<CashSessionPaymentSettlement> b)
    {
        b.HasKey(x => x.Id);
        b.Property(x => x.RequestHash).HasMaxLength(64).IsUnicode(false).IsRequired();
        b.Property(x => x.Notes).HasMaxLength(500);
        b.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        b.HasOne(x => x.CashSession).WithMany().HasForeignKey(x => x.CashSessionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.TenantId, x.CashSessionId }).IsUnique();
    }
}
