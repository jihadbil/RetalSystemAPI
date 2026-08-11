using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Purchase;

public class PurchaseOrderSummaryDto : BaseDto
{
    public string OrderNumber { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string? WarehouseName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}
