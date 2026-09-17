using AutoMapper;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات دورة المشتريات (Purchase Mapping Profile).
/// يحدد قواعد التحويل لأوامر الشراء، فواتير الشراء، تفاصيل العبوات، ومرتجعات الشراء مع ترجمة الحالات وطرق الدفع.
/// </summary>
public class PurchaseMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد تحويل كيانات المشتريات (أوامر الشراء، الفواتير، بنودها، المرتجعات) ونواقل البيانات المقابلة لها.
    /// </summary>
    public PurchaseMappingProfile()
    {
        // ── تحويلات أوامر الشراء (Purchase Order) ──────────────────────────
        // تكوين تحويل كيان أمر الشراء إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<PurchaseOrder, PurchaseOrderResponseDto>()
            // إسقاط اسم المورد بأمان مع التحقق من عدم فراغ الكيان المرتبط
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            // إسقاط اسم الفرع بأمان من الكيان المرتبط
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null!))
            // إسقاط اسم المستودع بأمان من الكيان المرتبط
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src =>
                src.Warehouse != null ? src.Warehouse.Name : null))
            // تحويل حالة أمر الشراء إلى نص
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            // إسقاط قائمة البنود المرتبطة بأمر الشراء
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // تكوين تحويل كيان أمر الشراء إلى ناقل بيانات الملخص السريع للعرض
        CreateMap<PurchaseOrder, PurchaseOrderSummaryDto>()
            // إسقاط اسم المورد
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : null))
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null!))
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src =>
                src.Warehouse != null ? src.Warehouse.Name : null))
            // تحويل اسم الحالة إلى نص
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status.ToString()))
            // احتساب إجمالي عدد بنود أمر الشراء
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        // تكوين تحويل ناقل بيانات إنشاء أمر الشراء إلى كيان أمر الشراء
        CreateMap<CreatePurchaseOrderDto, PurchaseOrder>()
            // تعيين الحالة الافتراضية لأمر الشراء الجديد إلى مسودة (Draft)
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => PurchaseOrderStatus.Draft))
            // تجاهل المبلغ الإجمالي هنا ليتم احتسابه برمجياً من مجموع البنود في الخدمة
            .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

        // تكوين تحويل كيان بند أمر الشراء إلى ناقل بيانات استجابة البند
        CreateMap<PurchaseOrderItem, PurchaseOrderItemResponseDto>()
            // إسقاط وصف أو عنوان الباركود من الكيان المرتبط ProductBarCode
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null!))
            // إسقاط نص وقيمة الباركود
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null!))
            // إسقاط اسم الصنف من الكيان المرتبط عبر الباركود
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductBarCode != null && src.ProductBarCode.Product != null ? src.ProductBarCode.Product.Name : null!));

        // تكوين تحويل ناقل بيانات بند أمر الشراء إلى كيان بند أمر الشراء
        CreateMap<PurchaseOrderItemDto, PurchaseOrderItem>()
            // تجاهل إجمالي سطر البند ليتم حسابه بدقة رياضية في طبقة الخدمات
            .ForMember(dest => dest.LineTotal, opt => opt.Ignore());

        // ── تحويلات فواتير الشراء (Purchase Invoice) ───────────────────────
        // تكوين تحويل كيان فاتورة الشراء إلى ناقل بيانات الملخص السريع
        CreateMap<PurchaseInvoice, PurchaseInvoiceSummaryDto>()
            // إسقاط اسم المورد
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // ترجمة حالة الفاتورة التعدادية إلى النص العربي المعادل
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == InvoiceStatus.Draft ? "مسودة" :
                src.Status == InvoiceStatus.Pending ? "مفتوحة قيد الإدخال" :
                src.Status == InvoiceStatus.Paid ? "مغلقة ومرحلة" :
                src.Status == InvoiceStatus.PartiallyPaid ? "مسددة جزئياً" :
                src.Status == InvoiceStatus.Cancelled ? "ملغاة" :
                src.Status == InvoiceStatus.Voided ? "باطلة" : src.Status.ToString()))
            // ترجمة طريقة الدفع التعدادية إلى التسمية العربية المعتمدة
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()))
            // احتساب إجمالي عدد الأصناف في الفاتورة
            .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items.Count));

        // تكوين تحويل كيان فاتورة الشراء إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<PurchaseInvoice, PurchaseInvoiceResponseDto>()
            // إسقاط اسم المورد
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // ترجمة حالة الفاتورة التعدادية إلى النص العربي
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == InvoiceStatus.Draft ? "مسودة" :
                src.Status == InvoiceStatus.Pending ? "مفتوحة قيد الإدخال" :
                src.Status == InvoiceStatus.Paid ? "مغلقة ومرحلة" :
                src.Status == InvoiceStatus.PartiallyPaid ? "مسددة جزئياً" :
                src.Status == InvoiceStatus.Cancelled ? "ملغاة" :
                src.Status == InvoiceStatus.Voided ? "باطلة" : src.Status.ToString()))
            // ترجمة طريقة الدفع التعدادية إلى النص العربي
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()))
            // إسقاط رقم أمر الشراء المرتبط إن وجد
            .ForMember(dest => dest.PurchaseOrderNumber, opt => opt.MapFrom(src => src.PurchaseOrder != null ? src.PurchaseOrder.OrderNumber : null))
            // إسقاط قائمة البنود المفصلة للفاتورة
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // تكوين تحويل كيان بند فاتورة الشراء إلى ناقل بيانات استجابة البند
        CreateMap<PurchaseInvoiceItem, PurchaseInvoiceItemResponseDto>()
            // إسقاط اسم الصنف
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            // إسقاط رمز الباركود من الكيان المرتبط ProductBarCode
            .ForMember(dest => dest.BarCode, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null))
            // إسقاط تفاصيل تفكيك العبوات (Breakdowns)
            .ForMember(dest => dest.Breakdowns, opt => opt.MapFrom(src => src.Breakdowns));

        // تكوين تحويل تفكيك عبوة الشراء إلى ناقل بيانات استجابة التفكيك
        CreateMap<PurchaseInvoiceItemBreakdown, PurchaseInvoiceItemBreakdownResponseDto>()
            // إسقاط كود الباركود
            .ForMember(dest => dest.BarCode, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null))
            // إسقاط مسمى أو وصف الباركود
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null));

        // تكوين تحويل ناقل بيانات إنشاء فاتورة الشراء إلى كيان الفاتورة
        CreateMap<CreatePurchaseInvoiceDto, PurchaseInvoice>()
            // تجاهل إسقاط البنود لتتم معالجتها والتحقق من حساباتها وأرصدتها في الخدمة
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        // تكوين تحويل ناقل بيانات إنشاء بند فاتورة الشراء إلى كيان البند
        CreateMap<CreatePurchaseInvoiceItemDto, PurchaseInvoiceItem>();
        // تكوين تحويل ناقل تفكيك عبوة الشراء إلى كيان تفكيك العبوة
        CreateMap<CreatePurchaseInvoiceItemBreakdownDto, PurchaseInvoiceItemBreakdown>();

        // ── تحويلات مرتجعات الشراء (Purchase Return) ───────────────────────
        // تكوين تحويل كيان مرتجع الشراء إلى ناقل بيانات الملخص
        CreateMap<PurchaseReturn, PurchaseReturnSummaryDto>()
            // إسقاط رقم فاتورة الشراء المرتجع منها
            .ForMember(dest => dest.PurchaseInvoiceNumber, opt => opt.MapFrom(src => src.PurchaseInvoice != null ? src.PurchaseInvoice.InvoiceNumber : null))
            // إسقاط اسم المورد
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // ترجمة طريقة استرداد أو تسوية المرتجع إلى العربية
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل (تخفيض حساب المورد)" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()))
            // ترجمة سبب الإرجاع إلى التسمية العربية التوضيحية
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == PurchaseReturnReason.Defective ? "بضاعة معيبة أو تالفة" :
                src.Reason == PurchaseReturnReason.WrongSpecification ? "غير مطابق للمواصفات" :
                src.Reason == PurchaseReturnReason.NearExpiryOrExpired ? "منتهي أو قريب الصلاحية" :
                src.Reason == PurchaseReturnReason.ExcessStock ? "فائض مخزون / مرتجع" :
                src.Reason == PurchaseReturnReason.Other ? "سبب آخر" : src.Reason.ToString()))
            // احتساب إجمالي عدد البنود المرتجعة
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        // تكوين تحويل كيان مرتجع الشراء إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<PurchaseReturn, PurchaseReturnResponseDto>()
            // إسقاط رقم فاتورة الشراء المرجعية
            .ForMember(dest => dest.PurchaseInvoiceNumber, opt => opt.MapFrom(src => src.PurchaseInvoice != null ? src.PurchaseInvoice.InvoiceNumber : null))
            // إسقاط اسم المورد
            .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // ترجمة طريقة السداد
            .ForMember(dest => dest.PaymentMethodName, opt => opt.MapFrom(src =>
                src.PaymentMethod == PaymentMethod.Cash ? "نقدي" :
                src.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" :
                src.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" :
                src.PaymentMethod == PaymentMethod.Credit ? "آجل (تخفيض حساب المورد)" :
                src.PaymentMethod == PaymentMethod.Cheque ? "صك" : src.PaymentMethod.ToString()))
            // ترجمة سبب الإرجاع إلى العربية
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == PurchaseReturnReason.Defective ? "بضاعة معيبة أو تالفة" :
                src.Reason == PurchaseReturnReason.WrongSpecification ? "غير مطابق للمواصفات" :
                src.Reason == PurchaseReturnReason.NearExpiryOrExpired ? "منتهي أو قريب الصلاحية" :
                src.Reason == PurchaseReturnReason.ExcessStock ? "فائض مخزون / مرتجع" :
                src.Reason == PurchaseReturnReason.Other ? "سبب آخر" : src.Reason.ToString()))
            // إسقاط قائمة البنود المرتجعة
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // تكوين تحويل كيان بند مرتجع الشراء إلى ناقل بيانات الاستجابة
        CreateMap<PurchaseReturnItem, PurchaseReturnItemResponseDto>()
            // إسقاط اسم الصنف
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            // إسقاط مسمى أو عنوان الباركود
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            // إسقاط قيمة كود الباركود
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        // تكوين تحويل ناقل بيانات إنشاء مرتجع الشراء إلى كيان مرتجع الشراء
        CreateMap<CreatePurchaseReturnDto, PurchaseReturn>()
            // تجاهل البنود لتتم معالجتها في خدمة المرتجعات مع فحص المخزون
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        // تكوين تحويل ناقل بيانات إنشاء بند مرتجع الشراء إلى كيان بند المرتجع
        CreateMap<CreatePurchaseReturnItemDto, PurchaseReturnItem>()
            // احتساب إجمالي السطر بضرب الكمية في سعر الوحدة
            .ForMember(dest => dest.LineTotal, opt => opt.MapFrom(src => src.Quantity * src.UnitPrice));
    }
}
