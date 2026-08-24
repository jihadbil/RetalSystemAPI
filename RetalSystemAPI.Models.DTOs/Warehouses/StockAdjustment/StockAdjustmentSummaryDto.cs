using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

public class StockAdjustmentSummaryDto : BaseDto
{
    public string AdjustmentNumber { get; set; } = null!;
    public DateTime AdjustmentDate { get; set; }
    public string WarehouseName { get; set; } = null!;
    public StockAdjustmentReason Reason { get; set; }
    public string ReasonName { get; set; } = null!;
    public int ItemCount { get; set; }
}
