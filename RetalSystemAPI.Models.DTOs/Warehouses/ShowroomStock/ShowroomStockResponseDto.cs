using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;

public class ShowroomStockResponseDto : BaseDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public int MinStockLevel { get; set; }
    public bool IsBelowMinLevel { get; set; }
}
