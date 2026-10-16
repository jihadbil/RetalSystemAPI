using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
public partial interface ICategoryRepository
{
    Task<Category?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> ListAsync(CategoryQuery request, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetRootsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);
    Task<PageResult<Category>> SearchAsync(CategoryQuery request, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CategoryLookupRow>> GetLookupAsync(CancellationToken ct = default);
    Task<Category?> GetSampleAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CategoryNamesRow>> GetNamesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default);
}
