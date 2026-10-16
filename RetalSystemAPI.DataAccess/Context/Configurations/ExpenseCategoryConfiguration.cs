using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Accounting;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لتصنيفات المصروفات (ExpenseCategories).
/// </summary>
public class ExpenseCategoryConfiguration : IEntityTypeConfiguration<ExpenseCategory>
{
    public void Configure(EntityTypeBuilder<ExpenseCategory> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات
        builder.ToTable("ExpenseCategories");
        // تعيين المفتاح الأساسي
        builder.HasKey(ec => ec.Id);

        // اسم تصنيف المصروف
        builder.Property(ec => ec.Name)
            .IsRequired()
            .HasMaxLength(200);

        // حالة النشاط
        builder.Property(ec => ec.IsActive)
            .HasDefaultValue(true);

        // إدارة التزامن المتفائل RowVersion
        builder.Property(ec => ec.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط التصنيف بالمستأجر
        builder.HasOne(ec => ec.Tenant)
            .WithMany()
            .HasForeignKey(ec => ec.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // الفهارس
        builder.HasIndex(ec => ec.TenantId);
        builder.HasIndex(ec => new { ec.TenantId, ec.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(ec => new { ec.TenantId, ec.IsActive });
        builder.HasIndex(ec => new { ec.TenantId, ec.IsDeleted });
    }
}
