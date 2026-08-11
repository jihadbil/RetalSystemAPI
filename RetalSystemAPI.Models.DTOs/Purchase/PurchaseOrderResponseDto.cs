using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Purchase;

public class PurchaseOrderResponseDto : BaseDto
{
    public string OrderNumber { get; set; } = null!;
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public List<PurchaseOrderItemResponseDto> Items { get; set; } = new();
}
