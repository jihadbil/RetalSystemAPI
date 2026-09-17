using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;

/// <summary>
/// ناقل بيانات تعديل باركود منتج حالي (Update Product Barcode DTO).
/// </summary>
public class UpdateProductBarCodeDto
{
    /// <summary>رمز الباركود الجديد</summary>
    [Required(ErrorMessage = "الباركود مطلوب")]
    public string BarCode { get; set; } = null!;

    /// <summary>عنوان الباركود الجديد</summary>
    [Required(ErrorMessage = "عنوان الباركود مطلوب")]
    public string Title { get; set; } = null!;

    /// <summary>وصف الباركود</summary>
    public string? Description { get; set; }
}
