using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Desktop.Models.Catalog;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryDto> Children { get; set; } = new();
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
}

public class UnitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
}

public class CreateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
}

public class UpdateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Symbol { get; set; }
}

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public decimal AveragePrice { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? DefaultImageUrl { get; set; }
    public string? DefaultImage { get; set; }
    public string? DefaultBarCode { get; set; }
    public int ShowroomQuantity { get; set; }
    public List<ProductUnitDto> Units { get; set; } = new();
    public List<ProductBarCodeDto> BarCodes { get; set; } = new();
    public List<ProductImageDto> Images { get; set; } = new();

    public string? DisplayImageUrl => !string.IsNullOrWhiteSpace(DefaultImageUrl)
        ? (DefaultImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? DefaultImageUrl : $"https://localhost:7226/{DefaultImageUrl.TrimStart('/')}")
        : (!string.IsNullOrWhiteSpace(DefaultImage)
            ? (DefaultImage.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? DefaultImage : $"https://localhost:7226/{DefaultImage.TrimStart('/')}")
            : (Images?.FirstOrDefault(i => i.IsDefault)?.FullImageUrl ?? Images?.FirstOrDefault()?.FullImageUrl));
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public Guid CategoryId { get; set; }
    public int InitialShowroomQuantity { get; set; } = 0;
    public Guid? ShowroomWarehouseId { get; set; }
    public Guid? StorageWarehouseId { get; set; }
    public List<CreateShowroomStockQuantityRequest> ShowroomInitialQuantities { get; set; } = new();
    public List<CreateStorageStockQuantityRequest> StorageInitialQuantities { get; set; } = new();
    public List<CreateProductBarCodeRequest> BarCodes { get; set; } = new();
    public List<CreateProductUnitRequest> Units { get; set; } = new();
}

public class CreateShowroomStockQuantityRequest
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class CreateStorageStockQuantityRequest
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string BarCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }
    public Guid CategoryId { get; set; }
    public List<CreateProductBarCodeRequest> BarCodes { get; set; } = new();
    public List<CreateProductUnitRequest> Units { get; set; } = new();
}

public class ProductUnitDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int ConversionFactor { get; set; }
    public bool IsDefault { get; set; }
}

public class CreateProductUnitRequest
{
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public int ConversionFactor { get; set; } = 1;
    public bool IsDefault { get; set; }
}

public class ProductBarCodeDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string BarCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal CostPrice { get; set; }
}

public class CreateProductBarCodeRequest
{
    public string BarCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int InitialQuantity { get; set; } = 0;
}

public class UpdateProductBarCodeRequest
{
    public string BarCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// صورة معلقة مرتبطة بباركود قبل حفظ المنتج الجديد؛ تُرفع للخادم بعد إنشاء المنتج
/// مع إعادة ربط معرف الباركود المؤقت بالمعرف الحقيقي الصادر من الخادم.
/// </summary>
public class PendingProductImage
{
    public Guid TempBarcodeId { get; set; }
    public string BarCode { get; set; } = string.Empty;
    public string BarcodeTitle { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string FileName => string.IsNullOrWhiteSpace(FilePath) ? string.Empty : System.IO.Path.GetFileName(FilePath);
}

public class ProductImageDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public Guid? BarcodeId { get; set; }
    public string BarcodeTitle { get; set; } = string.Empty;

    public string FullImageUrl => string.IsNullOrWhiteSpace(ImageUrl)
        ? string.Empty
        : (ImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? ImageUrl
            : $"https://localhost:7226/{ImageUrl.TrimStart('/')}");
}

public enum ProductImportMode
{
    Upsert = 0,
    InsertOnly = 1,
    UpdateExistingOnly = 2
}

public class ProductExcelImportOptionsDto
{
    public ProductImportMode ImportMode { get; set; } = ProductImportMode.Upsert;
    public Guid? DefaultShowroomWarehouseId { get; set; }
    public Guid? DefaultStorageWarehouseId { get; set; }
    public bool AutoCreateCategories { get; set; } = true;
    public bool AutoCreateUnits { get; set; } = true;
    public bool AutoGenerateMissingBarcodes { get; set; } = true;
}

public enum RowValidationStatus
{
    Valid = 0,
    Warning = 1,
    Error = 2
}

public enum RowActionType
{
    Create = 0,
    Update = 1,
    Skip = 2
}

public partial class ProductExcelRowValidationDto : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    private int _rowNumber;
    private string? _itemCode;
    private string _name = string.Empty;
    private string? _barCode;
    private string? _categoryName;
    private string? _unitName;
    private decimal _costPrice;
    private decimal _salePrice;
    private int _showroomQuantity;
    private int _storageQuantity;
    private RowValidationStatus _status = RowValidationStatus.Valid;
    private RowActionType _action = RowActionType.Create;
    private string? _message;
    private bool _isAutoBarcode;
    private bool _isCustomBarcode;
    private string? _originalBarCode;

