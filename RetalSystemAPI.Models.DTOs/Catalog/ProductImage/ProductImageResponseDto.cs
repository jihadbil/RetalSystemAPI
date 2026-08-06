using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductImage;

public class ProductImageResponseDto : BaseDto
{
    public string ImageUrl { get; set; } = null!;
    public bool IsDefault { get; set; }
    public Guid ProductId { get; set; }
    public Guid? BarcodeId { get; set; }
}
