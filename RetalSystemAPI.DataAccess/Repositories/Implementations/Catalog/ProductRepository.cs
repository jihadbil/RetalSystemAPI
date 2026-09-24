using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// تنفيذ مستودع بيانات الأصناف والمنتجات مع إدارة وحداتها وباركوداتها وصورها.
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    // ── إدارة وحدات المنتج (Product Units) ───────────────────────

    /// <inheritdoc />
    public async Task AddUnitAsync(ProductUnit unit, CancellationToken ct = default)
    {
        await _context.Set<ProductUnit>().AddAsync(unit, ct);
    }

    /// <inheritdoc />
    public async Task AddUnitsAsync(IEnumerable<ProductUnit> units, CancellationToken ct = default)
    {
        await _context.Set<ProductUnit>().AddRangeAsync(units, ct);
    }

    /// <inheritdoc />
    public void UpdateUnit(ProductUnit unit)
    {
        var local = _context.Set<ProductUnit>().Local.FirstOrDefault(e => e.Id == unit.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, unit))
                _context.Entry(local).CurrentValues.SetValues(unit);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(unit);
        if (entry.State == EntityState.Detached)
            _context.Set<ProductUnit>().Attach(unit);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void RemoveUnit(ProductUnit unit)
    {
        _context.Set<ProductUnit>().Remove(unit);
    }

    /// <inheritdoc />
    public void SoftDeleteUnit(ProductUnit unit)
    {
        unit.IsDeleted = true;
        var local = _context.Set<ProductUnit>().Local.FirstOrDefault(e => e.Id == unit.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, unit))
                _context.Entry(local).CurrentValues.SetValues(unit);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(unit);
        if (entry.State == EntityState.Detached)
            _context.Set<ProductUnit>().Attach(unit);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void HardDeleteUnit(ProductUnit unit)
    {
        var local = _context.Set<ProductUnit>().Local.FirstOrDefault(e => e.Id == unit.Id);
        if (local != null)
        {
            _context.Set<ProductUnit>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(unit);
            if (entry.State != EntityState.Detached)
                _context.Set<ProductUnit>().Remove(unit);
            else
            {
                _context.Attach(unit);
                _context.Set<ProductUnit>().Remove(unit);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductUnit>> GetUnitsAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>()
            .AsNoTracking()
            .Where(u => u.ProductId == productId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductUnit>> FindUnitsAsync(Expression<Func<ProductUnit, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductUnit>> FindUnitsAsync(ISpecification<ProductUnit> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ProductUnit>
            .GetQuery(_context.Set<ProductUnit>().AsQueryable(), spec)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductUnit>> FindUnitsTrackedAsync(Expression<Func<ProductUnit, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductUnit>> GetAllUnitsTrackedAsync(CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ProductUnit?> GetUnitByIdAsync(Guid unitId, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().FindAsync(new object[] { unitId }, ct);
    }

    /// <inheritdoc />
    public async Task<bool> UnitExistsAsync(Expression<Func<ProductUnit, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().AnyAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<ProductUnit?> FirstOrDefaultUnitTrackedAsync(Expression<Func<ProductUnit, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TResult>> SelectUnitsAsync<TResult>(Expression<Func<ProductUnit, TResult>> selector, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>()
            .AsNoTracking()
            .Select(selector)
            .ToListAsync(ct);
    }


    // ── إدارة باركودات ونكهات المنتج (Product BarCodes) ─────────

    /// <inheritdoc />
    public async Task AddBarCodeAsync(ProductBarCode barCode, CancellationToken ct = default)
    {
        await _context.Set<ProductBarCode>().AddAsync(barCode, ct);
    }

    /// <inheritdoc />
    public async Task AddBarCodesAsync(IEnumerable<ProductBarCode> barCodes, CancellationToken ct = default)
    {
        await _context.Set<ProductBarCode>().AddRangeAsync(barCodes, ct);
    }

    /// <inheritdoc />
    public void UpdateBarCode(ProductBarCode barCode)
    {
        var local = _context.Set<ProductBarCode>().Local.FirstOrDefault(e => e.Id == barCode.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, barCode))
                _context.Entry(local).CurrentValues.SetValues(barCode);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(barCode);
        if (entry.State == EntityState.Detached)
            _context.Set<ProductBarCode>().Attach(barCode);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void RemoveBarCode(ProductBarCode barCode)
    {
        _context.Set<ProductBarCode>().Remove(barCode);
    }

    /// <inheritdoc />
    public void SoftDeleteBarCode(ProductBarCode barCode)
    {
        barCode.IsDeleted = true;
        var local = _context.Set<ProductBarCode>().Local.FirstOrDefault(e => e.Id == barCode.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, barCode))
                _context.Entry(local).CurrentValues.SetValues(barCode);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(barCode);
        if (entry.State == EntityState.Detached)
            _context.Set<ProductBarCode>().Attach(barCode);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void HardDeleteBarCode(ProductBarCode barCode)
    {
        var local = _context.Set<ProductBarCode>().Local.FirstOrDefault(e => e.Id == barCode.Id);
        if (local != null)
        {
            _context.Set<ProductBarCode>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(barCode);
            if (entry.State != EntityState.Detached)
                _context.Set<ProductBarCode>().Remove(barCode);
            else
            {
                _context.Attach(barCode);
                _context.Set<ProductBarCode>().Remove(barCode);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductBarCode>> GetBarCodesAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Where(b => b.ProductId == productId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductBarCode>> FindBarCodesAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductBarCode>> FindBarCodesAsync(ISpecification<ProductBarCode> spec, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ProductBarCode>
            .GetQuery(_context.Set<ProductBarCode>().AsQueryable(), spec)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductBarCode>> FindBarCodesTrackedAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductBarCode>> GetAllBarCodesTrackedAsync(CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>()
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ProductBarCode?> GetBarCodeByIdAsync(Guid barCodeId, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().FindAsync(new object[] { barCodeId }, ct);
    }

    /// <inheritdoc />
    public async Task<ProductBarCode?> FirstOrDefaultBarCodeAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<bool> BarCodeExistsAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AnyAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<ProductBarCode?> FirstOrDefaultBarCodeTrackedAsync(Expression<Func<ProductBarCode, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().FirstOrDefaultAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TResult>> SelectBarCodesAsync<TResult>(Expression<Func<ProductBarCode, TResult>> selector, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Select(selector)
            .ToListAsync(ct);
    }


    // ── إدارة صور المنتج (Product Images) ─────────────────────────

    /// <inheritdoc />
    public async Task AddImageAsync(ProductImage image, CancellationToken ct = default)
    {
        await _context.Set<ProductImage>().AddAsync(image, ct);
    }

    /// <inheritdoc />
    public async Task AddImagesAsync(IEnumerable<ProductImage> images, CancellationToken ct = default)
    {
        await _context.Set<ProductImage>().AddRangeAsync(images, ct);
    }

    /// <inheritdoc />
    public void UpdateImage(ProductImage image)
    {
        var local = _context.Set<ProductImage>().Local.FirstOrDefault(e => e.Id == image.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, image))
                _context.Entry(local).CurrentValues.SetValues(image);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(image);
        if (entry.State == EntityState.Detached)
            _context.Set<ProductImage>().Attach(image);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void RemoveImage(ProductImage image)
    {
        _context.Set<ProductImage>().Remove(image);
    }

    /// <inheritdoc />
    public void SoftDeleteImage(ProductImage image)
    {
        image.IsDeleted = true;
        var local = _context.Set<ProductImage>().Local.FirstOrDefault(e => e.Id == image.Id);
        if (local != null)
        {
            if (!ReferenceEquals(local, image))
                _context.Entry(local).CurrentValues.SetValues(image);
            else
                _context.Entry(local).State = EntityState.Modified;
            return;
        }

        var entry = _context.Entry(image);
        if (entry.State == EntityState.Detached)
            _context.Set<ProductImage>().Attach(image);
        entry.State = EntityState.Modified;
    }

    /// <inheritdoc />
    public void HardDeleteImage(ProductImage image)
    {
        var local = _context.Set<ProductImage>().Local.FirstOrDefault(e => e.Id == image.Id);
        if (local != null)
        {
            _context.Set<ProductImage>().Remove(local);
        }
        else
        {
            var entry = _context.Entry(image);
            if (entry.State != EntityState.Detached)
                _context.Set<ProductImage>().Remove(image);
            else
            {
                _context.Attach(image);
                _context.Set<ProductImage>().Remove(image);
            }
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductImage>> GetImagesAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>()
            .AsNoTracking()
            .Where(i => i.ProductId == productId)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductImage>> FindImagesAsync(Expression<Func<ProductImage, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>()
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductImage>> FindImagesTrackedAsync(Expression<Func<ProductImage, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>()
            .Where(predicate)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<ProductImage?> GetImageByIdAsync(Guid imageId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().FindAsync(new object[] { imageId }, ct);
    }

    /// <inheritdoc />
    public async Task<bool> ImageExistsAsync(Expression<Func<ProductImage, bool>> predicate, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().AnyAsync(predicate, ct);
    }

    /// <inheritdoc />
    public async Task<int> CountImagesAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().CountAsync(i => i.ProductId == productId, ct);
    }
}
