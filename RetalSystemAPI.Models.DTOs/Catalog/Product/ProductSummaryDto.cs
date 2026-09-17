using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

/// <summary>
/// ناقل بيانات ملخص المنتج السريع (Product Summary DTO).
/// خفيف ومخصص لعرض المنتجات في الجداول وقوائم البحث وشاشات الكاشير ونقاط البيع.
/// </summary>
public class ProductSummaryDto : BaseDto
{
    /// <summary>اسم المنتج</summary>
    public string Name { get; set; } = null!;

    /// <summary>كود الصنف الداخلي</summary>
    public string Code { get; set; } = null!;

    /// <summary>وصف مختصر للمنتج</summary>
    public string? Description { get; set; }

    /// <summary>معرف التصنيف</summary>
    public Guid CategoryId { get; set; }

    /// <summary>اسم التصنيف</summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>سعر البيع</summary>
    public decimal SalePrice { get; set; }

    /// <summary>سعر التكلفة</summary>
    public decimal CostPrice { get; set; }

    /// <summary>متوسط سعر التكلفة</summary>
    public decimal AveragePrice { get; set; }

    /// <summary>رابط الصورة الافتراضية الرئيسية للمنتج</summary>
    public string? DefaultImage { get; set; }

    /// <summary>رمز الباركود الافتراضي الأساسي للمنتج</summary>
    public string? DefaultBarCode { get; set; }

    /// <summary>قائمة الباركودات</summary>
    public List<ProductBarCodeResponseDto> BarCodes { get; set; } = new();

    /// <summary>قائمة الوحدات</summary>
    public List<ProductUnitResponseDto> Units { get; set; } = new();

    /// <summary>قائمة الصور</summary>
    public List<ProductImageResponseDto> Images { get; set; } = new();
}
