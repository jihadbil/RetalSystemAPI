using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record StockAdjustmentQuery : PageRequest
{
    public Guid? WarehouseId { get; init; }
    public StockAdjustmentReason? Reason { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Search { get; init; }
}