    public int RowNumber { get => _rowNumber; set => SetProperty(ref _rowNumber, value); }
    public string? ItemCode { get => _itemCode; set => SetProperty(ref _itemCode, value); }
    public string Name { get => _name; set => SetProperty(ref _name, value); }
    public string? BarCode
    {
        get => _barCode;
        set
        {
            if (SetProperty(ref _barCode, value))
            {
                OnPropertyChanged(nameof(BarcodeDisplayBadgeColor));
                OnPropertyChanged(nameof(BarcodeTypeBadgeText));
            }
        }
    }
    public string? CategoryName { get => _categoryName; set => SetProperty(ref _categoryName, value); }
    public string? UnitName { get => _unitName; set => SetProperty(ref _unitName, value); }
    public decimal CostPrice { get => _costPrice; set => SetProperty(ref _costPrice, value); }
    public decimal SalePrice { get => _salePrice; set => SetProperty(ref _salePrice, value); }
    public int ShowroomQuantity { get => _showroomQuantity; set => SetProperty(ref _showroomQuantity, value); }
    public int StorageQuantity { get => _storageQuantity; set => SetProperty(ref _storageQuantity, value); }
    public RowValidationStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(ref _status, value))
            {
                OnPropertyChanged(nameof(StatusBadgeColor));
            }
        }
    }
    public RowActionType Action
    {
        get => _action;
        set
        {
            if (SetProperty(ref _action, value))
            {
                OnPropertyChanged(nameof(ActionBadgeColor));
                OnPropertyChanged(nameof(ActionText));
            }
        }
    }
    public string? Message { get => _message; set => SetProperty(ref _message, value); }
    public bool IsAutoBarcode
    {
        get => _isAutoBarcode;
        set
        {
            if (SetProperty(ref _isAutoBarcode, value))
            {
                OnPropertyChanged(nameof(BarcodeDisplayBadgeColor));
                OnPropertyChanged(nameof(BarcodeTypeBadgeText));
            }
        }
    }
    public bool IsCustomBarcode
    {
        get => _isCustomBarcode;
        set
        {
            if (SetProperty(ref _isCustomBarcode, value))
            {
                OnPropertyChanged(nameof(BarcodeDisplayBadgeColor));
                OnPropertyChanged(nameof(BarcodeTypeBadgeText));
            }
        }
    }
    public string? OriginalBarCode { get => _originalBarCode; set => SetProperty(ref _originalBarCode, value); }

    public string StatusBadgeColor => Status switch
    {
        RowValidationStatus.Valid => "#10B981", // Green
        RowValidationStatus.Warning => "#F59E0B", // Amber
        RowValidationStatus.Error => "#EF4444", // Red
        _ => "#6B7280"
    };

    public string ActionBadgeColor => Action switch
    {
        RowActionType.Create => "#3B82F6", // Blue (New)
        RowActionType.Update => "#8B5CF6", // Purple (Update)
        RowActionType.Skip => "#6B7280", // Gray (Skip)
        _ => "#6B7280"
    };

    public string ActionText => Action switch
    {
        RowActionType.Create => "إضافة جديد",
        RowActionType.Update => "تحديث صنف",
        RowActionType.Skip => "تجاهل",
        _ => "-"
    };

    public string BarcodeDisplayBadgeColor
    {
        get
        {
            if (IsCustomBarcode) return "#10B981"; // Green (Custom manual)
            if (IsAutoBarcode || BarCode == "(صرف كود تلقائي)" || (BarCode != null && BarCode.Contains("تلقائي"))) return "#F59E0B"; // Amber (Auto)
            if (string.IsNullOrWhiteSpace(BarCode)) return "#EF4444"; // Red (Missing)
            return "#3B82F6"; // Blue (Original from file)
        }
    }

    public string BarcodeTypeBadgeText
    {
        get
        {
            if (IsCustomBarcode) return "يدوي";
            if (IsAutoBarcode || BarCode == "(صرف كود تلقائي)" || (BarCode != null && BarCode.Contains("تلقائي"))) return "تلقائي";
            if (string.IsNullOrWhiteSpace(BarCode)) return "بدون كود";
            return "من الملف";
        }
    }
}

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
