using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;

public class ProductBarCodeResponseDto : BaseDto
{
    public string BarCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public Guid ProductId { get; set; }
    public List<ProductImageResponseDto> Images { get; set; } = new();
}
