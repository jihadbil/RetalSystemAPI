using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وفحوصات التكامل المرجعي والبحث الإحصائي للعملاء.
/// </summary>
public partial class CustomerRepository
{
    /// <inheritdoc/>
    public Task<bool> HasHistoricalReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من وجود أي تاريخ مالي مسجل للعميل يمنع حذفه
        return HasFinancialHistoryAsync(id, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasFinancialHistoryAsync(Guid id, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي
        var tenantId = _context.CurrentTenantId;

        // التحقق من وجود فواتير مبيعات، مرتجعات، أو حركات كشف حساب مرتبطة بالعميل حتى المحذوفة منطقياً
        return await _context.SalesInvoices.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.CustomerId == id, ct)
            || await _context.SalesReturns.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.CustomerId == id, ct)
            || await _context.CustomerTransactions.IgnoreQueryFilters().AnyAsync(e => e.TenantId == tenantId && e.CustomerId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<Customer?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // بناء استعلام تفاصيل العميل مع هواتفه عبر المواصفة كقراءة فقط دون تتبع
        return await SpecificationEvaluator<Customer>
            .GetQuery(_context.Set<Customer>(), new CustomerWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Customer>> ListAsync(CustomerQuery request, CancellationToken ct = default)
    {
        // تنفيذ استعلام القائمة المفلترة باستخدام SplitQuery لسرعة استرجاع الهواتف
        return await BuildList(request)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <summary>
    /// دالة مساعدة مركزية لبناء استعلام استرجاع العملاء بناءً على فلاتر النوع، النشاط، البحث، والديون.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <returns>استعلام IQueryable للعملاء.</returns>
    private IQueryable<Customer> BuildList(CustomerQuery request)
    {
        // تطبيق مواصفة تفاصيل العملاء وفلاتر النوع والنشاط وكلمات البحث
        var query = SpecificationEvaluator<Customer>.GetQuery(
            _context.Set<Customer>(),
            new CustomerWithDetailsSpec(request.Type, request.IsActive, request.Search));

        // تطبيق فلتر وجود الديون إن تم تحديده
        if (request.HasDebt.HasValue)
            query = query.Where(x => (x.CurrentBalance > 0) == request.HasDebt.Value);

        return query;
    }

    /// <inheritdoc/>
    public async Task<BalanceSummary> GetBalanceSummaryAsync(CustomerQuery request, CancellationToken ct = default)
    {
        // تجميع أرصدة العملاء دفترياً لحساب إجمالي المديونيات والأرصدة الدائنة وعدد المدينين
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
    public async Task<PageResult<Customer>> SearchAsync(CustomerQuery request, CancellationToken ct = default)
    {
        // تجهيز الاستعلام الأساسي المفلتر
        var query = BuildList(request);

        // فلترة العملاء أصحاب الأرصدة الإيجابية إن طلب ذلك
        if (request.HasBalance)
            query = query.Where(customer => customer.CurrentBalance > 0);

        // قراءة الصفحة المطلوبة وتطبيق الترقيم التلقائي
        return await QueryPaging.ReadAsync(query, request, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> CodeExistsAsync(string code, CancellationToken ct = default)
    {
        // فحص وجود كود مطابق لعميل مسجل للمستأجر الحالي
        return await _context.Set<Customer>()
            .AsNoTracking()
            .AnyAsync(e => e.Code != null && e.Code.Trim() == code, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> CodeExistsAsync(string code, Guid id, CancellationToken ct = default)
    {
        // فحص وجود كود مطابق مع استبعاد سجل العميل قيد التعديل
        return await _context.Set<Customer>()
            .AsNoTracking()
            .AnyAsync(e => e.Code != null && e.Code.Trim() == code && e.Id != id, ct);
    }
}
