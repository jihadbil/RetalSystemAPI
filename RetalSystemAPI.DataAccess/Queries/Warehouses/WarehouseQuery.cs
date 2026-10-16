using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record WarehouseQuery : PageRequest
{
    public Guid? BranchId { get; init; }
    public WarehouseType? Type { get; init; }
}
