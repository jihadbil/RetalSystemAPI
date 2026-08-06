using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

public class UpdateProductDto
{
    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [MaxLength(300, ErrorMessage = "اسم المنتج يجب أن لا يتجاوز 300 حرف")]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "سعر التكلفة يجب أن يكون أكبر من أو يساوي 0")]
    public decimal CostPrice { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "سعر البيع يجب أن يكون أكبر من أو يساوي 0")]
    public decimal SalePrice { get; set; }

    [Required(ErrorMessage = "معرف التصنيف مطلوب")]
    public Guid CategoryId { get; set; }
}
