using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Context.Configurations;

/// <summary>
/// تكوين Fluent API لبيانات المستأجرين (Tenants) وضمان فرادية اسم المستأجر في النظام بالكامل.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    /// <summary>
    /// تكوين خصائص الكيان، الجداول، المفاتيح، العلاقات، والفهارس في قاعدة البيانات باستخدام Fluent API.
    /// </summary>
    /// <param name="builder">منشئ تكوين الكيان EntityTypeBuilder</param>
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        // تعيين اسم الجدول في قاعدة البيانات إلى "Tenants"
        builder.ToTable("Tenants");
        // تعيين الخاصية Id كمفتاح أساسي (Primary Key) للجدول
        builder.HasKey(t => t.Id);
        // تجاهل خاصية الملاحة الذاتية Tenant لمنع التكرار الدائري
        builder.Ignore(t => t.Tenant);

        // تكوين حقل اسم المستأجر كحقل إجباري بحد أقصى 200 حرف
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        // تكوين حقل رقم الهاتف كحقل إجباري بحد أقصى 20 حرفاً
        builder.Property(t => t.PhoneNumber)
            .IsRequired()
            .HasMaxLength(20);

        // تكوين حقل الوصف بحد أقصى 1000 حرف
        builder.Property(t => t.Description)
            .HasMaxLength(1000);

        // تكوين حقل العنوان بحد أقصى 500 حرف
        builder.Property(t => t.Address)
            .HasMaxLength(500);

        // تكوين حقل رابط الشعار بحد أقصى 2000 حرف
        builder.Property(t => t.LogoUrl)
            .HasMaxLength(2000);

        // تكوين حقل الاسم التجاري بحد أقصى 250 حرفاً
        builder.Property(t => t.CommercialName)
            .HasMaxLength(250);

        // تكوين حقل الرقم الضريبي بحد أقصى 50 حرفاً
        builder.Property(t => t.TaxNumber)
            .HasMaxLength(50);

        // تكوين حقل رقم السجل التجاري بحد أقصى 50 حرفاً
        builder.Property(t => t.CommercialRegistrationNumber)
            .HasMaxLength(50);

        // تكوين حقل نوع النشاط التجاري بحد أقصى 100 حرف
        builder.Property(t => t.BusinessType)
            .HasMaxLength(100);

        // تكوين حقل الهاتف الإضافي بحد أقصى 30 حرفاً
        builder.Property(t => t.AdditionalPhone)
            .HasMaxLength(30);

        // تكوين حقل رابط الموقع الإلكتروني بحد أقصى 250 حرفاً
        builder.Property(t => t.WebsiteUrl)
            .HasMaxLength(250);

        // تكوين حقل المدينة بحد أقصى 100 حرف
        builder.Property(t => t.City)
            .HasMaxLength(100);

        // تكوين حقل الدولة بحد أقصى 100 حرف
        builder.Property(t => t.Country)
            .HasMaxLength(100);

        // تكوين حقل الرمز البريدي بحد أقصى 20 حرفاً
        builder.Property(t => t.PostalCode)
            .HasMaxLength(20);

        // تكوين حقل ترويسة الفاتورة بحد أقصى 1000 حرف
        builder.Property(t => t.InvoiceHeaderNote)
            .HasMaxLength(1000);

        // تكوين حقل تذييل الفاتورة بحد أقصى 2000 حرف
        builder.Property(t => t.InvoiceFooterNote)
            .HasMaxLength(2000);

        // تكوين حقل البيانات البنكية بحد أقصى 2000 حرف
        builder.Property(t => t.BankDetails)
            .HasMaxLength(2000);

        // تكوين حقل العملة الافتراضية بحد أقصى 50 حرفاً
        builder.Property(t => t.DefaultCurrency)
            .HasMaxLength(50);

        // تكوين حقل رمز العملة الافتراضية بحد أقصى 20 حرفاً
        builder.Property(t => t.DefaultCurrencySymbol)
            .HasMaxLength(20);

        // تكوين حقل النسبة الضريبية الافتراضية بدقة عشرية (18, 2)
        builder.Property(t => t.DefaultTaxRate)
            .HasPrecision(18, 2);

        // تكوين حقل المنطقة الزمنية بحد أقصى 100 حرف
        builder.Property(t => t.Timezone)
            .HasMaxLength(100);

        // ضبط خاصية RowVersion كرمز لإدارة التزامن المتفائل (Optimistic Concurrency Token)
        builder.Property(t => t.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        // إنشاء فهرس فريد (Unique Index) على اسم المستأجر لضمان عدم تكرار الأسماء في النظام
        builder.HasIndex(t => t.Name)
            .IsUnique();
    }
}
