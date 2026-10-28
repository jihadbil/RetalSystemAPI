using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات وبحوثات المستودعات والتحقق من التبعيات والمستندات التاريخية.
/// </summary>
public partial class WarehouseRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasDocumentReferencesAsync(Guid id, CancellationToken ct = default)
    {
        // استخراج معرف المستأجر الحالي لضمان عزل البيانات
        var tenantId = _context.CurrentTenantId;

        // المستندات التاريخية (بما فيها المحذوفة مؤقتاً) يجب فحصها لمنع كسر العلاقات المحاسبية والمخزنية
        return await _context.SalesInvoices.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.SalesReturns.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.PurchaseInvoices.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.PurchaseReturns.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.PurchaseOrders.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.StockAdjustments.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.WarehouseId == id, ct)
            || await _context.StockTransfers.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && (x.FromWarehouseId == id || x.ToWarehouseId == id), ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Warehouse>> GetByTypeAsync(WarehouseType type, CancellationToken ct = default)
    {
        // استرجاع المستودعات التابعة للنوع المعين مرتبة حسب المعرف دون تتبع التغييرات
        return await _context.Warehouses
            .AsNoTracking()
            .Where(e => e.Type == type)
            .OrderBy(e => e.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Warehouse>> ListLookupAsync(CancellationToken ct = default)
    {
        // جلب قائمة مستودعات سريعة للاستخدام في حقول الاختيار بالواجهة
        return await _context.Set<Warehouse>()
            .AsNoTracking()
            .OrderBy(e => e.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<Warehouse?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // جلب بيانات المستودع التفصيلية وعلاقته بالفرع عبر مواصفة WarehouseWithDetailsSpec
        return await SpecificationEvaluator<Warehouse>
            .GetQuery(_context.Set<Warehouse>(), new WarehouseWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Warehouse>> ListAsync(WarehouseQuery request, CancellationToken ct = default)
    {
        // جلب قائمة المستودعات المفلترة بحسب الفرع أو النوع مع تجزئة الاستعلام
        return await SpecificationEvaluator<Warehouse>
            .GetQuery(_context.Set<Warehouse>(), new WarehouseWithDetailsSpec(request.BranchId, request.Type))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<Warehouse>> SearchAsync(WarehouseQuery request, CancellationToken ct = default)
    {
        // تنفيذ البحث المقسم إلى صفحات للمستودعات بحسب المعايير المرسلة
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<Warehouse>.GetQuery(_context.Set<Warehouse>(), new WarehouseWithDetailsSpec(request.BranchId, request.Type)),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, Guid branchId, CancellationToken ct = default)
    {
        // التحقق من تكرار اسم المستودع داخل نفس الفرع للمستأجر
        return await _context.Set<Warehouse>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name && e.BranchId == branchId, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NameExistsAsync(string name, Guid branchId, Guid id, CancellationToken ct = default)
    {
        // التحقق من تكرار اسم المستودع في نفس الفرع باستثناء المعرف الحالي قيد التعديل
        return await _context.Set<Warehouse>()
            .AsNoTracking()
            .AnyAsync(e => e.Name == name && e.BranchId == branchId && e.Id != id, ct);
    }
}
