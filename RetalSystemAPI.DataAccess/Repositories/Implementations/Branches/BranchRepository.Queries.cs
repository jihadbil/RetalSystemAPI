using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// الجزء المخصص للاستعلامات وفحوصات التكامل المرجعي والبحث لمستودع الفروع.
/// </summary>
public partial class BranchRepository
{
    /// <inheritdoc/>
    public Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود تبعيات أو إشارات تاريخية للفرع
        return HasDependenciesAsync(id, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasOperationalHistoryAsync(Guid id, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _context.CurrentTenantId;

        // فحص وجود سجلات تشغيلية عبر مختلف الوحدات مع تجاهل فلاتر الحذف المنطقي
        return await _context.Warehouses.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.Safes.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.Expenses.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.CustomerTransactions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.SupplierTransactions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.PosTerminals.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct)
            || await _context.CashSessions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.BranchId == id, ct);
    }

    /// <inheritdoc/>
    public Task<Branch?> GetWithPhonesForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع كيان الفرع متتبعاً مع تضمين هواتفه لتعديلها معاً
        return _context.Branches
            .Include(b => b.BranchPhones)
            .SingleOrDefaultAsync(b => b.Id == id, ct);
    }

    /// <inheritdoc/>
    public Task<bool> HasUsersAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود مستخدمين مسجلين تحت هذا الفرع للمستأجر الحالي
        return _context.Users.AnyAsync(u => u.TenantId == _context.CurrentTenantId && u.BranchId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasDependenciesAsync(Guid id, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenant = _context.CurrentTenantId;

        // فحص شامل لكافة الحركات والمستندات المرتبطة بالفرع حتى المحذوفة منطقياً
        return await _context.Warehouses.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.PurchaseInvoices.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.PurchaseReturns.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.SalesInvoices.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.SalesReturns.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.CustomerTransactions.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.SupplierTransactions.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.Safes.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.Expenses.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.PosTerminals.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct)
            || await _context.CashSessions.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenant && x.BranchId == id, ct);
    }

    /// <inheritdoc/>
    public Task<bool> PhonesExistAsync(IReadOnlyCollection<string> numbers, Guid? excludedBranchId = null, CancellationToken ct = default)
    {
        // التحقق من وجود أي رقم هاتف مسجل مسبقاً مع استثناء الفرع المعني عند التعديل
        return _context.BranchPhones.AnyAsync(p => numbers.Contains(p.PhoneNumber) &&
            (!excludedBranchId.HasValue || p.BranchId != excludedBranchId.Value), ct);
    }

    /// <inheritdoc/>
    public async Task<bool> IsActiveForTenantAsync(Guid branchId, Guid currentTenantId, CancellationToken ct = default)
    {
        // فحص نشاط وصلاحية الفرع ومطابقته للمستأجر كاستعلام خفيف
        return await _context.Set<Branch>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == branchId && e.TenantId == currentTenantId && e.IsActive, ct);
    }

    /// <inheritdoc/>
    public async Task<Branch?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // بناء استعلام الفرع مع الهواتف عبر مواصفة BranchWithPhonesSpec كقراءة فقط
        return await SpecificationEvaluator<Branch>
            .GetQuery(_context.Set<Branch>(), new BranchWithPhonesSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Branch>> ListAsync(BranchQuery request, CancellationToken ct = default)
    {
        // تنفيذ استعلام الفروع مع الهواتف باستخدام SplitQuery لتعظيم كفاءة استرجاع القوائم
        return await SpecificationEvaluator<Branch>
            .GetQuery(_context.Set<Branch>(), new BranchWithPhonesSpec())
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<Branch>> SearchAsync(BranchQuery request, CancellationToken ct = default)
    {
        // تنفيذ البحث المجزأ على صفحات بالاعتماد على مواصفة الفرع ومحرك التصفح QueryPaging
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<Branch>.GetQuery(_context.Set<Branch>(), new BranchWithPhonesSpec()),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        // فحص وجود اسم فرع مطابق ضمن نطاق المستأجر
        return await _context.Set<Branch>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        // فحص وجود اسم فرع مطابق مع استثناء السجل الحالي قيد التعديل
        return await _context.Set<Branch>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name && e.Id != id, ct);
    }

    /// <inheritdoc/>
    public async Task<Branch?> GetDetailsForDeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع الفرع مع الهواتف والمستخدمين عبر المواصفة المخصصة للتحقق قبل الحذف
        return await SpecificationEvaluator<Branch>
            .GetQuery(_context.Set<Branch>(), new BranchWithPhonesAndUsersSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }
}
