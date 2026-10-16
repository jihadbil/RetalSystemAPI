using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات أرصدة المخازن الرئيسية وصالات العرض المترابطة.
/// </summary>
public partial class StockRepository : IStockRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<StorgeStock> _storageStocks;
    private readonly DbSet<ShowroomStock> _showroomStocks;
    public StockRepository(AppDbContext context)
    {
        _context = context;
        _storageStocks = context.Set<StorgeStock>();
        _showroomStocks = context.Set<ShowroomStock>();
    }

    // ── أرصدة المستودعات الرئيسية (StorgeStock) ─────────────────
    /// <inheritdoc/>
    public async Task<StorgeStock?> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        return await _storageStocks.AsNoTracking().FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == productBarcodeId, ct);
    }

    /// <inheritdoc/>
    public async Task AddStorgeStockAsync(StorgeStock stock, CancellationToken ct = default)
    {
        await _storageStocks.AddAsync(stock, ct);
    }

    /// <inheritdoc/>
    public async Task AddStorgeStocksAsync(IEnumerable<StorgeStock> stocks, CancellationToken ct = default)
    {
        await _storageStocks.AddRangeAsync(stocks, ct);
    }

    /// <inheritdoc/>
    public void UpdateStorgeStock(StorgeStock stock)
    {
        EntityPersistence.Update(_context, stock);
    }

    /// <inheritdoc/>
    public void SoftDeleteStorgeStock(StorgeStock stock)
    {
        EntityPersistence.SoftDelete(_context, stock);
    }

    /// <inheritdoc/>
    public void HardDeleteStorgeStock(StorgeStock stock)
    {
        EntityPersistence.HardDelete(_context, stock);
    }

    // ── أرصدة صالات العرض (ShowroomStock) ────────────────────────
    /// <inheritdoc/>
    public async Task<ShowroomStock?> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        return await _showroomStocks.AsNoTracking().FirstOrDefaultAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId, ct);
    }

    /// <inheritdoc/>
    public async Task AddShowroomStockAsync(ShowroomStock stock, CancellationToken ct = default)
    {
        await _showroomStocks.AddAsync(stock, ct);
    }

    /// <inheritdoc/>
    public async Task AddShowroomStocksAsync(IEnumerable<ShowroomStock> stocks, CancellationToken ct = default)
    {
        await _showroomStocks.AddRangeAsync(stocks, ct);
    }

    /// <inheritdoc/>
    public void UpdateShowroomStock(ShowroomStock stock)
    {
        EntityPersistence.Update(_context, stock);
    }

    /// <inheritdoc/>
    public void SoftDeleteShowroomStock(ShowroomStock stock)
    {
        EntityPersistence.SoftDelete(_context, stock);
    }

    /// <inheritdoc/>
    public void HardDeleteShowroomStock(ShowroomStock stock)
    {
        EntityPersistence.HardDelete(_context, stock);
    }
}
