using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
/// <summary>
/// تنفيذ مستودع بيانات الأصناف والمنتجات مع إدارة وحداتها وباركوداتها وصورها.
/// </summary>
public partial class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    // ── إدارة وحدات المنتج (Product Units) ───────────────────────
    /// <inheritdoc/>
    public async Task AddUnitAsync(ProductUnit unit, CancellationToken ct = default)
    {
        await _context.Set<ProductUnit>().AddAsync(unit, ct);
    }

    /// <inheritdoc/>
    public async Task AddUnitsAsync(IEnumerable<ProductUnit> units, CancellationToken ct = default)
    {
        await _context.Set<ProductUnit>().AddRangeAsync(units, ct);
    }

    /// <inheritdoc/>
    public void UpdateUnit(ProductUnit unit)
    {
        EntityPersistence.Update(_context, unit);
    }

    /// <inheritdoc/>
    public void RemoveUnit(ProductUnit unit)
    {
        EntityPersistence.HardDelete(_context, unit);
    }

    /// <inheritdoc/>
    public void SoftDeleteUnit(ProductUnit unit)
    {
        EntityPersistence.SoftDelete(_context, unit);
    }

    /// <inheritdoc/>
    public void HardDeleteUnit(ProductUnit unit)
    {
        EntityPersistence.HardDelete(_context, unit);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductUnit>> GetUnitsAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().AsNoTracking().Where(u => u.ProductId == productId).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ProductUnit?> GetUnitByIdAsync(Guid unitId, CancellationToken ct = default)
    {
        return await EntityPersistence.GetByIdAsync<ProductUnit>(_context, unitId, ct);
    }

    // ── إدارة باركودات ونكهات المنتج (Product BarCodes) ─────────
    /// <inheritdoc/>
    public async Task AddBarCodeAsync(ProductBarCode barCode, CancellationToken ct = default)
    {
        await _context.Set<ProductBarCode>().AddAsync(barCode, ct);
    }

    /// <inheritdoc/>
    public async Task AddBarCodesAsync(IEnumerable<ProductBarCode> barCodes, CancellationToken ct = default)
    {
        await _context.Set<ProductBarCode>().AddRangeAsync(barCodes, ct);
    }

    /// <inheritdoc/>
    public void UpdateBarCode(ProductBarCode barCode)
    {
        EntityPersistence.Update(_context, barCode);
    }

    /// <inheritdoc/>
    public void RemoveBarCode(ProductBarCode barCode)
    {
        EntityPersistence.HardDelete(_context, barCode);
    }

    /// <inheritdoc/>
    public void SoftDeleteBarCode(ProductBarCode barCode)
    {
        EntityPersistence.SoftDelete(_context, barCode);
    }

    /// <inheritdoc/>
    public void HardDeleteBarCode(ProductBarCode barCode)
    {
        EntityPersistence.HardDelete(_context, barCode);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarCode>> GetBarCodesAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().Where(b => b.ProductId == productId).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ProductBarCode?> GetBarCodeByIdAsync(Guid barCodeId, CancellationToken ct = default)
    {
        return await EntityPersistence.GetByIdAsync<ProductBarCode>(_context, barCodeId, ct);
    }

    // ── إدارة صور المنتج (Product Images) ─────────────────────────
    /// <inheritdoc/>
    public async Task AddImageAsync(ProductImage image, CancellationToken ct = default)
    {
        await _context.Set<ProductImage>().AddAsync(image, ct);
    }

    /// <inheritdoc/>
    public async Task AddImagesAsync(IEnumerable<ProductImage> images, CancellationToken ct = default)
    {
        await _context.Set<ProductImage>().AddRangeAsync(images, ct);
    }

    /// <inheritdoc/>
    public void UpdateImage(ProductImage image)
    {
        EntityPersistence.Update(_context, image);
    }

    /// <inheritdoc/>
    public void RemoveImage(ProductImage image)
    {
        EntityPersistence.HardDelete(_context, image);
    }

    /// <inheritdoc/>
    public void SoftDeleteImage(ProductImage image)
    {
        EntityPersistence.SoftDelete(_context, image);
    }

    /// <inheritdoc/>
    public void HardDeleteImage(ProductImage image)
    {
        EntityPersistence.HardDelete(_context, image);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductImage>> GetImagesAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().AsNoTracking().Where(i => i.ProductId == productId).ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ProductImage?> GetImageByIdAsync(Guid imageId, CancellationToken ct = default)
    {
        return await EntityPersistence.GetByIdAsync<ProductImage>(_context, imageId, ct);
    }

    /// <inheritdoc/>
    public Task<ProductImage?> GetImageForDeletionAsync(Guid imageId, CancellationToken ct = default)
        => _context.Set<ProductImage>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == imageId && i.TenantId == _context.CurrentTenantId, ct);

    public async Task<int> CountImagesAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().CountAsync(i => i.ProductId == productId, ct);
    }
}
