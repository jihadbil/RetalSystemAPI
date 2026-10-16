using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record PurchaseOrderQuery : PageRequest
{
    public Guid? BranchId { get; init; }
    public Guid? WarehouseId { get; init; }
    public PurchaseOrderStatus? Status { get; init; }
    public string? Search { get; init; }
}
