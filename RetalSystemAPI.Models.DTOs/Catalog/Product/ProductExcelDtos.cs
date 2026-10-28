using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

/// <summary>
/// أوضاع واستراتيجيات استيراد الأصناف من ملف الإكسل.
/// </summary>
public enum ProductImportMode
{
    /// <summary>
    /// إضافة الأصناف الجديدة وتحديث الأصناف الحالية (Upsert).
    /// </summary>
    Upsert = 0,

    /// <summary>
    /// إضافة الأصناف الجديدة فقط وتجاهل الأصناف الموجودة مسبقاً (Insert Only).
    /// </summary>
    InsertOnly = 1,

    /// <summary>
    /// تحديث أسعار وأرصدة الأصناف المسجلة فقط وتجاهل إضافة أي صنف جديد (Update Existing Only).
    /// </summary>
    UpdateExistingOnly = 2
}

/// <summary>
/// خيارات وتفضيلات استيراد ملف الأصناف من Excel.
/// </summary>
public class ProductExcelImportOptionsDto
{
    /// <summary>
    /// استراتيجية الاستيراد (افتراضي: Upsert).
    /// </summary>
    public ProductImportMode ImportMode { get; set; } = ProductImportMode.Upsert;

    /// <summary>
    /// معرف صالة العرض الافتراضية لتهيئة رصيد الصالة للأصناف المستوردة.
    /// </summary>
    public Guid? DefaultShowroomWarehouseId { get; set; }

    /// <summary>
    /// معرف مخزن التخزين الافتراضي لتهيئة رصيد المخزن للأصناف المستوردة.
    /// </summary>
    public Guid? DefaultStorageWarehouseId { get; set; }

    /// <summary>
    /// إنشاء التصنيفات تلقائياً إذا لم تكن مسجلة مسبقاً في النظام.
    /// </summary>
    public bool AutoCreateCategories { get; set; } = true;

    /// <summary>
    /// إنشاء الوحدات تلقائياً إذا لم تكن موجودة مسبقاً في النظام.
    /// </summary>
    public bool AutoCreateUnits { get; set; } = true;

    /// <summary>
    /// صرف أكواد وباركودات تسلسلية فريدة تلقائياً للأصناف التي لا تمتلك باركود.
    /// </summary>
    public bool AutoGenerateMissingBarcodes { get; set; } = true;
}

/// <summary>
/// نموذج بيانات الصنف المستورد أو المصدر عبر أكسل (الورقة الأولى في الملف متعدد الشيتات).
/// </summary>
public class ProductExcelRowDto
{
    /// <summary>المعرف الفريد للصنف (اختياري، في حالة التحديث بالمعرف)</summary>
    public Guid? Id { get; set; }

    /// <summary>اسم الصنف التجاري</summary>
    public required string Name { get; set; }

    /// <summary>سعر تكلفة شراء الوحدة</summary>
    public decimal CostPrice { get; set; }

    /// <summary>سعر بيع الوحدة للجمهور</summary>
    public decimal SalePrice { get; set; }

    /// <summary>معرف تصنيف الصنف إن وجد</summary>
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// نموذج بيانات الباركود المستورد أو المصدر عبر أكسل (الورقة الثانية في الملف متعدد الشيتات).
/// </summary>
public class BarcodeExcelRowDto
{
    /// <summary>المعرف الفريد للباركود</summary>
    public Guid? Id { get; set; }

    /// <summary>معرف الصنف المرتبط به هذا الباركود</summary>
    public Guid ProductId { get; set; }

    /// <summary>قيمة الباركود النصية أو الرقمية</summary>
    public required string BarCode { get; set; }

    /// <summary>وصف أو عنوان الباركود (باركود العبوة، باركود الشركة، إلخ)</summary>
    public string? Title { get; set; }
}

/// <summary>
/// نموذج بيانات التصنيف المستورد أو المصدر عبر أكسل (الورقة الثالثة في الملف متعدد الشيتات).
/// </summary>
public class CategoryExcelRowDto
{
    /// <summary>المعرف الفريد للتصنيف إن وجد</summary>
    public Guid? Id { get; set; }

    /// <summary>اسم التصنيف أو المجموعة السلعية</summary>
    public required string Name { get; set; }
}

/// <summary>
/// نموذج بيانات سطر صنف في ملف الشيت الواحد المسطح (Single-Sheet Flat Row).
/// </summary>
public class SingleSheetProductRowDto
{
    /// <summary>كود الصنف الداخلي للمنشأة</summary>
    public string? ItemCode { get; set; }

    /// <summary>اسم الصنف</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>باركود الصنف الدولي أو المحلي</summary>
    public string? BarCode { get; set; }

    /// <summary>اسم التصنيف التابع له الصنف</summary>
    public string? CategoryName { get; set; }

