using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;

/// <summary>
/// ناقل بيانات إنشاء باركود جديد لمنتج (Create Product Barcode DTO).
/// </summary>
public class CreateProductBarCodeDto
{
    /// <summary>رمز الباركود الفريد</summary>
    [Required(ErrorMessage = "الباركود مطلوب")]
    public string BarCode { get; set; } = null!;

    /// <summary>عنوان أو مسمى الباركود (مثل: حبة، كرتونة، لون معين)</summary>
    [Required(ErrorMessage = "عنوان الباركود مطلوب")]
    public string Title { get; set; } = null!;

    /// <summary>وصف إضافي اختياري للباركود</summary>
    public string? Description { get; set; }

    /// <summary>معرف المنتج المراد ربط الباركود به</summary>
    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>الكمية المبدئية المرتبطة بهذا الباركود</summary>
    [Range(0, int.MaxValue, ErrorMessage = "الكمية المبدئية يجب أن تكون أكبر من أو تساوي 0")]
    public int InitialQuantity { get; set; } = 0;
}
