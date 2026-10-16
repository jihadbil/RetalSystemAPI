namespace RetalSystemAPI.DataAccess.Queries;
public sealed record ShowroomStockQuery : PageRequest
{
    public Guid WarehouseId { get; init; }
    public string? SearchTerm { get; init; }
    public bool ExactBarcode { get; init; }
}
