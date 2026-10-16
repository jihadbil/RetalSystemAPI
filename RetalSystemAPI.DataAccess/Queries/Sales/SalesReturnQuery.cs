using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record SalesReturnQuery : PageRequest
{
    public Guid? BranchId { get; init; }
    public Guid? WarehouseId { get; init; }
    public Guid? CustomerId { get; init; }
    public SalesReturnReason? Reason { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Search { get; init; }
}
