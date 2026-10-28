using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// الجزء المخصص لاستعلامات المستأجرين والتحقق من التفرد والتصفح بنظام الصفحات.
/// </summary>
public partial class TenantRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<Tenant>> ListLookupAsync(CancellationToken ct = default)
    {
        // استرجاع قائمة المستأجرين كقراءة فقط بدون تتبع لملء القوائم المنسدلة
        return await _context.Set<Tenant>()
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        // فحص وجود أي مستأجر يحمل نفس الاسم في النظام
        return await _context.Set<Tenant>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        // فحص وجود اسم مكرر مع استبعاد سجل المستأجر الحالي
        return await _context.Set<Tenant>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name && e.Id != id, ct);
    }

    /// <inheritdoc/>
    public Task<PageResult<Tenant>> SearchAsync(TenantQuery request, CancellationToken ct = default)
    {
        // بناء الاستعلام التصفحي مع الترتيب التنازلي حسب تاريخ الإنشاء ثم المعرف
        return QueryPaging.ReadAsync(
            _context.Tenants.OrderByDescending(t => t.CreatedAt).ThenBy(t => t.Id),
            request,
            ct);
    }
}
