using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Customers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لأرقام هواتف العملاء (CustomerPhones).
/// </summary>
public class CustomerPhoneConfiguration : IEntityTypeConfiguration<CustomerPhone>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<CustomerPhone> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "CustomerPhones"
        builder.ToTable("CustomerPhones");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(cp => cp.Id);

        // تكوين حقل رقم الهاتف كحقل إجباري بحد أقصى 20 حرفاً
        builder.Property(cp => cp.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        // تكوين حقل اسم جهة الاتصال أو الشخص المسؤول بحد أقصى 100 حرف
        builder.Property(cp => cp.ContactName)
            .HasMaxLength(100);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(cp => cp.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط هاتف العميل بالعميل التابع له مع تفعيل الحذف المتتالي Cascade لحذف الهواتف عند حذف العميل
        builder.HasOne(cp => cp.Customer)
            .WithMany(c => c.CustomerPhones)
            .HasForeignKey(cp => cp.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط الهاتف بالمستأجر مع منع الإجراء NoAction لتفادي مشاكل المسارات المتعددة للحذف
        builder.HasOne(cp => cp.Tenant)
            .WithMany()
            .HasForeignKey(cp => cp.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس مركب على (TenantId, PhoneNumber) لتسريع البحث برقم الهاتف
        builder.HasIndex(cp => new { cp.TenantId, cp.PhoneNumber });
        // إنشاء فهرس على عمود CustomerId لتسريع استرجاع هواتف العميل
        builder.HasIndex(cp => cp.CustomerId);
    }
}
