using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Accounting;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لسندات المصروفات (Expenses).
/// </summary>
public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات
        builder.ToTable("Expenses", table =>
        {
            table.HasCheckConstraint("CK_Expenses_Amount", "[Amount] > 0");
            table.HasCheckConstraint("CK_Expenses_PaymentMethod", "[PaymentMethod] IN (1,2,3,5)");
        });
        // تعيين المفتاح الأساسي
        builder.HasKey(e => e.Id);

        // رقم سند المصروف الفريد
        builder.Property(e => e.ExpenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        // تاريخ المصروف
        builder.Property(e => e.ExpenseDate)
            .IsRequired();

        // مبلغ المصروف بدقة مالية decimal(18,4)
        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,4)")
            .IsRequired();

        // طريقة الدفع
        builder.Property(e => e.PaymentMethod)
            .IsRequired();

        // اسم المستفيد
        builder.Property(e => e.BeneficiaryName)
            .HasMaxLength(200);

        // بيان المصروف
        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        // ملاحظات
        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        // إدارة التزامن المتفائل RowVersion
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط المصروف بتصنيف المصروفات
        builder.HasOne(e => e.ExpenseCategory)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.ExpenseCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المصروف بالفرع
        builder.HasOne(e => e.Branch)
            .WithMany()
            .HasForeignKey(e => e.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المصروف بالخزينة (اختياري)
        builder.HasOne(e => e.Safe)
            .WithMany()
            .HasForeignKey(e => e.SafeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط المصروف بالمستأجر
        builder.HasOne(e => e.Tenant)
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // الفهارس
        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => new { e.TenantId, e.ExpenseNumber }).IsUnique();
        builder.HasIndex(e => new { e.TenantId, e.BranchId });
        builder.HasIndex(e => new { e.TenantId, e.ExpenseCategoryId });
        builder.HasIndex(e => new { e.TenantId, e.SafeId });
        builder.HasIndex(e => e.ExpenseDate);
        builder.HasIndex(e => new { e.TenantId, e.IsDeleted });
    }
}
