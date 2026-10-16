using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Accounting;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبيانات الخزائن والبنوك وأدراج النقود (Safes).
/// </summary>
public class SafeConfiguration : IEntityTypeConfiguration<Safe>
{
    public void Configure(EntityTypeBuilder<Safe> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات
        builder.ToTable("Safes", table =>
        {
            table.HasCheckConstraint("CK_Safes_Type", "[Type] IN (1,2,3)");
            table.HasCheckConstraint("CK_Safes_Balances", "[OpeningBalance] >= 0 AND [CurrentBalance] >= 0");
        });
        // تعيين المفتاح الأساسي
        builder.HasKey(s => s.Id);

        // تكوين اسم الخزينة / الحساب / الدرج
        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        // نوع الخزينة (بنك، صندوق، درج نقود)
        builder.Property(s => s.Type)
            .IsRequired();

        // الرصيد الافتتاحي بدقة مالية decimal(18,4)
        builder.Property(s => s.OpeningBalance)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // الرصيد الحالي بدقة مالية decimal(18,4)
        builder.Property(s => s.CurrentBalance)
            .HasColumnType("decimal(18,4)")
            .HasDefaultValue(0);

        // بيانات الحساب البنكي (اختياري)
        builder.Property(s => s.BankAccountNumber)
            .HasMaxLength(100);

        builder.Property(s => s.BankName)
            .HasMaxLength(200);

        // حالة النشاط
        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        // ملاحظات
        builder.Property(s => s.Notes)
            .HasMaxLength(500);

        // إدارة التزامن المتفائل RowVersion
        builder.Property(s => s.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط الخزينة بالفرع
        builder.HasOne(s => s.Branch)
            .WithMany()
            .HasForeignKey(s => s.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        // ربط الخزينة بالمستأجر
        builder.HasOne(s => s.Tenant)
            .WithMany()
            .HasForeignKey(s => s.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // الفهارس
        builder.HasIndex(s => s.TenantId);
        builder.HasIndex(s => new { s.TenantId, s.BranchId });
        builder.HasIndex(s => new { s.TenantId, s.Type });
        builder.HasIndex(s => new { s.TenantId, s.IsActive });
        builder.HasIndex(s => new { s.TenantId, s.IsDeleted });
        builder.HasIndex(s => new { s.TenantId, s.BranchId, s.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
