using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class SalesReturnItemResponseDto : BaseDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = null!;
    public Guid? ProductBarCodeId { get; set; }
    public string? BarcodeTitle { get; set; }
    public string? BarcodeValue { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}
