using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لحركات كشف حساب الموردين (SupplierTransactions).
/// </summary>
public class SupplierTransactionConfiguration : IEntityTypeConfiguration<SupplierTransaction>
{
    public void Configure(EntityTypeBuilder<SupplierTransaction> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات
        builder.ToTable("SupplierTransactions", table =>
        {
            table.HasCheckConstraint("CK_SupplierTransactions_Type", "[TransactionType] BETWEEN 1 AND 6");
            table.HasCheckConstraint("CK_SupplierTransactions_DebitCredit", "([DebitAmount] > 0 AND [CreditAmount] = 0) OR ([CreditAmount] > 0 AND [DebitAmount] = 0)");
            table.HasCheckConstraint("CK_SupplierTransactions_DocumentType", "[DocumentType] BETWEEN 1 AND 13");
            table.HasCheckConstraint("CK_SupplierTransactions_PaymentMethod", "[PaymentMethod] IS NULL OR [PaymentMethod] BETWEEN 1 AND 5");
        });
        // تعيين المفتاح الأساسي
        builder.HasKey(st => st.Id);

        // نوع الحركة
        builder.Property(st => st.TransactionType)
            .IsRequired();

        // المبلغ المدين بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(st => st.DebitAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // المبلغ الدائن بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(st => st.CreditAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

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

        // إدارة التزامن المتفائل RowVersion
        builder.Property(st => st.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الحركة بالمورد
        builder.HasOne(st => st.Supplier)
            .WithMany(s => s.SupplierTransactions)
            .HasForeignKey(st => st.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الحركة بالفرع
        builder.HasOne(st => st.Branch)
            .WithMany()
            .HasForeignKey(st => st.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الحركة بالمستأجر
        builder.HasOne(st => st.Tenant)
            .WithMany()
            .HasForeignKey(st => st.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // الفهارس
        builder.HasIndex(st => st.TenantId);
        builder.HasIndex(st => new { st.TenantId, st.SupplierId, st.TransactionDate });
        builder.HasIndex(st => new { st.TenantId, st.SupplierId, st.TransactionType });
        builder.HasIndex(st => new { st.TenantId, st.BranchId });
        builder.HasIndex(st => new { st.TenantId, st.DocumentType, st.DocumentId });
        builder.HasIndex(st => st.TransactionDate);
        builder.HasIndex(st => new { st.TenantId, st.IsDeleted });
    }
}
