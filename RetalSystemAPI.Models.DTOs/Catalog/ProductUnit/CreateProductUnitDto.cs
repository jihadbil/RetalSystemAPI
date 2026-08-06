using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

public class CreateProductUnitDto
{
    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "معرف الوحدة مطلوب")]
    public Guid UnitId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "معامل التحويل يجب أن يكون 1 على الأقل")]
    public int ConversionFactor { get; set; }

    public bool IsDefault { get; set; } = false;
}
