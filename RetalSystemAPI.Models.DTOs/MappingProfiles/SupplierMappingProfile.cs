using AutoMapper;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات الموردين (Supplier Mapping Profile).
/// يحدد قواعد التحويل بين كيانات الموردين وهواتفهم ونواقل البيانات في كلا الاتجاهين.
/// </summary>
public class SupplierMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد تحويل كيانات الموردين ونماذج الإنشاء والتعديل والاستجابة وهواتف الموردين.
    /// </summary>
    public SupplierMappingProfile()
    {
        // تكوين تحويل كيان المورد إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<Supplier, SupplierResponseDto>()
            // إسقاط قائمة هواتف المورد من الخاصية الملاحية SupplierPhones
            .ForMember(dest => dest.Phones, opt => opt.MapFrom(src => src.SupplierPhones));

        // تكوين تحويل كيان المورد إلى ناقل بيانات الملخص السريع للعرض في الجداول
        CreateMap<Supplier, SupplierSummaryDto>()
            // احتساب إجمالي عدد الهواتف المسجلة للمورد
            .ForMember(dest => dest.PhoneCount, opt => opt.MapFrom(src => src.SupplierPhones.Count));

        // تكوين تحويل ناقل بيانات إنشاء المورد إلى كيان المورد
        CreateMap<CreateSupplierDto, Supplier>();

        // تكوين تحويل ناقل بيانات تعديل المورد إلى كيان المورد
        CreateMap<UpdateSupplierDto, Supplier>()
            // استبعاد المعرف الأساسي Id لمنع تعديل المفتاح الأساسي للكيان
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // تكوين تحويل كيان هاتف المورد إلى ناقل بيانات استجابة هاتف المورد
        CreateMap<SupplierPhone, SupplierPhoneResponseDto>();
        // تكوين تحويل ناقل بيانات هاتف المورد إلى كيان هاتف المورد
        CreateMap<SupplierPhoneDto, SupplierPhone>();

        // تكوين تحويل كيان حركة حساب المورد إلى ناقل بيانات الاستجابة
        CreateMap<SupplierTransaction, SupplierTransactionResponseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.TransactionTypeName, opt => opt.MapFrom(src =>
                src.TransactionType == SupplierTransactionType.PurchaseInvoice ? "فاتورة شراء" :
                src.TransactionType == SupplierTransactionType.Payment ? "سداد / دفعة" :
                src.TransactionType == SupplierTransactionType.PurchaseReturn ? "مرتجع مشتريات" :
                src.TransactionType == SupplierTransactionType.OpeningBalance ? "رصيد افتتاحي" :
                src.TransactionType == SupplierTransactionType.Adjustment ? "تسوية حساب" :
                src.TransactionType == SupplierTransactionType.Reversal ? "عكس قيد" : src.TransactionType.ToString()))
            .ForMember(dest => dest.DocumentTypeName, opt => opt.MapFrom(src =>
                src.DocumentType == FinancialDocumentType.PurchaseInvoice ? "فاتورة مشتريات" :
                src.DocumentType == FinancialDocumentType.PurchaseReturn ? "مرتجع مشتريات" :
                src.DocumentType == FinancialDocumentType.SupplierPayment ? "سند صرف / سداد" :
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
