using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;

public class StorgeStockResponseDto : BaseDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public Guid ProductBarcodeId { get; set; }
    public string BarcodeTitle { get; set; } = null!;
    public string BarcodeValue { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal Quantity { get; set; }
    public int MinStockLevel { get; set; }
    public bool IsBelowMinLevel { get; set; }
}
