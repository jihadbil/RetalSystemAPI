using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductImage;

public class CreateProductImageDto
{
    [Required(ErrorMessage = "رابط الصورة مطلوب")]
    public string ImageUrl { get; set; } = null!;

    public bool IsDefault { get; set; } = false;

    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    public Guid? BarcodeId { get; set; }
}
