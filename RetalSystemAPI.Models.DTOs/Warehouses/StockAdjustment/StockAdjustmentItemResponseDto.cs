using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

public class StockAdjustmentItemResponseDto : BaseDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid? ProductBarCodeId { get; set; }
    public string? BarcodeTitle { get; set; }
    public string? BarcodeValue { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int DifferenceQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public StockAdjustmentReason Reason { get; set; }
    public string ReasonName { get; set; } = string.Empty;
}
