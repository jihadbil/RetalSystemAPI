using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;

/// <summary>
/// ناقل بيانات استجابة باركود المنتج (Product Barcode Response DTO).
/// </summary>
public class ProductBarCodeResponseDto : BaseDto
{
    /// <summary>رمز الباركود</summary>
    public string BarCode { get; set; } = null!;

    /// <summary>عنوان ومسمى الباركود</summary>
    public string Title { get; set; } = null!;

    /// <summary>الوصف الإضافي</summary>
    public string? Description { get; set; }

    /// <summary>المعرف الفريد للمنتج التابع له</summary>
    public Guid ProductId { get; set; }

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>سعر تكلفة الصنف</summary>
    public decimal CostPrice { get; set; }

    /// <summary>قائمة الصور المرتبطة خصيصاً بهذا الباركود</summary>
    public List<ProductImageResponseDto> Images { get; set; } = new();
}
