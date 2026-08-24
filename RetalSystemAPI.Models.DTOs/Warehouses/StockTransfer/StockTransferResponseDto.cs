using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

public class StockTransferResponseDto : BaseDto
{
    public string TransferNumber { get; set; } = null!;
    public DateTime TransferDate { get; set; }
    public Guid FromWarehouseId { get; set; }
    public string FromWarehouseName { get; set; } = null!;
    public Guid ToWarehouseId { get; set; }
    public string ToWarehouseName { get; set; } = null!;
    public StockTransferStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public string? Notes { get; set; }
    public List<StockTransferItemResponseDto> Items { get; set; } = new();
}
