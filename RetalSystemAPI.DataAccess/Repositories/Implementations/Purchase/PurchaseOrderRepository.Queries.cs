using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات أوامر وطلبات الشراء والتحقق من التبعيات وتفرد أرقام الأوامر.
/// </summary>
public partial class PurchaseOrderRepository
{
    /// <inheritdoc/>
    public async Task<bool> HasBarcodeReferencesAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        // فحص وجود أي بند أمر شراء يشير للباركودات المعطاة
        return await _context.Set<PurchaseOrderItem>()
            .AsNoTracking()
            .AnyAsync(e => removedBarcodeIds.Contains(e.ProductBarCodeId), ct);
    }

    /// <inheritdoc/>
    public async Task<PurchaseOrder?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع تفاصيل أمر الشراء وبنوده والمورد والفرع كقراءة فقط عبر مواصفة PurchaseOrderWithDetailsSpec
        return await SpecificationEvaluator<PurchaseOrder>
            .GetQuery(_context.Set<PurchaseOrder>(), new PurchaseOrderWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PurchaseOrder>> ListAsync(PurchaseOrderQuery request, CancellationToken ct = default)
    {
        // تنفيذ استعلام قائمة أوامر الشراء باستخدام SplitQuery لتعظيم كفاءة استرجاع البنود
        return await SpecificationEvaluator<PurchaseOrder>
            .GetQuery(_context.Set<PurchaseOrder>(), new PurchaseOrderListSpec(request.BranchId, request.WarehouseId, request.Status, request.Search))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<PurchaseOrder>> SearchAsync(PurchaseOrderQuery request, CancellationToken ct = default)
    {
        // البحث في أوامر الشراء بنظام الصفحات والفرز
        return await QueryPaging.ReadAsync(
            SpecificationEvaluator<PurchaseOrder>.GetQuery(_context.Set<PurchaseOrder>(), new PurchaseOrderListSpec(request.BranchId, request.WarehouseId, request.Status, request.Search)),
            request,
            ct);
    }

    /// <inheritdoc/>
    public async Task<bool> NumberExistsAsync(string orderNumber, CancellationToken ct = default)
    {
        // فحص وجود أمر شراء مسجل بنفس الرقم
        return await _context.Set<PurchaseOrder>()
            .AsNoTracking()
            .AnyAsync(e => e.OrderNumber == orderNumber, ct);
    }

    /// <inheritdoc/>
    public async Task<PurchaseOrder?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل أمر الشراء وبنوده متتبعة للتعديل ضمن وحدة العمل
        return await SpecificationEvaluator<PurchaseOrder>
            .GetQuery(_context.Set<PurchaseOrder>(), new PurchaseOrderWithDetailsSpec(id))
            .FirstOrDefaultAsync(ct);
    }
}