    /// <summary>اسم وحدة القياس الأساسية (قطعة، كيلو، كرتون، إلخ)</summary>
    public string? UnitName { get; set; }

    /// <summary>معامل التحويل للوحدة (افتراضي 1)</summary>
    public int ConversionFactor { get; set; } = 1;

    /// <summary>سعر التكلفة</summary>
    public decimal CostPrice { get; set; }

    /// <summary>سعر البيع الافتراضي</summary>
    public decimal SalePrice { get; set; }

    /// <summary>رصيد المخزون الافتتاحي في صالة العرض</summary>
    public int ShowroomQuantity { get; set; }

    /// <summary>رصيد المخزون الافتتاحي في مستودع التخزين</summary>
    public int StorageQuantity { get; set; }

    /// <summary>الحد الأدنى لرصيد الصنف للتنبيه قبل النفاذ</summary>
    public int MinStockLevel { get; set; }

    /// <summary>الوصف التفصيلي أو مواصفات الصنف</summary>
    public string? Description { get; set; }
}

/// <summary>
/// حالة التحقق من صحة سطر البيانات في مرحلة المعاينة المسبقة.
/// </summary>
public enum RowValidationStatus
{
    /// <summary>السطر سليم وجاهز للتنفيذ دون أي ملاحظات</summary>
    Valid = 0,

    /// <summary>السطر يحتوي تحذيراً غير مانع للاستيراد (مثل توليد باركود تلقائي)</summary>
    Warning = 1,

    /// <summary>السطر يحتوي خطأ يمنع استيراده (مثل نقص حقل إجباري)</summary>
    Error = 2
}

/// <summary>
/// الإجراء الفعلي المتوقع تنفيذه للسطر بناءً على فحص البيانات واستراتيجية الاستيراد.
/// </summary>
public enum RowActionType
{
    /// <summary>إنشاء صنف جديد تماماً</summary>
    Create = 0,

    /// <summary>تحديث بيانات وأسعار صنف موجود مسبقاً</summary>
    Update = 1,

    /// <summary>تخطي وتجاهل السطر وفق استراتيجية الاستيراد المختارة</summary>
    Skip = 2
}

/// <summary>
/// نتيجة فحص والتحقق من سطر واحد في المعاينة المسبقة لملف الإكسل.
/// </summary>
public class ProductExcelRowValidationDto
{
    /// <summary>رقم السطر في ملف Excel</summary>
    public int RowNumber { get; set; }

    /// <summary>كود الصنف المستخرج من الملف</summary>
    public string? ItemCode { get; set; }

    /// <summary>اسم الصنف</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>الباركود المفحوص أو المولد</summary>
    public string? BarCode { get; set; }

    /// <summary>اسم التصنيف</summary>
    public string? CategoryName { get; set; }

    /// <summary>اسم الوحدة</summary>
    public string? UnitName { get; set; }

    /// <summary>سعر التكلفة</summary>
    public decimal CostPrice { get; set; }

    /// <summary>سعر البيع</summary>
    public decimal SalePrice { get; set; }

    /// <summary>كمية صالة العرض</summary>
    public int ShowroomQuantity { get; set; }

    /// <summary>كمية المخزن الرئيسي</summary>
    public int StorageQuantity { get; set; }

    /// <summary>حالة التحقق من السطر (سليم، تحذير، خطأ)</summary>
    public RowValidationStatus Status { get; set; } = RowValidationStatus.Valid;

    /// <summary>الإجراء المقرر اتخاذه للصنف (إنشاء، تحديث، تخطي)</summary>
    public RowActionType Action { get; set; } = RowActionType.Create;

    /// <summary>رسالة الشرح أو تفاصيل الخطأ/التحذير في السطر</summary>
    public string? Message { get; set; }

    /// <summary>
    /// هل تم صرف هذا الكود تلقائياً بواسطة النظام.
    /// </summary>
    public bool IsAutoBarcode { get; set; }

    /// <summary>
    /// هل تم تحديد أو تعديل هذا الكود يدوياً بواسطة المستخدم.
    /// </summary>
    public bool IsCustomBarcode { get; set; }

    /// <summary>
    /// الباركود الأصلي في ملف الإكسل إن وجد.
    /// </summary>
    public string? OriginalBarCode { get; set; }
}

/// <summary>
/// نتيجة الفحص والمعاينة المسبقة لملف Excel قبل الحفظ الفعلي (Dry-Run Preview).
/// </summary>
public class ProductExcelValidationResultDto
{
    /// <summary>إجمالي عدد الأسطر المفحوصة في الملف</summary>
    public int TotalRows { get; set; }

