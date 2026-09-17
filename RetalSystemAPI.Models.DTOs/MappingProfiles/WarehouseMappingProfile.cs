using AutoMapper;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

/// <summary>
/// ملف تعريف تحويلات المستودعات والمخزون (Warehouse Mapping Profile).
/// يحدد قواعد التحويل للمستودعات، أرصدة المخازن وصالات العرض، تحويلات المخزون، وتسويات الجرد.
/// </summary>
public class WarehouseMappingProfile : Profile
{
    /// <summary>
    /// يُهيئ قواعد تحويل كيانات المستودعات والأرصدة والتحويلات والتسويات ونواقل البيانات المقابلة لها.
    /// </summary>
    public WarehouseMappingProfile()
    {
        // ── تحويلات المستودعات (Warehouse) ──────────────────────────────────
        // تكوين تحويل كيان المستودع إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<Warehouse, WarehouseResponseDto>()
            // إسقاط اسم الفرع من الكيان المرتبط Branch
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null!))
            // ترجمة نوع المستودع التعدادي (مخزن / صالة عرض) إلى نص عربي
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == WarehouseType.Storge ? "مخزن" : "صالة عرض"));

        // تكوين تحويل كيان المستودع إلى ناقل بيانات الملخص السريع للجداول
        CreateMap<Warehouse, WarehouseSummaryDto>()
            // إسقاط اسم الفرع
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null!))
            // ترجمة نوع المستودع إلى نص عربي
            .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                src.Type == WarehouseType.Storge ? "مخزن" : "صالة عرض"));

        // تكوين تحويل ناقل بيانات إنشاء المستودع إلى كيان المستودع
        CreateMap<CreateWarehouseDto, Warehouse>();

        // تكوين تحويل ناقل بيانات تعديل المستودع إلى كيان المستودع
        CreateMap<UpdateWarehouseDto, Warehouse>()
            // استبعاد المعرف الأساسي Id لمنع تعديله
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // ── تحويلات أرصدة المخازن الرئيسية (StorgeStock) ────────────────────
        // تكوين تحويل كيان رصيد المخزن إلى ناقل بيانات الاستجابة
        CreateMap<StorgeStock, StorgeStockResponseDto>()
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null!))
            // إسقاط عنوان الباركود
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarcode != null ? src.ProductBarcode.Title : null!))
            // إسقاط رمز الباركود
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarcode != null ? src.ProductBarcode.BarCode : null!))
            // إسقاط اسم الصنف
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.ProductBarcode != null && src.ProductBarcode.Product != null ? src.ProductBarcode.Product.Name : null!))
            // استخراج رابط صورة الباركود المخصصة أو الصورة الافتراضية للصنف
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                src.ProductBarcode != null && src.ProductBarcode.Product != null
                    ? (src.ProductBarcode.Product.ProductImages.FirstOrDefault(i => i.BarcodeId == src.ProductBarcodeId) != null
                        ? src.ProductBarcode.Product.ProductImages.First(i => i.BarcodeId == src.ProductBarcodeId).ImageUrl
                        : (src.ProductBarcode.Product.ProductImages.FirstOrDefault(i => i.IsDefault) != null
                            ? src.ProductBarcode.Product.ProductImages.First(i => i.IsDefault).ImageUrl
                            : (src.ProductBarcode.Product.ProductImages.FirstOrDefault() != null
                                ? src.ProductBarcode.Product.ProductImages.First().ImageUrl
                                : null)))
                    : null))
            // التحقق مما إذا كان الرصيد الحالي أقل من حد الطلب الأدنى
            .ForMember(dest => dest.IsBelowMinLevel, opt => opt.MapFrom(src => src.Quantity < src.MinStockLevel));

        // ── تحويلات أرصدة صالات العرض (ShowroomStock) ───────────────────────
        // تكوين تحويل كيان رصيد الصالة إلى ناقل بيانات الاستجابة
        CreateMap<ShowroomStock, ShowroomStockResponseDto>()
            // إسقاط اسم المستودع أو الصالة
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : null!))
            // إسقاط اسم الصنف
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null!))
            // استخراج رابط الصورة الافتراضية للصنف أو أول صورة مسجلة
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src =>
                src.Product != null
                    ? (src.Product.ProductImages.FirstOrDefault(i => i.IsDefault) != null
                        ? src.Product.ProductImages.First(i => i.IsDefault).ImageUrl
                        : (src.Product.ProductImages.FirstOrDefault() != null
                            ? src.Product.ProductImages.First().ImageUrl
                            : null))
                    : null))
            // فحص هبوط الرصيد دون الحد الأدنى
            .ForMember(dest => dest.IsBelowMinLevel, opt => opt.MapFrom(src => src.Quantity < src.MinStockLevel));

        // ── تحويلات التحويل المخزني (Stock Transfer) ─────────────────────────
        // تكوين تحويل كيان أمر التحويل إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<StockTransfer, StockTransferResponseDto>()
            // إسقاط اسم المستودع المحوَّل منه
            .ForMember(dest => dest.FromWarehouseName, opt => opt.MapFrom(src => src.FromWarehouse != null ? src.FromWarehouse.Name : string.Empty))
            // إسقاط اسم المستودع المحوَّل إليه
            .ForMember(dest => dest.ToWarehouseName, opt => opt.MapFrom(src => src.ToWarehouse != null ? src.ToWarehouse.Name : string.Empty))
            // ترجمة حالة عملية التحويل إلى النص العربي المقابل
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == StockTransferStatus.Draft ? "مسودة" :
                src.Status == StockTransferStatus.Confirmed ? "مؤكدة" :
                src.Status == StockTransferStatus.Completed ? "منفذة ومرحّلة" :
                src.Status == StockTransferStatus.Cancelled ? "ملغاة" : src.Status.ToString()))
            // إسقاط قائمة البنود المحولة
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // تكوين تحويل كيان أمر التحويل إلى ناقل بيانات الملخص السريع للعرض
        CreateMap<StockTransfer, StockTransferSummaryDto>()
            // إسقاط اسم المستودع المحول منه
            .ForMember(dest => dest.FromWarehouseName, opt => opt.MapFrom(src => src.FromWarehouse != null ? src.FromWarehouse.Name : string.Empty))
            // إسقاط اسم المستودع المحول إليه
            .ForMember(dest => dest.ToWarehouseName, opt => opt.MapFrom(src => src.ToWarehouse != null ? src.ToWarehouse.Name : string.Empty))
            // ترجمة حالة التحويل إلى النص العربي
            .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                src.Status == StockTransferStatus.Draft ? "مسودة" :
                src.Status == StockTransferStatus.Confirmed ? "مؤكدة" :
                src.Status == StockTransferStatus.Completed ? "منفذة ومرحّلة" :
                src.Status == StockTransferStatus.Cancelled ? "ملغاة" : src.Status.ToString()))
            // احتساب إجمالي عدد البنود المحولة
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        // تكوين تحويل ناقل بيانات إنشاء تحويل مخزني إلى كيان التحويل
        CreateMap<CreateStockTransferDto, StockTransfer>()
            // تعيين الحالة الافتراضية للتحويل الجديد إلى مسودة (Draft)
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => StockTransferStatus.Draft))
            // تجاهل البنود لتتم معالجتها في خدمة المخزون مع التحقق من الأرصدة المتاحة
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        // تكوين تحويل ناقل بيانات تعديل التحويل المخزني إلى كيان التحويل
        CreateMap<UpdateStockTransferDto, StockTransfer>()
            // استبعاد المعرف الأساسي Id من التعديل
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // تكوين تحويل كيان بند التحويل المخزني إلى ناقل بيانات استجابة البند
        CreateMap<StockTransferItem, StockTransferItemResponseDto>()
            // إسقاط اسم الصنف
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            // إسقاط عنوان الباركود
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            // إسقاط رمز الباركود
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null));

        // تكوين تحويل ناقل بيانات بند التحويل المخزني إلى كيان بند التحويل
        CreateMap<StockTransferItemDto, StockTransferItem>();

        // ── تحويلات تسويات الجرد المخزني (Stock Adjustment) ─────────────────
        // تكوين تحويل كيان تسوية المخزون إلى ناقل بيانات الاستجابة التفصيلي
        CreateMap<StockAdjustment, StockAdjustmentResponseDto>()
            // إسقاط اسم المستودع الخاضع للتسوية
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // ترجمة سبب التسوية التعدادي إلى مسمى عربي واضح
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == StockAdjustmentReason.InventoryCount ? "جرد دوري" :
                src.Reason == StockAdjustmentReason.Damaged ? "بضاعة تالفة" :
                src.Reason == StockAdjustmentReason.Expired ? "بضاعة منتهية الصلاحية" :
                src.Reason == StockAdjustmentReason.InitialSetup ? "إعداد رصيد افتتاحي" :
                src.Reason == StockAdjustmentReason.Other ? "أخرى" : src.Reason.ToString()))
            // إسقاط بنود التسوية التفصيلية
            .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

        // تكوين تحويل كيان تسوية المخزون إلى ناقل بيانات الملخص السريع للعرض
        CreateMap<StockAdjustment, StockAdjustmentSummaryDto>()
            // إسقاط اسم المستودع
            .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
            // ترجمة سبب التسوية إلى نص عربي
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == StockAdjustmentReason.InventoryCount ? "جرد دوري" :
                src.Reason == StockAdjustmentReason.Damaged ? "بضاعة تالفة" :
                src.Reason == StockAdjustmentReason.Expired ? "بضاعة منتهية الصلاحية" :
                src.Reason == StockAdjustmentReason.InitialSetup ? "إعداد رصيد افتتاحي" :
                src.Reason == StockAdjustmentReason.Other ? "أخرى" : src.Reason.ToString()))
            // احتساب إجمالي عدد البنود في التسوية
            .ForMember(dest => dest.ItemCount, opt => opt.MapFrom(src => src.Items.Count));

        // تكوين تحويل ناقل بيانات إنشاء تسوية المخزون إلى كيان التسوية
        CreateMap<CreateStockAdjustmentDto, StockAdjustment>()
            // تجاهل البنود لتتم معالجتها وإنشاء حركات التسوية بدقة في الخدمة
            .ForMember(dest => dest.Items, opt => opt.Ignore());

        // تكوين تحويل كيان بند تسوية المخزون إلى ناقل بيانات استجابة البند
        CreateMap<StockAdjustmentItem, StockAdjustmentItemResponseDto>()
            // إسقاط اسم الصنف
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
            // إسقاط مسمى الباركود
            .ForMember(dest => dest.BarcodeTitle, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.Title : null))
            // إسقاط رمز الباركود
            .ForMember(dest => dest.BarcodeValue, opt => opt.MapFrom(src => src.ProductBarCode != null ? src.ProductBarCode.BarCode : null))
            // ترجمة سبب التسوية للبند إلى التسمية العربية
            .ForMember(dest => dest.ReasonName, opt => opt.MapFrom(src =>
                src.Reason == StockAdjustmentReason.InventoryCount ? "جرد دوري" :
                src.Reason == StockAdjustmentReason.Damaged ? "بضاعة تالفة" :
                src.Reason == StockAdjustmentReason.Expired ? "بضاعة منتهية الصلاحية" :
                src.Reason == StockAdjustmentReason.InitialSetup ? "رصيد افتتاحي" :
                src.Reason == StockAdjustmentReason.Other ? "أخرى" : src.Reason.ToString()));

        // تكوين تحويل ناقل بيانات بند تسوية المخزون إلى كيان البند
        CreateMap<StockAdjustmentItemDto, StockAdjustmentItem>()
            // احتساب فارق الكمية تلقائياً (الكمية الفعلية - الكمية الدفترية)
            .ForMember(dest => dest.DifferenceQuantity, opt => opt.MapFrom(src => src.ActualQuantity - src.SystemQuantity));
    }
}
