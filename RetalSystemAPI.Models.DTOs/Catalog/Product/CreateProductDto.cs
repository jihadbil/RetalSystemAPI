using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

public class CreateProductDto
{
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

    public List<CreateProductBarCodeDto> BarCodes { get; set; } = new();
    public List<CreateProductUnitDto> Units { get; set; } = new();
}