    /// <summary>عدد الأسطر الصالحة والمعتمدة للاستيراد</summary>
    public int ValidRows { get; set; }

    /// <summary>عدد الأسطر المعيبة التي تحتوي أخطاء تمنع استيرادها</summary>
    public int InvalidRows { get; set; }

    /// <summary>عدد الأصناف الجديدة المقرر إنشاؤها</summary>
    public int NewProductsCount { get; set; }

    /// <summary>عدد الأصناف الموجودة المقرر تحديث بياناتها</summary>
    public int UpdatedProductsCount { get; set; }

    /// <summary>عدد التصنيفات الجديدة المقرر إضافتها تلقائياً</summary>
    public int NewCategoriesCount { get; set; }

    /// <summary>عدد الباركودات الجديدة المقرر ربطها</summary>
    public int NewBarcodesCount { get; set; }

    /// <summary>عدد الباركودات التي ولدت تلقائياً للأصناف التي ليس لها باركود</summary>
    public int AutoGeneratedBarcodesCount { get; set; }

    /// <summary>عدد الأصناف التي لا تمتلك باركود</summary>
    public int MissingBarcodesCount { get; set; }

    /// <summary>هل نسق الملف شيت واحد مسطح أم متعدد الشيتات</summary>
    public bool IsSingleSheetFormat { get; set; } = true;

    /// <summary>قائمة نتائج فحص الأسطر الفردية بالتفصيل</summary>
    public List<ProductExcelRowValidationDto> Rows { get; set; } = new();

    /// <summary>قائمة التنبيهات والتحذيرات العامة حول الملف</summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>قائمة الأخطاء العامة المكتشفة التي قد تعيق الاستيراد</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>مؤشر هل الملف صالح بالكامل للاستيراد دون أي أخطاء مانعة</summary>
    public bool IsValid => InvalidRows == 0 && Errors.Count == 0;
}

/// <summary>
/// تفاصيل سطر فشل استيراده لتصديره إلى Excel لتمكين المستخدم من تصحيحه.
/// </summary>
public class FailedRowDetailsDto
{
    /// <summary>رقم السطر في ملف الإكسل الأصلي</summary>
    public int RowNumber { get; set; }

    /// <summary>كود الصنف</summary>
    public string? ItemCode { get; set; }

    /// <summary>اسم الصنف</summary>
    public string? Name { get; set; }

    /// <summary>الباركود</summary>
    public string? BarCode { get; set; }

    /// <summary>اسم التصنيف</summary>
    public string? CategoryName { get; set; }

    /// <summary>اسم الوحدة</summary>
    public string? UnitName { get; set; }

    /// <summary>سعر التكلفة</summary>
    public decimal CostPrice { get; set; }

    /// <summary>سعر البيع</summary>
    public decimal SalePrice { get; set; }

    /// <summary>كمية صالة العرض</summary>
    public int ShowroomQuantity { get; set; }

    /// <summary>كمية المخزن</summary>
    public int StorageQuantity { get; set; }

    /// <summary>السبب التفصيلي لفشل استيراد هذا السطر</summary>
    public string ErrorReason { get; set; } = string.Empty;
}

/// <summary>
/// نتيجة إتمام عملية استيراد الأصناف من ملف الإكسل وإحصائياتها.
/// </summary>
public class ProductImportResultDto
{
    /// <summary>عدد التصنيفات المستوردة أو المحدثة</summary>
    public int CategoriesImported { get; set; }

    /// <summary>عدد الأصناف التي تم استيرادها وحفظها بنجاح</summary>
    public int ProductsImported { get; set; }

    /// <summary>عدد الباركودات المسجلة</summary>
    public int BarcodesImported { get; set; }

    /// <summary>عدد الباركودات التي ولدها النظام تلقائياً</summary>
    public int AutoGeneratedBarcodesCount { get; set; }

    /// <summary>عدد وحدات القياس المستوردة</summary>
    public int UnitsImported { get; set; }

    /// <summary>عدد سجلات المخزون التي تم تهيئة أرصدتها الافتتاحية</summary>
    public int StocksInitialized { get; set; }

    /// <summary>إجمالي عدد الأسطر التي فشل استيرادها</summary>
    public int FailedRowsCount => FailedRows.Count;

    /// <summary>قائمة تفاصيل الأسطر الفاشلة لتسهيل إعادة تصديرها ومعالجتها</summary>
    public List<FailedRowDetailsDto> FailedRows { get; set; } = new();

    /// <summary>قائمة التنبيهات المسجلة أثناء المعالجة</summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>قائمة الأخطاء الحرجة إن وجدت</summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>مؤشر هل تمت عملية الاستيراد بالكامل بنجاح تام</summary>
    public bool IsSuccess => Errors.Count == 0 && FailedRows.Count == 0;
}
