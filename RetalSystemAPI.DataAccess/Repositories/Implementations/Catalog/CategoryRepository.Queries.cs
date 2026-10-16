using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class CategoryRepository
{
    public async Task<Category?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Category>.GetQuery(_context.Set<Category>(), new CategoryWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> ListAsync(CategoryQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Category>.GetQuery(_context.Set<Category>(), new CategoryWithDetailsSpec()).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetRootsAsync(CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Category>.GetQuery(_context.Set<Category>(), new RootCategoriesSpec()).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Category>.GetQuery(_context.Set<Category>(), new SubCategoriesSpec(parentId)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<PageResult<Category>> SearchAsync(CategoryQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(SpecificationEvaluator<Category>.GetQuery(_context.Set<Category>(), new CategoryWithDetailsSpec()), request, ct);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        return await _context.Set<Category>().AsNoTracking().AnyAsync(e => e.Name == name, ct);
    }

    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<Category>().AsNoTracking().AnyAsync(e => e.Name == name && e.Id != id, ct);
    }

    public async Task<IReadOnlyList<CategoryLookupRow>> GetLookupAsync(CancellationToken ct = default)
    {
        return await _context.Set<Category>().AsNoTracking().Select(e => new CategoryLookupRow(e.Id, e.Name)).ToListAsync(ct);
    }

    public async Task<Category?> GetSampleAsync(CancellationToken ct = default)
    {
        return await _context.Set<Category>().AsNoTracking().FirstOrDefaultAsync(e => true, ct);
    }

    public async Task<IReadOnlyList<CategoryNamesRow>> GetNamesAsync(CancellationToken ct = default)
    {
        return await _context.Set<Category>().AsNoTracking().Select(e => new CategoryNamesRow(e.Name)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Category>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default)
    {
        return await ImportQueryBatch.ReadAsync(keys, (values, ids) => _context.Set<Category>().Where(e => ids.Contains(e.Id) || values.Contains(e.Name.Trim().ToUpper())), ct);
    }
}
