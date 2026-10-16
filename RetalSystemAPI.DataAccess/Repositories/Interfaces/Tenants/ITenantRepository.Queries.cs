using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface ITenantRepository
{
    Task<IReadOnlyList<Tenant>> ListLookupAsync(CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);
    Task<PageResult<Tenant>> SearchAsync(TenantQuery request, CancellationToken ct = default);
}
