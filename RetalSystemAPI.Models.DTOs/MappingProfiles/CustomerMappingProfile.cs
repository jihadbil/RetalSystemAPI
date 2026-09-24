using AutoMapper;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات العملاء (Customer Mapping Profile).
/// يحدد قواعد التحويل بين كيانات العملاء وهواتفهم ونواقل البيانات مع ترجمة أنواع العملاء إلى نصوص عربية.
/// </summary>
public class CustomerMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد تحويل كيانات العملاء ونماذج الإدخال والاستجابة والملخصات وهواتف العملاء.
    /// </summary>
    public CustomerMappingProfile()
    {
        // تكوين تحويل كيان العميل إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<Customer, CustomerResponseDto>()
            // إسقاط قائمة أرقام هواتف العميل من الخاصية الملاحية CustomerPhones
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.CustomerPhones))
            // تحويل القيمة التعدادية لنوع العميل (Retail, Wholesale, Corporate) إلى الاسم العربي المقابل
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == CustomerType.Retail ? "قطاعي" :
                src.Type == CustomerType.Wholesale ? "جملة" :
                src.Type == CustomerType.Corporate ? "شركات" : src.Type.ToString()));

        // تكوين تحويل كيان العميل إلى ناقل بيانات الملخص السريع للعرض في الجداول
        CreateMap<Customer, CustomerSummaryDto>()
            // احتساب إجمالي عدد الهواتف المسجلة للعميل
            .ForMember(dest => dest.PhoneCount, opt => opt.MapFrom(src => src.CustomerPhones.Count))
            // استخراج رقم الهاتف الافتراضي الأساسي أو أول رقم مسجل
            .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src =>
                src.CustomerPhones.Where(p => p.IsDefault).Select(p => p.PhoneNumber).FirstOrDefault()
                ?? src.CustomerPhones.Select(p => p.PhoneNumber).FirstOrDefault()))
            // تحويل القيمة التعدادية لنوع العميل إلى الاسم العربي المعادل للعرض
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == CustomerType.Retail ? "قطاعي" :
                src.Type == CustomerType.Wholesale ? "جملة" :
                src.Type == CustomerType.Corporate ? "شركات" : src.Type.ToString()));

        // تكوين تحويل ناقل بيانات إنشاء العميل إلى كيان العميل
        CreateMap<CreateCustomerDto, Customer>()
            // تجاهل إسقاط الهواتف هنا لتتم معالجتها والتحقق منها يدوياً وبشكل صريح في خدمة العملاء
            .ForMember(dest => dest.CustomerPhones, opt => opt.Ignore());

        // تكوين تحويل ناقل بيانات تحديث العميل إلى كيان العميل
        CreateMap<UpdateCustomerDto, Customer>()
            // استبعاد المعرف الأساسي Id لمنع تعديل المفتاح الأساسي للعميل
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            // تجاهل إسقاط الهواتف لتفادي الكتابة الخاطئة فوق أرقام الهواتف القائمة
            .ForMember(dest => dest.CustomerPhones, opt => opt.Ignore());

        // تكوين تحويل كيان هاتف العميل إلى ناقل بيانات استجابة هاتف العميل
        CreateMap<CustomerPhone, CustomerPhoneResponseDto>();
        // تكوين تحويل ناقل بيانات هاتف العميل إلى كيان هاتف العميل
        CreateMap<CustomerPhoneDto, CustomerPhone>();

        // تكوين تحويل كيان حركة حساب العميل إلى ناقل بيانات الاستجابة
        CreateMap<CustomerTransaction, CustomerTransactionResponseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.TransactionTypeName, opt => opt.MapFrom(src =>
                src.TransactionType == CustomerTransactionType.SalesInvoice ? "فاتورة بيع" :
                src.TransactionType == CustomerTransactionType.Payment ? "سداد / دفعة" :
                src.TransactionType == CustomerTransactionType.SalesReturn ? "مرتجع مبيعات" :
                src.TransactionType == CustomerTransactionType.OpeningBalance ? "رصيد افتتاحي" :
                src.TransactionType == CustomerTransactionType.Adjustment ? "تسوية حساب" :
                src.TransactionType == CustomerTransactionType.Reversal ? "عكس قيد" : src.TransactionType.ToString()))
            .ForMember(dest => dest.DocumentTypeName, opt => opt.MapFrom(src =>
                src.DocumentType == FinancialDocumentType.SalesInvoice ? "فاتورة مبيعات" :
                src.DocumentType == FinancialDocumentType.SalesReturn ? "مرتجع مبيعات" :
                src.DocumentType == FinancialDocumentType.CustomerPayment ? "سند قبض / سداد" :
                src.DocumentType == FinancialDocumentType.Adjustment ? "تسوية حساب" :
                src.DocumentType == FinancialDocumentType.OpeningBalance ? "رصيد افتتاحي" : src.DocumentType.ToString()))
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == null ? null :
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل بنكي" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()));
    }
}
