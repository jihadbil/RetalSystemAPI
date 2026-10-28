using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// استعلامات المنتجات والباركودات والوحدات والصور، وعمليات التصدير والاستيراد.
/// </summary>
public partial class ProductRepository
{
    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarCode>> SearchBarcodesAsync(BarcodeQuery request, CancellationToken ct = default)
    {
        // استعلام الباركودات مع تفاصيلها باستخدام المواصفة المخصصة كقراءة فقط
        return await SpecificationEvaluator<ProductBarCode>
            .GetQuery(_context.Set<ProductBarCode>(), new AllProductBarCodesSpec(request.Search))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesWithDetailsAsync(Guid productId, CancellationToken ct = default)
    {
        // استرجاع باركودات المنتج مع العلاقات الملاحية عبر مواصفة ProductBarCodesByProductSpec
        return await SpecificationEvaluator<ProductBarCode>
            .GetQuery(_context.Set<ProductBarCode>(), new ProductBarCodesByProductSpec(productId))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> BarCodeExistsAsync(string barCode, CancellationToken ct = default)
    {
        // فحص وجود أي باركود مطابق في قاعدة البيانات للمستأجر الحالي
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .AnyAsync(e => e.BarCode == barCode, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> BarCodeExistsAsync(string barCode, Guid barCodeId, CancellationToken ct = default)
    {
        // فحص وجود باركود مطابق مع استبعاد سجل الباركود الحالي قيد التعديل
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .AnyAsync(e => e.BarCode == barCode && e.Id != barCodeId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductExportRow>> GetExportRowsAsync(CancellationToken ct = default)
    {
        // إسقاط بيانات الأصناف الأساسية بصيغة خفيفة لتصديرها إلى Excel
        return await _context.Set<Product>()
            .AsNoTracking()
            .Select(e => new ProductExportRow(e.Id, e.Name, e.CostPrice, e.SalePrice, e.CategoryId, e.Description))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarcodeExportRow>> GetBarcodeExportRowsAsync(CancellationToken ct = default)
    {
        // إسقاط بيانات الباركودات بصيغة التصدير لشيت الباركودات
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Select(e => new ProductBarcodeExportRow(e.Id, e.ProductId, e.BarCode, e.Title))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductUnitExportRow>> GetUnitExportRowsAsync(CancellationToken ct = default)
    {
        // إسقاط بيانات وحدات الأصناف بصيغة التصدير لشيت الوحدات
        return await _context.Set<ProductUnit>()
            .AsNoTracking()
            .Select(e => new ProductUnitExportRow(e.ProductId, e.UnitId, e.ConversionFactor, e.IsDefault))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductLookupRow>> GetLookupAsync(CancellationToken ct = default)
    {
        // استخراج معرف واسم المنتج فقط لسرعة التحميل في القوائم المنسدلة
        return await _context.Set<Product>()
            .AsNoTracking()
            .Select(e => new ProductLookupRow(e.Id, e.Name))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarcodeLookupRow>> GetBarcodeLookupAsync(CancellationToken ct = default)
    {
        // استخراج الباركود ومعرف المنتج للقوائم السريعة
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Select(e => new ProductBarcodeLookupRow(e.BarCode, e.ProductId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Product>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default)
    {
        // جلب الأصناف المطابقة لمفاتيح الاستيراد (المعرف أو الاسم أو الباركود) بدفعة استعلام موحدة
        return await ImportQueryBatch.ReadAsync(
            keys,
            (values, ids) => _context.Set<Product>().Where(e => ids.Contains(e.Id) || values.Contains(e.Name.Trim().ToUpper()) || e.ProductBarCodes.Any(b => values.Contains(b.BarCode.Trim().ToUpper()))),
            ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductImage>> GetImagesByProductAsync(Guid productId, CancellationToken ct = default)
    {
        // استرجاع صور المنتج كقراءة فقط بدون تتبع
        return await _context.Set<ProductImage>()
            .AsNoTracking()
            .Where(e => e.ProductId == productId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> BarcodeBelongsToProductAsync(Guid barcodeId, Guid productId, CancellationToken ct = default)
    {
        // التحقق من تبعية الباركود للمنتج المعطى
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == barcodeId && e.ProductId == productId, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasImagesAsync(Guid productId, CancellationToken ct = default)
    {
        // التحقق من وجود أي صورة مسجلة للمنتج
        return await _context.Set<ProductImage>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == productId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductImage>> GetDefaultImagesForUpdateAsync(Guid productId, CancellationToken ct = default)
    {
        // جلب الصور الافتراضية للمنتج لتعديل حالتها في الذاكرة
        return await _context.Set<ProductImage>()
            .Where(e => e.ProductId == productId && e.IsDefault)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductImage>> GetImagesForUpdateAsync(Guid productId, CancellationToken ct = default)
    {
        // جلب صور المنتج ككيانات متتبعة للتعديل
        return await _context.Set<ProductImage>()
            .Where(e => e.ProductId == productId)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Product>> ListAsync(ProductQuery request, CancellationToken ct = default)
    {
        // تنفيذ استعلام قائمة الأصناف المفلترة بكافة تفاصيلها
        return await BuildProductList(request)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Product>> SearchLookupAsync(string search, CancellationToken ct = default)
    {
        // استعلام البحث السريع مع الترتيب الذكي (مطابقة الباركود أولاً، ثم بداية الاسم، ثم الترتيب الأبجدي)
        return await BuildProductList(new ProductQuery { Search = search })
            .OrderBy(p => p.ProductBarCodes.Any(b => b.BarCode == search) ? 0 : 1)
            .ThenBy(p => p.Name.StartsWith(search) ? 0 : 1)
            .ThenBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Take(100)
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<Product?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع الصنف مع كافة تفاصيله الملاحية عبر مواصفة ProductWithDetailsSpec
        return await SpecificationEvaluator<Product>
            .GetQuery(_context.Set<Product>(), new ProductWithDetailsSpec(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<PageResult<Product>> SearchAsync(ProductQuery request, CancellationToken ct = default)
    {
        // تطبيق البحث والتصفح بنظام الصفحات للأصناف
        return await QueryPaging.ReadAsync(BuildProductList(request), request, ct);
    }

    /// <inheritdoc/>
    public async Task<ProductBarCode?> GetBarcodeByValueAsync(string barCode, CancellationToken ct = default)
    {
        // البحث عن الباركود بمطابقة القيمة النصية كقراءة فقط
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.BarCode == barCode, ct);
    }

    /// <inheritdoc/>
    public async Task<ProductBarCode?> GetFirstMatchingBarcodeAsync(IEnumerable<string> barCodeStrings, CancellationToken ct = default)
    {
        // البحث عن أول باركود مطابق لأي من النصوص الممررة
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => barCodeStrings.Contains(e.BarCode), ct);
    }

    /// <inheritdoc/>
    public async Task<Product?> GetDetailsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل الصنف مع وحداته وباركوداته وصوره متتبعاً للتعديل الذري
        return await SpecificationEvaluator<Product>
            .GetQuery(_context.Set<Product>(), new ProductWithDetailsSpec(id))
            .FirstOrDefaultAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> BarcodesExistOutsideProductAsync(IEnumerable<string> barCodeStrings, Guid id, CancellationToken ct = default)
    {
        // فحص وجود أي باركود مستخدم لدى منتج آخر
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .AnyAsync(e => barCodeStrings.Contains(e.BarCode) && e.ProductId != id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductImage>> GetImagesByBarcodesForUpdateAsync(IEnumerable<Guid> removedBarcodeIds, CancellationToken ct = default)
    {
        // جلب الصور المرتبطة بقائمة باركودات محددة لحذفها
        return await _context.Set<ProductImage>()
            .Where(e => removedBarcodeIds.Contains(e.BarcodeId!.Value))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesByProductAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع باركودات المنتج كقراءة فقط
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Where(e => e.ProductId == id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع باركودات المنتج متتبعة للتعديل
        return await _context.Set<ProductBarCode>()
            .Where(e => e.ProductId == id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductUnit>> GetUnitsForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        // استرجاع وحدات المنتج متتبعة للتعديل
        return await _context.Set<ProductUnit>()
            .Where(e => e.ProductId == id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductUnit>> GetUnitsWithDetailsAsync(Guid productId, CancellationToken ct = default)
    {
        // استرجاع وحدات المنتج متضمنة بيانات الوحدة الأساسية
        return await SpecificationEvaluator<ProductUnit>
            .GetQuery(_context.Set<ProductUnit>(), new ProductUnitsByProductSpec(productId))
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> ProductUnitExistsAsync(Guid productId, Guid unitId, CancellationToken ct = default)
    {
        // التحقق من وجود ربط مسبق بين المنتج والوحدة
        return await _context.Set<ProductUnit>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == productId && e.UnitId == unitId, ct);
    }

    /// <inheritdoc/>
    public async Task<bool> HasUnitsAsync(Guid productId, CancellationToken ct = default)
    {
        // التحقق من وجود أي وحدات مسجلة للمنتج
        return await _context.Set<ProductUnit>()
            .AsNoTracking()
            .AnyAsync(e => e.ProductId == productId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductUnit>> GetDefaultUnitsForUpdateAsync(Guid productId, CancellationToken ct = default)
    {
        // جلب الوحدات الافتراضية للمنتج ككيانات متتبعة للتعديل
        return await _context.Set<ProductUnit>()
            .Where(e => e.ProductId == productId && e.IsDefault)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> IsUnitInUseAsync(Guid id, CancellationToken ct = default)
    {
        // التحقق من استخدام وحدة القياس في أي منتج
        return await _context.Set<ProductUnit>()
            .AsNoTracking()
            .AnyAsync(e => e.UnitId == id, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesByProductIdsAsync(IEnumerable<Guid> fallbackProductIds, CancellationToken ct = default)
    {
        // جلب باركودات قائمة منتجات محددة
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Where(e => fallbackProductIds.Contains(e.ProductId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Product>> GetByIdsForUpdateAsync(IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        // استرجاع مجموعة منتجات ككيانات متتبعة للتعديل
        return await _context.Set<Product>()
            .Where(e => productIds.Contains(e.Id))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarCode>> GetBarcodesByIdsAsync(IEnumerable<Guid> barcodeIds, CancellationToken ct = default)
    {
        // استرجاع مجموعة باركودات بالمعرفات كقراءة فقط
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Where(e => barcodeIds.Contains(e.Id))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Product>> GetByIdsAsync(IEnumerable<Guid> itemProductIds, CancellationToken ct = default)
    {
        // استرجاع مجموعة منتجات بالمعرفات كقراءة فقط
        return await _context.Set<Product>()
            .AsNoTracking()
            .Where(e => itemProductIds.Contains(e.Id))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductBarcodeOwnershipRow>> GetBarcodeOwnershipAsync(CancellationToken ct = default)
    {
        // استخراج ملكية الباركودات كبيانات خفيفة
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Select(e => new ProductBarcodeOwnershipRow(e.Id, e.TenantId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<bool> BarcodeIdExistsAsync(Guid productBarcodeId, CancellationToken ct = default)
    {
        // فحص وجود معرف باركود في النظام
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .AnyAsync(e => e.Id == productBarcodeId, ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ProductOwnershipRow>> GetOwnershipAsync(CancellationToken ct = default)
    {
        // استخراج ملكية المنتجات كبيانات خفيفة
        return await _context.Set<Product>()
            .AsNoTracking()
            .Select(e => new ProductOwnershipRow(e.Id, e.TenantId))
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guid>> GetBarcodeIdsAsync(CancellationToken ct = default)
    {
        // جلب قائمة بكافة معرفات الباركودات المسجلة
        return await _context.Set<ProductBarCode>()
            .AsNoTracking()
            .Select(e => e.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guid>> GetIdsAsync(CancellationToken ct = default)
    {
        // جلب قائمة بكافة معرفات المنتجات المسجلة
        return await _context.Set<Product>()
            .AsNoTracking()
            .Select(e => e.Id)
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ProductBarCode>> GetImportBarcodesForUpdateAsync(ImportKeys keys, IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        // دمج معرفات المنتجات مع مفاتيح الاستيراد لجلب الباركودات المرشحة للتحديث
        var related = keys with
        {
            Ids = keys.Ids.Concat(productIds).Distinct().ToArray()
        };
        return ImportQueryBatch.ReadAsync(
            related,
            (values, ids) => _context.ProductBarCodes.Where(b => ids.Contains(b.Id) || ids.Contains(b.ProductId) || values.Contains(b.BarCode.Trim().ToUpper())),
            ct);
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<ProductUnit>> GetImportUnitsForUpdateAsync(IEnumerable<Guid> productIds, CancellationToken ct = default)
    {
        // استرجاع وحدات المنتجات المرشحة للاستيراد بدفعة واحدة
        return ImportQueryBatch.ByIdsAsync(productIds, ids => _context.ProductUnits.Where(u => ids.Contains(u.ProductId)), ct);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetBarcodeValuesAsync(CancellationToken ct = default)
    {
        // استخراج كافة نصوص الباركودات المسجلة للتحقق من عدم التكرار
        return await _context.ProductBarCodes
            .AsNoTracking()
            .Select(b => b.BarCode)
            .ToListAsync(ct);
    }

    /// <summary>
    /// دالة مساعدة مركزية لبناء استعلام استرجاع الأصناف مع فلاتر التصنيف والبحث وتضمين العلاقات.
    /// </summary>
    /// <param name="request">معايير وفلاتر الاستعلام.</param>
    /// <returns>استعلام IQueryable للأصناف متضمناً العلاقات الملاحية الأساسية.</returns>
    private IQueryable<Product> BuildProductList(ProductQuery request)
    {
        // بناء الاستعلام وتطبيق فلاتر التصنيف والبحث النصي وتضمين التصنيف والباركودات والصور
        return _context.Products
            .Where(p => (!request.CategoryId.HasValue || p.CategoryId == request.CategoryId.Value) &&
                        (string.IsNullOrWhiteSpace(request.Search) || p.Name.Contains(request.Search) || p.ProductBarCodes.Any(b => b.BarCode.Contains(request.Search))))
            .Include(p => p.Category)
            .Include(p => p.ProductBarCodes)
            .Include(p => p.ProductImages)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id);
    }
}
