using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Purchase;

public class PurchaseOrderItemResponseDto : BaseDto
{
    public Guid ProductBarCodeId { get; set; }
    public string BarcodeTitle { get; set; } = null!;
    public string BarcodeValue { get; set; } = null!;
    public string ProductName { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
