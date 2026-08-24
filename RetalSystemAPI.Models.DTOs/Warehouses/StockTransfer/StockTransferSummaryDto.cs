using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

public class StockTransferSummaryDto : BaseDto
{
    public string TransferNumber { get; set; } = null!;
    public DateTime TransferDate { get; set; }
    public string FromWarehouseName { get; set; } = null!;
    public string ToWarehouseName { get; set; } = null!;
    public StockTransferStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public int ItemCount { get; set; }
}
