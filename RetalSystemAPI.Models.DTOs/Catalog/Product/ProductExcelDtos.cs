using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

/// <summary>
/// أوضاع واستراتيجيات استيراد الأصناف.
/// </summary>
public enum ProductImportMode
{
    /// <summary>
    /// إضافة الأصناف الجديدة وتحديث الأصناف الحالية.
    /// </summary>
    Upsert = 0,

    /// <summary>
    /// إضافة الأصناف الجديدة فقط وتجاهل الأصناف الموجودة.
    /// </summary>
    InsertOnly = 1,

    /// <summary>
    /// تحديث أسعار وأرصدة الأصناف المسجلة فقط وتجاهل إضافة أصناف جديدة.
    /// </summary>
    UpdateExistingOnly = 2
}

/// <summary>
/// خيارات استيراد ملف Excel.
/// </summary>
public class ProductExcelImportOptionsDto
{
    /// <summary>
    /// استراتيجية الاستيراد (افتراضي: Upsert).
    /// </summary>
    public ProductImportMode ImportMode { get; set; } = ProductImportMode.Upsert;

    /// <summary>
    /// معرف صالة العرض الافتراضية لتهيئة رصيد الصالة.
    /// </summary>
    public Guid? DefaultShowroomWarehouseId { get; set; }

    /// <summary>
    /// معرف مخزن التخزين الافتراضي لتهيئة رصيد المخزن.
    /// </summary>
    public Guid? DefaultStorageWarehouseId { get; set; }

    /// <summary>
    /// إنشاء التصنيفات تلقائياً إذا لم تكن موجودة.
    /// </summary>
    public bool AutoCreateCategories { get; set; } = true;

    /// <summary>
    /// إنشاء الوحدات تلقائياً إذا لم تكن موجودة.
    /// </summary>
    public bool AutoCreateUnits { get; set; } = true;

    /// <summary>
    /// صرف أكواد وباركودات تسلسلية فريدة تلقائياً للأصناف التي ليس لها كود.
    /// </summary>
    public bool AutoGenerateMissingBarcodes { get; set; } = true;
}

/// <summary>
/// نموذج بيانات الصنف المستورد أو المصدر عبر أكسل (الورقة الأولى في الملف متعدد الشيتات).
/// </summary>
public class ProductExcelRowDto
{
    public Guid? Id { get; set; }
    public required string Name { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// نموذج بيانات الباركود المستورد أو المصدر عبر أكسل (الورقة الثانية في الملف متعدد الشيتات).
/// </summary>
public class BarcodeExcelRowDto
{
    public Guid? Id { get; set; }
    public Guid ProductId { get; set; }
    public required string BarCode { get; set; }
    public string? Title { get; set; }
}

/// <summary>
/// نموذج بيانات التصنيف المستورد أو المصدر عبر أكسل (الورقة الثالثة في الملف متعدد الشيتات).
/// </summary>
public class CategoryExcelRowDto
{
    public Guid? Id { get; set; }
    public required string Name { get; set; }
}

/// <summary>
/// نموذج بيانات سطر صنف في ملف الشيت الواحد المسطح (Single-Sheet Flat Row).
/// </summary>
public class SingleSheetProductRowDto
{
    public string? ItemCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BarCode { get; set; }
    public string? CategoryName { get; set; }
    public string? UnitName { get; set; }
    public int ConversionFactor { get; set; } = 1;
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int ShowroomQuantity { get; set; }
    public int StorageQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// حالة التحقق من صحة السطر في المعاينة.
/// </summary>
public enum RowValidationStatus
{
    Valid = 0,
    Warning = 1,
    Error = 2
}

/// <summary>
/// الإجراء المتوقع تنفيذه للسطر.
/// </summary>
public enum RowActionType
{
    Create = 0,
    Update = 1,
    Skip = 2
}

/// <summary>
/// نتيجة فحص والتحقق من سطر واحد في المعاينة.
/// </summary>
public class ProductExcelRowValidationDto
{
    public int RowNumber { get; set; }
    public string? ItemCode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? BarCode { get; set; }
    public string? CategoryName { get; set; }
    public string? UnitName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int ShowroomQuantity { get; set; }
    public int StorageQuantity { get; set; }
    public RowValidationStatus Status { get; set; } = RowValidationStatus.Valid;
    public RowActionType Action { get; set; } = RowActionType.Create;
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
/// نتيجة الفحص والمعاينة المسبقة لملف Excel قبل الحفظ (Dry-Run Preview).
/// </summary>
public class ProductExcelValidationResultDto
{
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int NewProductsCount { get; set; }
    public int UpdatedProductsCount { get; set; }
    public int NewCategoriesCount { get; set; }
    public int NewBarcodesCount { get; set; }
    public int AutoGeneratedBarcodesCount { get; set; }
    public int MissingBarcodesCount { get; set; }
    public bool IsSingleSheetFormat { get; set; } = true;
    public List<ProductExcelRowValidationDto> Rows { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool IsValid => InvalidRows == 0 && Errors.Count == 0;
}

/// <summary>
/// تفاصيل سطر فشل استيراده لتصديره إلى Excel.
/// </summary>
public class FailedRowDetailsDto
{
    public int RowNumber { get; set; }
    public string? ItemCode { get; set; }
    public string? Name { get; set; }
    public string? BarCode { get; set; }
    public string? CategoryName { get; set; }
    public string? UnitName { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public int ShowroomQuantity { get; set; }
    public int StorageQuantity { get; set; }
    public string ErrorReason { get; set; } = string.Empty;
}

/// <summary>
/// نتيجة عملية استيراد الأصناف من ملف أكسل.
/// </summary>
public class ProductImportResultDto
{
    public int CategoriesImported { get; set; }
    public int ProductsImported { get; set; }
    public int BarcodesImported { get; set; }
    public int AutoGeneratedBarcodesCount { get; set; }
    public int UnitsImported { get; set; }
    public int StocksInitialized { get; set; }
    public int FailedRowsCount => FailedRows.Count;
    public List<FailedRowDetailsDto> FailedRows { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public bool IsSuccess => Errors.Count == 0 && FailedRows.Count == 0;
}
