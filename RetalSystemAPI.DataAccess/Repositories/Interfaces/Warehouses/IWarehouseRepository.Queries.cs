using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface IWarehouseRepository
{
    Task<IReadOnlyList<Warehouse>> GetByTypeAsync(WarehouseType type, CancellationToken ct = default);
    Task<IReadOnlyList<Warehouse>> ListLookupAsync(CancellationToken ct = default);
    Task<Warehouse?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Warehouse>> ListAsync(WarehouseQuery request, CancellationToken ct = default);
    Task<PageResult<Warehouse>> SearchAsync(WarehouseQuery request, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid branchId, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid branchId, Guid id, CancellationToken ct = default);
}
