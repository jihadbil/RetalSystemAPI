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
    /// <summary>
    /// تهيئة مستودع المنتجات وربطه بسياق قاعدة البيانات المشترك.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات.</param>
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    // ── إدارة وحدات المنتج (Product Units) ───────────────────────

    /// <inheritdoc/>
    public async Task AddUnitAsync(ProductUnit unit, CancellationToken ct = default)
    {
        // إضافة وحدة قياس جديدة للمنتج في جدول وحدات الأصناف
        await _context.Set<ProductUnit>().AddAsync(unit, ct);
    }

    /// <inheritdoc/>
    public async Task AddUnitsAsync(IEnumerable<ProductUnit> units, CancellationToken ct = default)
    {
        // إضافة قائمة وحدات قياس للمنتج دفعة واحدة
        await _context.Set<ProductUnit>().AddRangeAsync(units, ct);
    }

    /// <inheritdoc/>
    public void UpdateUnit(ProductUnit unit)
    {
        // تحديث وحدة قياس المنتج مع حماية التزامن والمفاتيح
        EntityPersistence.Update(_context, unit);
    }

    /// <inheritdoc/>
    public void RemoveUnit(ProductUnit unit)
    {
        // إزالة وحدة المنتج نهائياً من السياق وقاعدة البيانات
        EntityPersistence.HardDelete(_context, unit);
    }

    /// <inheritdoc/>
    public void SoftDeleteUnit(ProductUnit unit)
    {
        // تطبيق الحذف المنطقي على وحدة المنتج
        EntityPersistence.SoftDelete(_context, unit);
    }

    /// <inheritdoc/>
    public void HardDeleteUnit(ProductUnit unit)
    {
        // حذف وحدة المنتج نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, unit);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductUnit>> GetUnitsAsync(Guid productId, CancellationToken ct = default)
    {
        // استعلام وحدات المنتج كقراءة فقط بدون تتبع
        return await _context.Set<ProductUnit>()
            .AsNoTracking()
            .Where(u => u.ProductId == productId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ProductUnit?> GetUnitByIdAsync(Guid unitId, CancellationToken ct = default)
    {
        // استرجاع وحدة المنتج ككيان متتبع للتعديل
        return await EntityPersistence.GetByIdAsync<ProductUnit>(_context, unitId, ct);
    }

    // ── إدارة باركودات ونكهات المنتج (Product BarCodes) ─────────

    /// <inheritdoc/>
    public async Task AddBarCodeAsync(ProductBarCode barCode, CancellationToken ct = default)
    {
        // إضافة باركود جديد للمنتج في جدول الباركودات
        await _context.Set<ProductBarCode>().AddAsync(barCode, ct);
    }

    /// <inheritdoc/>
    public async Task AddBarCodesAsync(IEnumerable<ProductBarCode> barCodes, CancellationToken ct = default)
    {
        // إضافة قائمة باركودات للمنتج دفعة واحدة
        await _context.Set<ProductBarCode>().AddRangeAsync(barCodes, ct);
    }

    /// <inheritdoc/>
    public void UpdateBarCode(ProductBarCode barCode)
    {
        // تحديث بيانات باركود المنتج مع التحقق من التزامن
        EntityPersistence.Update(_context, barCode);
    }

    /// <inheritdoc/>
    public void RemoveBarCode(ProductBarCode barCode)
    {
        // إزالة باركود المنتج نهائياً من السياق
        EntityPersistence.HardDelete(_context, barCode);
    }

    /// <inheritdoc/>
    public void SoftDeleteBarCode(ProductBarCode barCode)
    {
        // تطبيق الحذف المنطقي على باركود المنتج
        EntityPersistence.SoftDelete(_context, barCode);
    }

    /// <inheritdoc/>
    public void HardDeleteBarCode(ProductBarCode barCode)
    {
        // حذف باركود المنتج نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, barCode);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarCode>> GetBarCodesAsync(Guid productId, CancellationToken ct = default)
    {
        // استعلام باركودات المنتج كقراءة فقط بدون تتبع
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Where(b => b.ProductId == productId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ProductBarCode?> GetBarCodeByIdAsync(Guid barCodeId, CancellationToken ct = default)
    {
        // استرجاع باركود المنتج ككيان متتبع للتعديل
        return await EntityPersistence.GetByIdAsync<ProductBarCode>(_context, barCodeId, ct);
    }

    // ── إدارة صور المنتج (Product Images) ─────────────────────────

    /// <inheritdoc/>
    public async Task AddImageAsync(ProductImage image, CancellationToken ct = default)
    {
        // إضافة صورة جديدة للمنتج في جدول الصور
        await _context.Set<ProductImage>().AddAsync(image, ct);
    }

    /// <inheritdoc/>
    public async Task AddImagesAsync(IEnumerable<ProductImage> images, CancellationToken ct = default)
    {
        // إضافة قائمة صور للمنتج دفعة واحدة
        await _context.Set<ProductImage>().AddRangeAsync(images, ct);
    }

    /// <inheritdoc/>
    public void UpdateImage(ProductImage image)
    {
        // تحديث بيانات صورة المنتج
        EntityPersistence.Update(_context, image);
    }

    /// <inheritdoc/>
    public void RemoveImage(ProductImage image)
    {
        // إزالة صورة المنتج نهائياً من السياق
        EntityPersistence.HardDelete(_context, image);
    }

    /// <inheritdoc/>
    public void SoftDeleteImage(ProductImage image)
    {
        // تطبيق الحذف المنطقي على صورة المنتج
        EntityPersistence.SoftDelete(_context, image);
    }

    /// <inheritdoc/>
    public void HardDeleteImage(ProductImage image)
    {
        // حذف صورة المنتج نهائياً من قاعدة البيانات
        EntityPersistence.HardDelete(_context, image);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductImage>> GetImagesAsync(Guid productId, CancellationToken ct = default)
    {
        // استعلام كافة صور المنتج كقراءة فقط بدون تتبع
        return await _context.Set<ProductImage>()
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<ProductImage?> GetImageByIdAsync(Guid imageId, CancellationToken ct = default)
    {
        // استرجاع صورة المنتج ككيان متتبع للتعديل
        return await EntityPersistence.GetByIdAsync<ProductImage>(_context, imageId, ct);
    }

    /// <inheritdoc/>
    public Task<ProductImage?> GetImageForDeletionAsync(Guid imageId, CancellationToken ct = default)
    {
        // جلب صورة المنتج مع تجاهل فلاتر الحذف المنطقي للتمكن من حذفها فعلياً وتطهيرها
        return _context.Set<ProductImage>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.Id == imageId && i.TenantId == _context.CurrentTenantId, ct);
    }

    /// <inheritdoc/>
    public async Task<int> CountImagesAsync(Guid productId, CancellationToken ct = default)
    {
        // حساب إجمالي عدد الصور التابعة للمنتج بشكل خفيف وسريع
        return await _context.Set<ProductImage>()
            .CountAsync(i => i.ProductId == productId, ct);
    }
}
