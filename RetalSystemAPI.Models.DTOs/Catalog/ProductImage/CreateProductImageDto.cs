using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductImage;

/// <summary>
/// ناقل بيانات إضافة صورة جديدة لمنتج أو باركود (Create Product Image DTO).
/// </summary>
public class CreateProductImageDto
{
    /// <summary>مسار أو رابط الصورة المرفوعة</summary>
    [Required(ErrorMessage = "رابط الصورة مطلوب")]
    public string ImageUrl { get; set; } = null!;

    /// <summary>هل تُعتبر هذه الصورة هي الصورة الافتراضية الرئيسية للمنتج</summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>المعرف الفريد للمنتج المراد ربط الصورة به</summary>
    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>المعرف الفريد للباركود المحدد في حال كانت الصورة خاصة بعبوة أو لون معين</summary>
    public Guid? BarcodeId { get; set; }
}
