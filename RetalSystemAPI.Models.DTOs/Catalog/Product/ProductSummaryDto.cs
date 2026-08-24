using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.Product;

public class ProductSummaryDto : BaseDto
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public decimal SalePrice { get; set; }
    public decimal CostPrice { get; set; }
    public string? DefaultImage { get; set; }
    public string? DefaultBarCode { get; set; }
}
