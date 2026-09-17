using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لهواتف الفروع (BranchPhones) مع ضبط الحذف المتتالي والفهارس الفريدة.
/// </summary>
public class BranchPhoneConfiguration : IEntityTypeConfiguration<BranchPhone>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<BranchPhone> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "BranchPhones"
        builder.ToTable("BranchPhones");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(bp => bp.Id);

        // تكوين حقل رقم الهاتف كحقل إجباري بحد أقصى 20 حرفاً
        builder.Property(bp => bp.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        // تكوين حقل مسمى الهاتف (مثل: الإدارة، المبيعات) بحد أقصى 100 حرف
        builder.Property(bp => bp.Name)
            .HasMaxLength(100);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(bp => bp.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط هاتف الفرع بالفرع الرئيسي بعلاقة أب وابن مع تفعيل الحذف المتتالي Cascade لحذف الهواتف عند حذف الفرع
        builder.HasOne(bp => bp.Branch)
            .WithMany(b => b.BranchPhones)
            .HasForeignKey(bp => bp.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط الهاتف بالمستأجر مع ضبط سلوك الحذف NoAction لتفادي مسارات الحذف المتعددة (Multiple Cascade Paths)
        builder.HasOne(bp => bp.Tenant)
            .WithMany()
            .HasForeignKey(bp => bp.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس مركب فريد على (TenantId, PhoneNumber) لضمان عدم تكرار نفس رقم الهاتف للفرع لدى نفس المستأجر
        builder.HasIndex(bp => new { bp.TenantId, bp.PhoneNumber })
            .IsUnique();
    }
}
