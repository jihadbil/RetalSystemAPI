using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات التصنيفات والتحقق من الأسماء والتسلسل الهرمي.
/// </summary>
public partial class CategoryRepository
{
    /// <inheritdoc/>
    public async Task<Category?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل التصنيف مع الكيانات المرتبطة عبر المواصفة كقراءة فقط
        return await SpecificationEvaluator<Category>
            .GetQuery(_context.Set<Category>(), new CategoryWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Category>> ListAsync(CategoryQuery request, CancellationToken ct = default)
    {
        // تنفيذ استعلام قائمة التصنيفات باستخدام SplitQuery لتعظيم كفاءة العلاقات
        return await SpecificationEvaluator<Category>
            .GetQuery(_context.Set<Category>(), new CategoryWithDetailsSpec())
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Category>> GetRootsAsync(CancellationToken ct = default)
    {
        // استرجاع التصنيفات الجذرية التي ليس لها تصنيف أب (ParentId == null)
        return await SpecificationEvaluator<Category>
            .GetQuery(_context.Set<Category>(), new RootCategoriesSpec())
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default)
    {
        // استرجاع التصنيفات الفرعية المباشرة التابعة للتصنيف الأب المحدد
        return await SpecificationEvaluator<Category>
            .GetQuery(_context.Set<Category>(), new SubCategoriesSpec(parentId))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<Category>> SearchAsync(CategoryQuery request, CancellationToken ct = default)
    {
        // تنفيذ البحث التصفحي في التصنيفات بالاعتماد على المواصفة ومحرك التصفح
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<Category>.GetQuery(_context.Set<Category>(), new CategoryWithDetailsSpec()),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        // فحص وجود أي تصنيف يحمل نفس الاسم في النظام
        return await _context.Set<Category>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        // فحص وجود تصنيف بنفس الاسم مع استبعاد السجل الحالي
        return await _context.Set<Category>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name && e.Id != id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CategoryLookupRow>> GetLookupAsync(CancellationToken ct = default)
    {
        // إسقاط معرف واسم التصنيف فقط لتوفير الذاكرة وسرعة نقل البيانات للقوائم المنسدلة
        return await _context.Set<Category>()
            .AsNoTracking()
            .Select(e => new CategoryLookupRow(e.Id, e.Name))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<Category?> GetSampleAsync(CancellationToken ct = default)
    {
        // استرجاع أول سجل تصنيف كعينة تجريبية
        return await _context.Set<Category>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => true, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CategoryNamesRow>> GetNamesAsync(CancellationToken ct = default)
    {
        // جلب قائمة بأسماء كافة التصنيفات كقراءة خفيفة
        return await _context.Set<Category>()
            .AsNoTracking()
            .Select(e => new CategoryNamesRow(e.Name))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Category>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default)
    {
        // قراءة وتتبع التصنيفات المطابقة لمفاتيح الاستيراد بدفعة واحدة لتحديثها
        return await ImportQueryBatch.ReadAsync(
            keys,
            (values, ids) => _context.Set<Category>().Where(e => ids.Contains(e.Id) || values.Contains(e.Name.Trim().ToUpper())),
            ct);
    }
}
