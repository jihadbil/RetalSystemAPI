using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Accounting;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لحركات الخزائن والبنوك وأدراج النقود (SafeTransactions).
/// </summary>
public class SafeTransactionConfiguration : IEntityTypeConfiguration<SafeTransaction>
{
    public void Configure(EntityTypeBuilder<SafeTransaction> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات
        builder.ToTable("SafeTransactions", table =>
        {
            table.HasCheckConstraint("CK_SafeTransactions_Amount", "[Amount] > 0");
            table.HasCheckConstraint("CK_SafeTransactions_Direction", "[Direction] IN (1,2)");
            table.HasCheckConstraint("CK_SafeTransactions_DocumentType", "[DocumentType] BETWEEN 1 AND 13");
        });
        // تعيين المفتاح الأساسي
        builder.HasKey(st => st.Id);

        // رقم الحركة الفريد
        builder.Property(st => st.TransactionNumber)
            .IsRequired()
            .HasMaxLength(50);

        // اتجاه الحركة (وارد أو صادر)
        builder.Property(st => st.Direction)
            .IsRequired();

        // مبلغ الحركة بدقة مالية decimal(18,4)
        builder.Property(st => st.Amount)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        // الرصيد بعد الحركة بدقة مالية decimal(18,4)
        builder.Property(st => st.BalanceAfter)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        // تاريخ ووقت الحركة
        builder.Property(st => st.TransactionDate)
            .IsRequired();

        // نوع المستند المالي
        builder.Property(st => st.DocumentType)
            .IsRequired();

        // البيان / الشرح
        builder.Property(st => st.Description)
            .HasMaxLength(500);

        // ملاحظات
        builder.Property(st => st.Notes)
            .HasMaxLength(500);

        builder.Property(st => st.IdempotencyKey).HasMaxLength(150);
        builder.Property(st => st.PerformedByUserId).HasMaxLength(450);

        // إدارة التزامن المتفائل RowVersion
        builder.Property(st => st.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الحركة بالخزينة
        builder.HasOne(st => st.Safe)
            .WithMany(s => s.Transactions)
            .HasForeignKey(st => st.SafeId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الحركة بالمستأجر
        builder.HasOne(st => st.Tenant)
            .WithMany()
            .HasForeignKey(st => st.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.CashSession)
            .WithMany()
            .HasForeignKey(st => st.CashSessionId)
            .OnDelete(DeleteBehavior.Restrict);

        // الفهارس
        builder.HasIndex(st => st.TenantId);
        builder.HasIndex(st => new { st.TenantId, st.TransactionNumber }).IsUnique();
        builder.HasIndex(st => new { st.TenantId, st.SafeId, st.TransactionDate });
        builder.HasIndex(st => new { st.TenantId, st.SafeId, st.Direction });
        builder.HasIndex(st => new { st.TenantId, st.DocumentType, st.DocumentId });
        builder.HasIndex(st => st.TransactionDate);
        builder.HasIndex(st => new { st.TenantId, st.IsDeleted });
        builder.HasIndex(st => new { st.TenantId, st.IdempotencyKey })
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasIndex(st => new { st.TenantId, st.CashSessionId, st.TransactionDate });
    }
}
