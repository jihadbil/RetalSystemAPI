using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لحركات كشف حساب الزبائن والعملاء (CustomerTransactions).
/// </summary>
public class CustomerTransactionConfiguration : IEntityTypeConfiguration<CustomerTransaction>
{
    public void Configure(EntityTypeBuilder<CustomerTransaction> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات
        builder.ToTable("CustomerTransactions", table =>
        {
            table.HasCheckConstraint("CK_CustomerTransactions_Type", "[TransactionType] BETWEEN 1 AND 6");
            table.HasCheckConstraint("CK_CustomerTransactions_DebitCredit", "([DebitAmount] > 0 AND [CreditAmount] = 0) OR ([CreditAmount] > 0 AND [DebitAmount] = 0)");
            table.HasCheckConstraint("CK_CustomerTransactions_DocumentType", "[DocumentType] BETWEEN 1 AND 13");
            table.HasCheckConstraint("CK_CustomerTransactions_PaymentMethod", "[PaymentMethod] IS NULL OR [PaymentMethod] BETWEEN 1 AND 5");
        });
        // تعيين المفتاح الأساسي
        builder.HasKey(ct => ct.Id);

        // نوع الحركة
        builder.Property(ct => ct.TransactionType)
            .IsRequired();

        // المبلغ المدين بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(ct => ct.DebitAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // المبلغ الدائن بدقة مالية decimal(18,4) مع قيمة افتراضية صفر
        builder.Property(ct => ct.CreditAmount)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // الرصيد بعد الحركة بدقة مالية decimal(18,4)
        builder.Property(ct => ct.BalanceAfter)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        // تاريخ ووقت الحركة
        builder.Property(ct => ct.TransactionDate)
            .IsRequired();

        // نوع المستند المالي
        builder.Property(ct => ct.DocumentType)
            .IsRequired();

        // البيان / الشرح
        builder.Property(ct => ct.Description)
            .HasMaxLength(500);

        // ملاحظات
        builder.Property(ct => ct.Notes)
            .HasMaxLength(500);

        // إدارة التزامن المتفائل RowVersion
        builder.Property(ct => ct.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الحركة بالعميل
        builder.HasOne(ct => ct.Customer)
            .WithMany(c => c.CustomerTransactions)
            .HasForeignKey(ct => ct.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الحركة بالفرع
        builder.HasOne(ct => ct.Branch)
            .WithMany()
            .HasForeignKey(ct => ct.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الحركة بالمستأجر
        builder.HasOne(ct => ct.Tenant)
            .WithMany()
            .HasForeignKey(ct => ct.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // الفهارس
        builder.HasIndex(ct => ct.TenantId);
        builder.HasIndex(ct => new { ct.TenantId, ct.CustomerId, ct.TransactionDate });
        builder.HasIndex(ct => new { ct.TenantId, ct.CustomerId, ct.TransactionType });
        builder.HasIndex(ct => new { ct.TenantId, ct.BranchId });
        builder.HasIndex(ct => new { ct.TenantId, ct.DocumentType, ct.DocumentId });
        builder.HasIndex(ct => ct.TransactionDate);
        builder.HasIndex(ct => new { ct.TenantId, ct.IsDeleted });
    }
}
