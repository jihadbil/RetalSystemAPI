using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

public class StockAdjustmentResponseDto : BaseDto
{
    public string AdjustmentNumber { get; set; } = null!;
    public DateTime AdjustmentDate { get; set; }
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public StockAdjustmentReason Reason { get; set; }
    public string ReasonName { get; set; } = null!;
    public string? Notes { get; set; }
    public List<StockAdjustmentItemResponseDto> Items { get; set; } = new();
}
