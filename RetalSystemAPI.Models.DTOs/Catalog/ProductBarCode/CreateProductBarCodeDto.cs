using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;

public class CreateProductBarCodeDto
{
    [Required(ErrorMessage = "الباركود مطلوب")]
    public string BarCode { get; set; } = null!;

    [Required(ErrorMessage = "عنوان الباركود مطلوب")]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }
}
