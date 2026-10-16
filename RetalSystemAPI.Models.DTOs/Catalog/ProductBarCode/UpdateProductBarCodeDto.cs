using RetalSystemAPI.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;

/// <summary>
/// ناقل بيانات تعديل باركود منتج حالي (Update Product Barcode DTO).
/// </summary>
public class UpdateProductBarCodeDto
{
    /// <summary>رمز الباركود الجديد</summary>
    [Required(ErrorMessage = "الباركود مطلوب")]
    [MaxLength(100)]
    public string BarCode { get; set; } = null!;

    /// <summary>عنوان الباركود الجديد</summary>
    [Required(ErrorMessage = "عنوان الباركود مطلوب")]
    [MaxLength(300)]
    public string Title { get; set; } = null!;

    /// <summary>وصف الباركود</summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
}
