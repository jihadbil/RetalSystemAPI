using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;
public partial class ProductRepository
{
    public async Task<IReadOnlyList<ProductBarCode>> SearchBarcodesAsync(BarcodeQuery request, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ProductBarCode>.GetQuery(_context.Set<ProductBarCode>(), new AllProductBarCodesSpec(request.Search)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesWithDetailsAsync(Guid productId, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ProductBarCode>.GetQuery(_context.Set<ProductBarCode>(), new ProductBarCodesByProductSpec(productId)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<bool> BarCodeExistsAsync(string barCode, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().AnyAsync(e => e.BarCode == barCode, ct);
    }

    public async Task<bool> BarCodeExistsAsync(string barCode, Guid barCodeId, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().AnyAsync(e => e.BarCode == barCode && e.Id != barCodeId, ct);
    }

    public async Task<IReadOnlyList<ProductExportRow>> GetExportRowsAsync(CancellationToken ct = default)
    {
        return await _context.Set<Product>().AsNoTracking().Select(e => new ProductExportRow(e.Id, e.Name, e.CostPrice, e.SalePrice, e.CategoryId, e.Description)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductBarcodeExportRow>> GetBarcodeExportRowsAsync(CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().Select(e => new ProductBarcodeExportRow(e.Id, e.ProductId, e.BarCode, e.Title)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductUnitExportRow>> GetUnitExportRowsAsync(CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().AsNoTracking().Select(e => new ProductUnitExportRow(e.ProductId, e.UnitId, e.ConversionFactor, e.IsDefault)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductLookupRow>> GetLookupAsync(CancellationToken ct = default)
    {
        return await _context.Set<Product>().AsNoTracking().Select(e => new ProductLookupRow(e.Id, e.Name)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductBarcodeLookupRow>> GetBarcodeLookupAsync(CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().Select(e => new ProductBarcodeLookupRow(e.BarCode, e.ProductId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default)
    {
        return await ImportQueryBatch.ReadAsync(keys, (values, ids) => _context.Set<Product>().Where(e => ids.Contains(e.Id) || values.Contains(e.Name.Trim().ToUpper()) || e.ProductBarCodes.Any(b => values.Contains(b.BarCode.Trim().ToUpper()))), ct);
    }

    public async Task<IReadOnlyList<ProductImage>> GetImagesByProductAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().AsNoTracking().Where(e => e.ProductId == productId).ToListAsync(ct);
    }

    public async Task<bool> BarcodeBelongsToProductAsync(Guid barcodeId, Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().AnyAsync(e => e.Id == barcodeId && e.ProductId == productId, ct);
    }

    public async Task<bool> HasImagesAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().AsNoTracking().AnyAsync(e => e.ProductId == productId, ct);
    }

    public async Task<IReadOnlyList<ProductImage>> GetDefaultImagesForUpdateAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().Where(e => e.ProductId == productId && e.IsDefault).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductImage>> GetImagesForUpdateAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().Where(e => e.ProductId == productId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> ListAsync(ProductQuery request, CancellationToken ct = default)
    {
        return await BuildProductList(request).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<Product?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Product>.GetQuery(_context.Set<Product>(), new ProductWithDetailsSpec(id)).AsNoTracking().FirstOrDefaultAsync(ct);
    }

    public async Task<PageResult<Product>> SearchAsync(ProductQuery request, CancellationToken ct = default)
    {
        return await QueryPaging.ReadAsync(BuildProductList(request), request, ct);
    }

    public async Task<ProductBarCode?> GetBarcodeByValueAsync(string barCode, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().FirstOrDefaultAsync(e => e.BarCode == barCode, ct);
    }

    public async Task<ProductBarCode?> GetFirstMatchingBarcodeAsync(IEnumerable<string> barCodeStrings, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().FirstOrDefaultAsync(e => barCodeStrings.Contains(e.BarCode), ct);
    }

    public async Task<Product?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<Product>.GetQuery(_context.Set<Product>(), new ProductWithDetailsSpec(id)).FirstOrDefaultAsync(ct);
    }

    public async Task<bool> BarcodesExistOutsideProductAsync(IEnumerable<string> barCodeStrings, Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().AnyAsync(e => barCodeStrings.Contains(e.BarCode) && e.ProductId != id, ct);
    }

    public async Task<IReadOnlyList<ProductImage>> GetImagesByBarcodesForUpdateAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<ProductImage>().Where(e => removedBarcodeIds.Contains(e.BarcodeId!.Value)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesByProductAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().Where(e => e.ProductId == id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().Where(e => e.ProductId == id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductUnit>> GetUnitsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().Where(e => e.ProductId == id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductUnit>> GetUnitsWithDetailsAsync(Guid productId, CancellationToken ct = default)
    {
        return await SpecificationEvaluator<ProductUnit>.GetQuery(_context.Set<ProductUnit>(), new ProductUnitsByProductSpec(productId)).AsNoTracking().AsSplitQuery().ToListAsync(ct);
    }

    public async Task<bool> ProductUnitExistsAsync(Guid productId, Guid unitId, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().AsNoTracking().AnyAsync(e => e.ProductId == productId && e.UnitId == unitId, ct);
    }

    public async Task<bool> HasUnitsAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().AsNoTracking().AnyAsync(e => e.ProductId == productId, ct);
    }

    public async Task<IReadOnlyList<ProductUnit>> GetDefaultUnitsForUpdateAsync(Guid productId, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().Where(e => e.ProductId == productId && e.IsDefault).ToListAsync(ct);
    }

    public async Task<bool> IsUnitInUseAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Set<ProductUnit>().AsNoTracking().AnyAsync(e => e.UnitId == id, ct);
    }

    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesByProductIdsAsync(IEnumerable<Guid> fallbackProductIds, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().Where(e => fallbackProductIds.Contains(e.ProductId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsForUpdateAsync(IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        return await _context.Set<Product>().Where(e => productIds.Contains(e.Id)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesByIdsAsync(IEnumerable<Guid> barcodeIds, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().Where(e => barcodeIds.Contains(e.Id)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> itemProductIds, CancellationToken ct = default)
    {
        return await _context.Set<Product>().AsNoTracking().Where(e => itemProductIds.Contains(e.Id)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<ProductBarcodeOwnershipRow>> GetBarcodeOwnershipAsync(CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().Select(e => new ProductBarcodeOwnershipRow(e.Id, e.TenantId)).ToListAsync(ct);
    }

    public async Task<bool> BarcodeIdExistsAsync(Guid productBarcodeId, CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().AnyAsync(e => e.Id == productBarcodeId, ct);
    }

    public async Task<IReadOnlyList<ProductOwnershipRow>> GetOwnershipAsync(CancellationToken ct = default)
    {
        return await _context.Set<Product>().AsNoTracking().Select(e => new ProductOwnershipRow(e.Id, e.TenantId)).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetBarcodeIdsAsync(CancellationToken ct = default)
    {
        return await _context.Set<ProductBarCode>().AsNoTracking().Select(e => e.Id).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetIdsAsync(CancellationToken ct = default)
    {
        return await _context.Set<Product>().AsNoTracking().Select(e => e.Id).ToListAsync(ct);
    }

    public Task<IReadOnlyList<ProductBarCode>> GetImportBarcodesForUpdateAsync(ImportKeys keys, IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        var related = keys with
        {
            Ids = keys.Ids.Concat(productIds).Distinct().ToArray()
        };
        return ImportQueryBatch.ReadAsync(related, (values, ids) => _context.ProductBarCodes.Where(b => ids.Contains(b.Id) || ids.Contains(b.ProductId) || values.Contains(b.BarCode.Trim().ToUpper())), ct);
    }

    public Task<IReadOnlyList<ProductUnit>> GetImportUnitsForUpdateAsync(IEnumerable<Guid> productIds, CancellationToken ct = default) => ImportQueryBatch.ByIdsAsync(productIds, ids => _context.ProductUnits.Where(u => ids.Contains(u.ProductId)), ct);
    public async Task<IReadOnlyList<string>> GetBarcodeValuesAsync(CancellationToken ct = default) => await _context.ProductBarCodes.AsNoTracking().Select(b => b.BarCode).ToListAsync(ct);
    private IQueryable<Product> BuildProductList(ProductQuery request) => _context.Products.Where(p => (!request.CategoryId.HasValue || p.CategoryId == request.CategoryId.Value) && (string.IsNullOrWhiteSpace(request.Search) || p.Name.Contains(request.Search) || p.ProductBarCodes.Any(b => b.BarCode.Contains(request.Search)))).Include(p => p.Category).Include(p => p.ProductBarCodes).Include(p => p.ProductImages).OrderBy(p => p.Name).ThenBy(p => p.Id);
}
