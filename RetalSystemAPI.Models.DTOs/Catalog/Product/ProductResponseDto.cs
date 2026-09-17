using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

/// <summary>
/// ناقل بيانات استجابة تفاصيل المنتج الشاملة (Product Response DTO).
/// يعرض الأسعار، متوسط التكلفة، التصنيف، والوحدات والباركودات والصور المرتبطة بالمنتج.
/// </summary>
public class ProductResponseDto : BaseDto
{
    /// <summary>اسم المنتج</summary>
    public string Name { get; set; } = null!;

    /// <summary>الوصف التفصيلي للمنتج</summary>
    public string? Description { get; set; }

    /// <summary>سعر التكلفة الأخير</summary>
    public decimal CostPrice { get; set; }

    /// <summary>سعر البيع المعتمد</summary>
    public decimal SalePrice { get; set; }

    /// <summary>متوسط سعر التكلفة المرجح (Moving Average Cost)</summary>
    public decimal AveragePrice { get; set; }

    /// <summary>معرف التصنيف التابع له</summary>
    public Guid CategoryId { get; set; }

    /// <summary>اسم التصنيف</summary>
    public string CategoryName { get; set; } = null!;

    /// <summary>قائمة وحدات القياس المتاحة للمنتج</summary>
    public List<ProductUnitResponseDto> Units { get; set; } = new();

    /// <summary>قائمة الباركودات المسجلة للمنتج</summary>
    public List<ProductBarCodeResponseDto> BarCodes { get; set; } = new();

    /// <summary>قائمة الصور المرفوعة للمنتج</summary>
    public List<ProductImageResponseDto> Images { get; set; } = new();
}
