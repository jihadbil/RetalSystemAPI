namespace RetalSystemAPI.DataAccess.Queries;
public sealed record StorageStockQuery : PageRequest
{
    public Guid WarehouseId { get; init; }
    public string? SearchTerm { get; init; }
    public bool ExactBarcode { get; init; }
}
