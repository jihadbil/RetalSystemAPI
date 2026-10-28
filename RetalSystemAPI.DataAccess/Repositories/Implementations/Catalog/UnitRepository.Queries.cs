using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وفحوصات وحدات القياس واسترجاع القوائم المنسدلة.
/// </summary>
public partial class UnitRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<UnitLookupRow>> GetLookupAsync(CancellationToken ct = default)
    {
        // استخراج معرف واسم الوحدة فقط لسرعة التحميل وتوفير الذاكرة
        return await _context.Set<Unit>()
            .AsNoTracking()
            .Select(e => new UnitLookupRow(e.Id, e.Name))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Unit>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default)
    {
        // جلب الوحدات المطابقة لأسماء ومعرفات الاستيراد بدفعة واحدة مع تتبعها للتحديث
        return await ImportQueryBatch.ReadAsync(
            keys,
            (values, ids) => _context.Set<Unit>().Where(e => ids.Contains(e.Id) || values.Contains(e.Name.Trim().ToUpper())),
            ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Unit>> ListLookupAsync(CancellationToken ct = default)
    {
        // استرجاع كافة كيانات الوحدات كقراءة فقط مرتبة بالمعرف
        return await _context.Set<Unit>()
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        // فحص وجود أي وحدة مسجلة بنفس الاسم للمستأجر
        return await _context.Set<Unit>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        // فحص تكرار الاسم مع استبعاد سجل الوحدة الحالي
        return await _context.Set<Unit>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name && e.Id != id, ct);
    }
}
