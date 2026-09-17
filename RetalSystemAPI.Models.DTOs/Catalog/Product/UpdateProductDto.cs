using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

/// <summary>
/// ناقل بيانات تعديل منتج قائم (Update Product DTO).
/// يُستخدم لتحديث بيانات المنتج، الأسعار، التصنيف، وقوائم الباركودات والوحدات التابعة له.
/// </summary>
public class UpdateProductDto
{
    /// <summary>المعرف الفريد للمنتج المطلوب تحديثه</summary>
    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid Id { get; set; }

    /// <summary>اسم المنتج</summary>
    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [MaxLength(300, ErrorMessage = "اسم المنتج يجب أن لا يتجاوز 300 حرف")]
    public string Name { get; set; } = null!;

    /// <summary>الوصف التفصيلي</summary>
    public string? Description { get; set; }

    /// <summary>سعر التكلفة الجديد</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سعر التكلفة يجب أن يكون أكبر من أو يساوي 0")]
    public decimal CostPrice { get; set; }

    /// <summary>سعر البيع الجديد</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون أكبر من أو يساوي 0")]
    public decimal SalePrice { get; set; }

    /// <summary>معرف التصنيف</summary>
    [Required(ErrorMessage = "معرف التصنيف مطلوب")]
    public Guid CategoryId { get; set; }

    /// <summary>قائمة الباركودات المحدثة</summary>
    public List<CreateProductBarCodeDto> BarCodes { get; set; } = new();

    /// <summary>قائمة الوحدات المحدثة</summary>
    public List<CreateProductUnitDto> Units { get; set; } = new();
}
