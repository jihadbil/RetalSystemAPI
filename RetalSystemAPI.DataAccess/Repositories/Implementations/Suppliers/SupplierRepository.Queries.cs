using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وفحوصات التكامل المرجعي والبحث الإحصائي للموردين.
/// </summary>
public partial class SupplierRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _context.CurrentTenantId;

        // التحقق من وجود مستندات مشتريات أو حركات مرتبطة بالمورد حتى المحذوفة منطقياً
        return await _context.PurchaseInvoices.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.SupplierId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.SupplierId == id, ct)
            || await _context.PurchaseReturns.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.SupplierId == id, ct)
            || await _context.SupplierTransactions.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.SupplierId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<Supplier?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع بيانات المورد مع هواتفه عبر مواصفة SupplierWithDetailsSpec كقراءة فقط
        return await SpecificationEvaluator<Supplier>
            .GetQuery(_context.Set<Supplier>(), new SupplierWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Supplier>> ListAsync(SupplierQuery request, CancellationToken ct = default)
    {
        // تنفيذ استعلام قائمة الموردين باستخدام SplitQuery لسرعة استرجاع الهواتف
        return await BuildList(request)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <summary>
    /// دالة مساعدة مركزية لبناء استعلام استرجاع الموردين مع تطبيق فلاتر البحث والديون والنشاط.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <returns>استعلام IQueryable للموردين.</returns>
    private IQueryable<Supplier> BuildList(SupplierQuery request)
    {
        // تطبيق مواصفة تفاصيل المورد مع شروط البحث
        var query = SpecificationEvaluator<Supplier>.GetQuery(
            _context.Set<Supplier>(),
            new SupplierWithDetailsSpec(request.Search));

        // تطبيق فلتر المديونية إن تم تحديده
        if (request.HasDebt.HasValue)
            query = query.Where(x => (x.CurrentBalance > 0) == request.HasDebt.Value);

        // تطبيق فلتر حالة النشاط إن تم تحديده
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);

        return query;
    }

    /// <inheritdoc/>
    public async Task<BalanceSummary> GetBalanceSummaryAsync(SupplierQuery request, CancellationToken ct = default)
    {
        // احتساب إجمالي الأرصدة المستحقة للموردين أو عليهم وعدد الموردين الدائنين
        return await BuildList(request)
            .AsNoTracking()
            .GroupBy(x => 1)
            .Select(g => new BalanceSummary
            {
                TotalDebt = g.Sum(x => x.CurrentBalance > 0 ? x.CurrentBalance : 0),
                TotalCredit = g.Sum(x => x.CurrentBalance < 0 ? -x.CurrentBalance : 0),
                WithDebt = g.Count(x => x.CurrentBalance > 0)
            })
            .FirstOrDefaultAsync(ct) ?? new();
    }

    /// <inheritdoc/>
    public async Task<PageResult<Supplier>> SearchAsync(SupplierQuery request, CancellationToken ct = default)
    {
        // تجهيز الاستعلام المفلتر
        var query = BuildList(request);

        // فلترة الموردين أصحاب الأرصدة إن طلب ذلك
        if (request.HasBalance)
            query = query.Where(supplier => supplier.CurrentBalance > 0);

        // قراءة الصفحة المطلوبة بالترقيم التلقائي
        return await QueryPaging.ReadAsync(query, request, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, CancellationToken ct = default)
    {
        // فحص وجود مورد مسجل بنفس الاسم للمستأجر الحالي
        return await _context.Set<Supplier>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default)
    {
        // فحص وجود مورد بنفس الاسم مع استثناء سجل المورد الحالي
        return await _context.Set<Supplier>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name && e.Id != id, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasFinancialHistoryAsync(Guid id, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _context.CurrentTenantId;

        // التحقق من وجود فواتير مشتريات، أوامر شراء، مرتجعات، أو حركات كشف حساب للمورد
        return await _context.PurchaseInvoices.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.SupplierId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.SupplierId == id, ct)
            || await _context.PurchaseReturns.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.SupplierId == id, ct)
            || await _context.SupplierTransactions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.SupplierId == id, ct);
    }
}
