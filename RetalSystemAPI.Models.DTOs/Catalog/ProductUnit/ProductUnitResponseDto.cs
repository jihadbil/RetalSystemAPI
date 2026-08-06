using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

public class ProductUnitResponseDto : BaseDto
{
    public Guid ProductId { get; set; }
    public Guid UnitId { get; set; }
    public string UnitName { get; set; } = null!;
    public int ConversionFactor { get; set; }
    public bool IsDefault { get; set; }
}
