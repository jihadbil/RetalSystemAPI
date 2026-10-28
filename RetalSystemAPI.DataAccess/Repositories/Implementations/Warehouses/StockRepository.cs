using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات أرصدة المخازن الرئيسية (StorgeStock) وصالات العرض (ShowroomStock) المترابطة.
/// </summary>
public partial class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<StorgeStock> _storageStocks;
    private readonly DbSet<ShowroomStock> _showroomStocks;

    /// <summary>
    /// تهيئة نسخة جديدة من مستودع الأرصدة المخزنية بحقن سياق البيانات وتحديد مجموعات الجداول.
    /// </summary>
    /// <param name="context">سياق قاعدة بيانات النظام للوصول إلى جداول الأرصدة وإجراء العمليات الحسابية.</param>
    public StockRepository(AppDbContext context)
    {
        // تعيين سياق قاعدة البيانات
        _context = context;

        // تهيئة مجموعة جداول أرصدة المستودعات الرئيسية
        _storageStocks = context.Set<StorgeStock>();

        // تهيئة مجموعة جداول أرصدة صالات العرض
        _showroomStocks = context.Set<ShowroomStock>();
    }

    // ── أرصدة المستودعات الرئيسية (StorgeStock) ─────────────────

    /// <inheritdoc/>
    public async Task<StorgeStock?> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        // البحث عن رصيد المستودع لباركود معين دون تتبع للتغييرات
        return await _storageStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == productBarcodeId, ct);
    }

    /// <inheritdoc/>
    public async Task AddStorgeStockAsync(StorgeStock stock, CancellationToken ct = default)
    {
        // إضافة سجل رصيد جديد للمستودع الرئيسي
        await _storageStocks.AddAsync(stock, ct);
    }

    /// <inheritdoc/>
    public async Task AddStorgeStocksAsync(IEnumerable<StorgeStock> stocks, CancellationToken ct = default)
    {
        // إضافة دفعة من سجلات أرصدة المستودع الرئيسي
        await _storageStocks.AddRangeAsync(stocks, ct);
    }

    /// <inheritdoc/>
    public void UpdateStorgeStock(StorgeStock stock)
    {
        // تحديث رصيد المستودع عبر معالج استمرارية الكيانات مع فحص الإصدار والمستأجر
        EntityPersistence.Update(_context, stock);
    }

    /// <inheritdoc/>
    public void SoftDeleteStorgeStock(StorgeStock stock)
    {
        // الحذف المنطقي لرصيد المستودع مع وسمه وتعيين وقت الحذف
        EntityPersistence.SoftDelete(_context, stock);
    }

    /// <inheritdoc/>
    public void HardDeleteStorgeStock(StorgeStock stock)
    {
        // الحذف الفيزيائي النهائي لرصيد المستودع من قاعدة البيانات
        EntityPersistence.HardDelete(_context, stock);
    }

    // ── أرصدة صالات العرض (ShowroomStock) ────────────────────────

    /// <inheritdoc/>
    public async Task<ShowroomStock?> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        // البحث عن رصيد صالة العرض للمنتج دون تتبع التغييرات
        return await _showroomStocks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);
    }

    /// <inheritdoc/>
    public async Task AddShowroomStockAsync(ShowroomStock stock, CancellationToken ct = default)
    {
        // إضافة سجل رصيد جديد لصالة العرض
        await _showroomStocks.AddAsync(stock, ct);
    }

    /// <inheritdoc/>
    public async Task AddShowroomStocksAsync(IEnumerable<ShowroomStock> stocks, CancellationToken ct = default)
    {
        // إضافة مجموعة سجلات أرصدة لصالات العرض دفعة واحدة
        await _showroomStocks.AddRangeAsync(stocks, ct);
    }

    /// <inheritdoc/>
    public void UpdateShowroomStock(ShowroomStock stock)
    {
        // تحديث رصيد صالة العرض عبر معالج استمرارية الكيانات
        EntityPersistence.Update(_context, stock);
    }

    /// <inheritdoc/>
    public void SoftDeleteShowroomStock(ShowroomStock stock)
    {
        // الحذف المنطقي لرصيد صالة العرض مع وسمه كمحذوف
        EntityPersistence.SoftDelete(_context, stock);
    }

    /// <inheritdoc/>
    public void HardDeleteShowroomStock(ShowroomStock stock)
    {
        // الحذف الفيزيائي النهائي لرصيد صالة العرض من قاعدة البيانات
        EntityPersistence.HardDelete(_context, stock);
    }
}
