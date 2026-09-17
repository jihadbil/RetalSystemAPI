using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductImage;

/// <summary>
/// ناقل بيانات استجابة صورة المنتج (Product Image Response DTO).
/// </summary>
public class ProductImageResponseDto : BaseDto
{
    /// <summary>رابط أو مسار الصورة</summary>
    public string ImageUrl { get; set; } = null!;

    /// <summary>هل هي الصورة الافتراضية</summary>
    public bool IsDefault { get; set; }

    /// <summary>معرف المنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>معرف الباركود المرتبطة به إن وجد</summary>
    public Guid? BarcodeId { get; set; }
}
