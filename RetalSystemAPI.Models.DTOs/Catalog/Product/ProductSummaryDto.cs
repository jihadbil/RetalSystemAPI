using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.DTOs.Catalog.ProductImage;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

public class ProductSummaryDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public decimal AveragePrice { get; set; }
    public string? DefaultImage { get; set; }
    public string? DefaultBarCode { get; set; }
    public List<ProductBarCodeResponseDto> BarCodes { get; set; } = new();
    public List<ProductUnitResponseDto> Units { get; set; } = new();
    public List<ProductImageResponseDto> Images { get; set; } = new();
}
