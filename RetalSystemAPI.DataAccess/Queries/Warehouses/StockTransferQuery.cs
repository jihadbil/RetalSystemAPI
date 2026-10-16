using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record StockTransferQuery : PageRequest
{
    public Guid? FromWarehouseId { get; init; }
    public Guid? ToWarehouseId { get; init; }
    public StockTransferStatus? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Search { get; init; }
}
