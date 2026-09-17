using AutoMapper;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات دورة المبيعات (Sales Mapping Profile).
/// يحدد قواعد التحويل لفواتير المبيعات وبنودها ومرتجعات المبيعات مع ترجمة الحالات وطرق الدفع وأسباب الإرجاع.
/// </summary>
public class SalesMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد تحويل كيانات المبيعات (فواتير، مرتجعات، بنود تفصيلية) ونواقل البيانات المقابلة لها.
    /// </summary>
    public SalesMappingProfile()
    {
        // ── تحويلات فواتير المبيعات (Sales Invoice) ──────────────────────────
        // تكوين تحويل كيان فاتورة المبيعات إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<SalesInvoice, SalesInvoiceResponseDto>()
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // إسقاط اسم العميل إن وجد
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            // ترجمة حالة الفاتورة التعدادية إلى النص العربي المعادل
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == InvoiceStatus.Draft ? "مسودة" :
                src.Status == InvoiceStatus.Pending ? "معلقة" :
                src.Status == InvoiceStatus.Paid ? "مدفوعة" :
                src.Status == InvoiceStatus.PartiallyPaid ? "مدفوعة جزئياً" :
                src.Status == InvoiceStatus.Cancelled ? "ملغاة" :
                src.Status == InvoiceStatus.Voided ? "باطلة" : src.Status.ToString()))
            // ترجمة طريقة الدفع التعدادية إلى النص العربي
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()))
            // إسقاط قائمة بنود الفاتورة المفصلة
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // تكوين تحويل كيان فاتورة المبيعات إلى ناقل بيانات الملخص السريع للعرض
        CreateMap<SalesInvoice, SalesInvoiceSummaryDto>()
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // إسقاط اسم العميل
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            // ترجمة حالة الفاتورة إلى النص العربي
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == InvoiceStatus.Draft ? "مسودة" :
                src.Status == InvoiceStatus.Pending ? "معلقة" :
                src.Status == InvoiceStatus.Paid ? "مدفوعة" :
                src.Status == InvoiceStatus.PartiallyPaid ? "مدفوعة جزئياً" :
                src.Status == InvoiceStatus.Cancelled ? "ملغاة" :
                src.Status == InvoiceStatus.Voided ? "باطلة" : src.Status.ToString()))
            // ترجمة طريقة الدفع إلى النص العربي
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()))
            // احتساب إجمالي عدد البنود المسجلة بالفاتورة
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        // تكوين تحويل ناقل بيانات إنشاء فاتورة المبيعات إلى كيان الفاتورة
        CreateMap<CreateSalesInvoiceDto, SalesInvoice>()
            // احتساب المبلغ المتبقي تلقائياً بطرح المدفوع من الإجمالي الكلي
            .ForMember(dest => dest.RemainingAmount, opt => opt.MapFrom(src => src.TotalAmount - src.PaidAmount))
            // تجاهل إسقاط البنود لتتم معالجتها والتحقق من توفر أرصدتها في الخدمة
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        // تكوين تحويل ناقل بيانات تعديل فاتورة المبيعات إلى كيان الفاتورة
        CreateMap<UpdateSalesInvoiceDto, SalesInvoice>()
            // استبعاد المعرف Id لمنع تعديل المفتاح الأساسي
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            // تجاهل المبلغ المتبقي ليتم إعادة حسابه في طبقة الخدمات
            .ForMember(dest => dest.RemainingAmount, opt => opt.Ignore());

        // تكوين تحويل كيان بند فاتورة المبيعات إلى ناقل بيانات استجابة البند
        CreateMap<SalesInvoiceItem, SalesInvoiceItemResponseDto>()
            // إسقاط اسم الصنف من الكيان المرتبط
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            // إسقاط عنوان الباركود
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            // إسقاط قيمة رمز الباركود
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        // تكوين تحويل ناقل بيانات بند فاتورة المبيعات إلى كيان البند
        CreateMap<SalesInvoiceItemDto, SalesInvoiceItem>()
            // احتساب إجمالي السطر (الكمية × السعر) مخصوماً منها الخصم الممنوح
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => (src.Quantity * src.UnitPrice) - src.DiscountAmount));

        // ── تحويلات مرتجعات المبيعات (Sales Return) ─────────────────────────
        // تكوين تحويل كيان مرتجع المبيعات إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<SalesReturn, SalesReturnResponseDto>()
            // إسقاط رقم فاتورة المبيعات الأصلية المرتجع منها
            .ForMember(dest => dest.OriginalInvoiceNumber, opt => opt.MapFrom(src => src.OriginalInvoice != null ? src.OriginalInvoice.InvoiceNumber : null))
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // إسقاط اسم المستودع المستلم للبضاعة المعادة
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // إسقاط اسم العميل
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            // ترجمة سبب إرجاع المبيعات إلى النص العربي التوضيحي
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == SalesReturnReason.Defective ? "بضاعة معيبة أو تالفة" :
                src.Reason == SalesReturnReason.WrongItem ? "صنف خاطئ" :
                src.Reason == SalesReturnReason.CustomerChange ? "تغيير رأي العميل" :
                src.Reason == SalesReturnReason.Expired ? "منتهي الصلاحية" :
                src.Reason == SalesReturnReason.Other ? "سبب آخر" : src.Reason.ToString()))
            // إسقاط قائمة البنود المرتجعة
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // تكوين تحويل كيان مرتجع المبيعات إلى ناقل بيانات الملخص السريع
        CreateMap<SalesReturn, SalesReturnSummaryDto>()
            // إسقاط رقم فاتورة المبيعات الأصلية
            .ForMember(dest => dest.OriginalInvoiceNumber, opt => opt.MapFrom(src => src.OriginalInvoice != null ? src.OriginalInvoice.InvoiceNumber : null))
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // إسقاط اسم العميل
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            // ترجمة سبب إرجاع المبيعات إلى النص العربي
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == SalesReturnReason.Defective ? "بضاعة معيبة أو تالفة" :
                src.Reason == SalesReturnReason.WrongItem ? "صنف خاطئ" :
                src.Reason == SalesReturnReason.CustomerChange ? "تغيير رأي العميل" :
                src.Reason == SalesReturnReason.Expired ? "منتهي الصلاحية" :
                src.Reason == SalesReturnReason.Other ? "سبب آخر" : src.Reason.ToString()))
            // احتساب إجمالي عدد البنود المرتجعة
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        // تكوين تحويل ناقل بيانات إنشاء مرتجع المبيعات إلى كيان المرتجع
        CreateMap<CreateSalesReturnDto, SalesReturn>()
            // تجاهل البنود لتتم معالجتها في خدمة المرتجعات مع إعادة المخزون
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        // تكوين تحويل كيان بند مرتجع المبيعات إلى ناقل بيانات استجابة البند
        CreateMap<SalesReturnItem, SalesReturnItemResponseDto>()
            // إسقاط اسم الصنف
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            // إسقاط مسمى الباركود
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            // إسقاط قيمة رمز الباركود
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        // تكوين تحويل ناقل بيانات إنشاء بند مرتجع المبيعات إلى كيان البند
        CreateMap<SalesReturnItemDto, SalesReturnItem>()
            // احتساب إجمالي السطر (الكمية × السعر)
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));
    }
}
