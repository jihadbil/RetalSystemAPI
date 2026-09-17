using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لأرقام هواتف الموردين (SupplierPhones).
/// </summary>
public class SupplierPhoneConfiguration : IEntityTypeConfiguration<SupplierPhone>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<SupplierPhone> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "SupplierPhones"
        builder.ToTable("SupplierPhones");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(sp => sp.Id);

        // تكوين حقل رقم الهاتف كحقل إجباري بحد أقصى 20 حرفاً
        builder.Property(sp => sp.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        // تكوين حقل مسمى جهة الاتصال أو الهاتف بحد أقصى 100 حرف
        builder.Property(sp => sp.Name)
            .HasMaxLength(100);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل
        builder.Property(sp => sp.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // ربط هاتف المورد بالمورد التابع له مع تفعيل الحذف المتتالي Cascade لحذف الهواتف عند حذف المورد
        builder.HasOne(sp => sp.Supplier)
            .WithMany(s => s.SupplierPhones)
            .HasForeignKey(sp => sp.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        // ربط الهاتف بالمستأجر مع منع الإجراء NoAction لتفادي مسارات الحذف المتعددة
        builder.HasOne(sp => sp.Tenant)
            .WithMany()
            .HasForeignKey(sp => sp.TenantId)
            .OnDelete(DeleteBehavior.NoAction);

        // إنشاء فهرس مركب فريد على (TenantId, PhoneNumber) لضمان عدم تكرار نفس رقم الهاتف للمورد لدى نفس المستأجر
        builder.HasIndex(sp => new { sp.TenantId, sp.PhoneNumber }).IsUnique();
        // إنشاء فهرس على عمود SupplierId لتسريع استعراض هواتف المورد
        builder.HasIndex(sp => sp.SupplierId);
    }
}
