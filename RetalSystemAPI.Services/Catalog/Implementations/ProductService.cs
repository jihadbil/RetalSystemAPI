using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Product;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Catalog.Specifications;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة كتالوج المنتجات والأصناف والباركودات وتوليد سجلات الأرصدة الافتتاحية في المخازن والصالات.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة المنتجات مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> GetAllAsync(Guid? categoryId = null, CancellationToken ct = default)
    {
        var spec = new ProductSummarySpec(categoryId);
        var products = await _unitOfWork.Products.FindAsync(spec, ct);
        var result = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(products);
        return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(new ProductWithDetailsSpec(id), ct);
        if (product is null)
        {
            return ServiceResult<ProductResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        var result = _mapper.Map<ProductResponseDto>(product);
        return ServiceResult<ProductResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<ProductSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? categoryId = null,
        CancellationToken ct = default)
    {
        var spec = new ProductSummarySpec(categoryId);
        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(items);
        var pagedResult = PagedResult<ProductSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<ProductSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(Array.Empty<ProductSummaryDto>());
        }

        var spec = new ProductSearchSpec(query);
        var products = await _unitOfWork.Products.FindAsync(spec, ct);

        // التطابق التام للباركود أولاً ثم الأسماء التي تبدأ بالكلمة ثم بقية النتائج
        var ordered = products
            .OrderBy(p => p.ProductBarCodes.Any(b => b.BarCode == query) ? 0 : 1)
            .ThenBy(p => p.Name.StartsWith(query) ? 0 : 1)
            .ThenBy(p => p.Name)
            .ToList();

        var dtos = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(ordered);

        return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default)
    {
        var barCodeEntry = await _unitOfWork.ProductBarCodes.FirstOrDefaultAsync(b => b.BarCode == barCode, ct);
        if (barCodeEntry is null)
        {
            return ServiceResult<ProductResponseDto>.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        return await GetByIdAsync(barCodeEntry.ProductId, ct);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        bool categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == dto.CategoryId, ct);
        if (!categoryExists)
        {
            return ServiceResult<ProductResponseDto>.Failure("التصنيف المحدد غير موجود", ErrorCodes.CategoryNotFound);
        }

        if (dto.CostPrice < 0 || dto.SalePrice < 0)
        {
            return ServiceResult<ProductResponseDto>.Failure("أسعار المنتج يجب أن لا تكون سالبة", ErrorCodes.ValidationError);
        }

        var product = _mapper.Map<Product>(dto);

        if (dto.Units != null && dto.Units.Count > 0)
        {
            product.ProductUnits = dto.Units.Select(u => new ProductUnit
            {
                UnitId = u.UnitId,
                ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1,
                IsDefault = u.IsDefault
            }).ToList();
        }

        if (dto.BarCodes != null && dto.BarCodes.Count > 0)
        {
            var barCodeStrings = dto.BarCodes.Select(b => b.BarCode.Trim()).ToList();
            bool duplicateInDto = barCodeStrings.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1);
            if (duplicateInDto)
            {
                return ServiceResult<ProductResponseDto>.Failure("يوجد باركود مكرر في البيانات المدخلة", ErrorCodes.BarCodeDuplicate);
            }

            var existingBarcode = await _unitOfWork.ProductBarCodes.FirstOrDefaultAsync(
                b => barCodeStrings.Contains(b.BarCode), ct);
            if (existingBarcode != null)
            {
                return ServiceResult<ProductResponseDto>.Failure($"الباركود '{existingBarcode.BarCode}' مسجل مسبقاً لصنف آخر في النظام", ErrorCodes.BarCodeDuplicate);
            }

            product.ProductBarCodes = dto.BarCodes.Select(b => new ProductBarCode
            {
                BarCode = b.BarCode.Trim(),
                Title = string.IsNullOrWhiteSpace(b.Title) ? dto.Name : b.Title,
                Description = b.Description
            }).ToList();
        }

        await _unitOfWork.Products.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // 1. توليد سجل مخزون الصالة لكل صالة عرض قائمة (مع التراجع لجلب كافة المخازن إن لم تتوفر صالات مصنفة)
        var showrooms = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Show, ct);
        if (showrooms.Count == 0)
        {
            showrooms = await _unitOfWork.Warehouses.GetAllAsync(ct);
        }

        var showroomQtyMap = dto.ShowroomInitialQuantities?.ToDictionary(s => s.WarehouseId, s => s.Quantity) ?? new Dictionary<Guid, int>();

        foreach (var show in showrooms)
        {
            int initShowQty = 0;
            if (showroomQtyMap.TryGetValue(show.Id, out int showQty))
            {
                initShowQty = showQty;
            }
            else if (dto.ShowroomWarehouseId.HasValue)
            {
                if (show.Id == dto.ShowroomWarehouseId.Value)
                {
                    initShowQty = dto.InitialShowroomQuantity;
                }
            }
            else
            {
                if (showrooms.Count > 0 && show.Id == showrooms[0].Id)
                {
                    initShowQty = dto.InitialShowroomQuantity;
                }
            }

            var showStock = new ShowroomStock
            {
                TenantId = product.TenantId,
                WarehouseId = show.Id,
                ProductId = product.Id,
                Quantity = initShowQty,
                MinStockLevel = 0
            };
            await _unitOfWork.ShowroomStocks.AddAsync(showStock, ct);
        }

        // 2. توليد سجلات مخزون التخزين لكل باركود/نكهة في كل مخزن تخزين قائم (مع التراجع لجلب كافة المخازن)
        var storgeWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Storge, ct);
        if (storgeWarehouses.Count == 0)
        {
            storgeWarehouses = await _unitOfWork.Warehouses.GetAllAsync(ct);
        }
        if (product.ProductBarCodes != null && product.ProductBarCodes.Count > 0)
        {
            var initialQtyMap = dto.BarCodes?.ToDictionary(b => b.BarCode, b => b.InitialQuantity) ?? new Dictionary<string, int>();
            var storageMultiQtyMap = dto.StorageInitialQuantities?.ToDictionary(s => $"{s.WarehouseId}_{s.BarCode}", s => s.Quantity) ?? new Dictionary<string, int>();

            foreach (var storgeWh in storgeWarehouses)
            {
                foreach (var bc in product.ProductBarCodes)
                {
                    int initQty = 0;
                    string key = $"{storgeWh.Id}_{bc.BarCode}";

                    if (storageMultiQtyMap.TryGetValue(key, out int multiQty))
                    {
                        initQty = multiQty;
                    }
                    else
                    {
                        int requestedQty = initialQtyMap.TryGetValue(bc.BarCode, out int q) ? q : 0;
                        if (dto.StorageWarehouseId.HasValue)
                        {
                            if (storgeWh.Id == dto.StorageWarehouseId.Value)
                            {
                                initQty = requestedQty;
                            }
                        }
                        else
                        {
                            if (storgeWarehouses.Count > 0 && storgeWh.Id == storgeWarehouses[0].Id)
                            {
                                initQty = requestedQty;
                            }
                        }
                    }

                    var storgeStock = new StorgeStock
                    {
                        TenantId = product.TenantId,
                        WarehouseId = storgeWh.Id,
                        ProductBarcodeId = bc.Id,
                        Quantity = initQty,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.StorgeStocks.AddAsync(storgeStock, ct);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var createdProduct = await _unitOfWork.Products.FirstOrDefaultAsync(new ProductWithDetailsSpec(product.Id), ct) ?? product;
        var responseDto = _mapper.Map<ProductResponseDto>(createdProduct);

        return ServiceResult<ProductResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        // تحميل متتبع (Tracked) لضمان حفظ تعديلات الأبناء (باركودات/وحدات) وحذف المحذوف منها فعلياً،
        // وتفادي تضارب تتبع النسخ المكررة عند تكرار نفس الصنف في البنود.
        var product = await _unitOfWork.Products.FirstOrDefaultTrackedAsync(new ProductWithDetailsSpec(id), ct);
        if (product is null)
        {
            return ServiceResult<ProductResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        if (dto.CategoryId != product.CategoryId)
        {
            bool categoryExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == dto.CategoryId, ct);
            if (!categoryExists)
            {
                return ServiceResult<ProductResponseDto>.Failure("التصنيف المحدد غير موجود", ErrorCodes.CategoryNotFound);
            }
        }

        if (dto.CostPrice < 0 || dto.SalePrice < 0)
        {
            return ServiceResult<ProductResponseDto>.Failure("أسعار المنتج يجب أن لا تكون سالبة", ErrorCodes.ValidationError);
        }

        // Validate barcodes uniqueness if provided
        if (dto.BarCodes != null && dto.BarCodes.Count > 0)
        {
            var barCodeStrings = dto.BarCodes.Select(b => b.BarCode.Trim()).ToList();
            bool duplicateInDto = barCodeStrings.GroupBy(x => x, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1);
            if (duplicateInDto)
            {
                return ServiceResult<ProductResponseDto>.Failure("يوجد باركود مكرر في البيانات المدخلة", ErrorCodes.BarCodeDuplicate);
            }

            var existingWithOtherProduct = await _unitOfWork.ProductBarCodes.ExistsAsync(
                b => barCodeStrings.Contains(b.BarCode) && b.ProductId != id, ct);
            if (existingWithOtherProduct)
            {
                return ServiceResult<ProductResponseDto>.Failure("أحد الباركودات المدخلة مستخدم بالفعل لمنتج آخر", ErrorCodes.BarCodeDuplicate);
            }
        }

        _mapper.Map(dto, product);
        product.Id = id;

        if (dto.Units != null)
        {
            var dtoUnitIds = dto.Units.Select(u => u.UnitId).ToHashSet();

            // 1. Remove units no longer in DTO
            var unitsToRemove = product.ProductUnits.Where(pu => !dtoUnitIds.Contains(pu.UnitId)).ToList();
            foreach (var u in unitsToRemove)
            {
                product.ProductUnits.Remove(u);
            }

            // 2. Update existing or add new units
            foreach (var u in dto.Units)
            {
                var existingUnit = product.ProductUnits.FirstOrDefault(pu => pu.UnitId == u.UnitId);
                if (existingUnit != null)
                {
                    existingUnit.ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1;
                    existingUnit.IsDefault = u.IsDefault;
                }
                else
                {
                    product.ProductUnits.Add(new ProductUnit
                    {
                        ProductId = id,
                        UnitId = u.UnitId,
                        ConversionFactor = u.ConversionFactor > 0 ? u.ConversionFactor : 1,
                        IsDefault = u.IsDefault
                    });
                }
            }
        }

        if (dto.BarCodes != null)
        {
            var dtoBarCodes = dto.BarCodes.Select(b => b.BarCode.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 1. حذف الباركودات المستبعدة من الطلب حذفاً منطقياً صريحاً
            //    (المنتج محمّل بدون تتبع، وإزالته من المجموعة وحدها لا تصل إلى EF Core)
            var barcodesToRemove = product.ProductBarCodes
                .Where(pb => !dtoBarCodes.Contains(pb.BarCode.Trim()))
                .ToList();

            if (barcodesToRemove.Count > 0)
            {
                var removedBarcodeIds = barcodesToRemove.Select(b => b.Id).ToList();

                // منع حذف باركود له حركة مخزنية أو فواتير أو صور مرتبطة
                bool usedInSales = await _unitOfWork.SalesInvoiceItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId!.Value), ct);
                bool usedInSalesReturns = await _unitOfWork.SalesReturnItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId!.Value), ct);
                bool usedInPurchases = await _unitOfWork.PurchaseInvoiceItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId!.Value), ct);
                bool usedInPurchaseReturns = await _unitOfWork.PurchaseReturnItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId!.Value), ct);
                bool usedInOrders = await _unitOfWork.PurchaseOrderItems.ExistsAsync(
                    i => removedBarcodeIds.Contains(i.ProductBarCodeId), ct);
                bool hasStock = await _unitOfWork.StorgeStocks.ExistsAsync(
                    s => removedBarcodeIds.Contains(s.ProductBarcodeId) && s.Quantity != 0, ct);

                if (usedInSales || usedInSalesReturns || usedInPurchases || usedInPurchaseReturns || usedInOrders || hasStock)
                {
                    return ServiceResult<ProductResponseDto>.Failure(
                        "لا يمكن حذف الباركود '" + string.Join("', '", barcodesToRemove.Select(b => b.BarCode)) +
                        "' لوجود حركة مخزنية أو فواتير مرتبطة به. يرجى الاحتفاظ به أو حذف الأصناف المرتبطة أولاً.",
                        ErrorCodes.BarCodeInUse);
                }

                // حذف منطقي متسلسل: الصور المرتبطة ثم أرصدة المخازن ثم الباركود نفسه
                // (FindTrackedAsync بدل FindAsync حتى لا تتضارب النسخ مع ربط الـ graph لاحقاً)
                var linkedImages = await _unitOfWork.ProductImages.FindTrackedAsync(
                    img => removedBarcodeIds.Contains(img.BarcodeId!.Value), ct);
                foreach (var img in linkedImages)
                {
                    _unitOfWork.ProductImages.SoftDelete(img);
                }

                var linkedStocks = await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => removedBarcodeIds.Contains(s.ProductBarcodeId), ct);
                foreach (var stock in linkedStocks)
                {
                    _unitOfWork.StorgeStocks.SoftDelete(stock);
                }

                foreach (var b in barcodesToRemove)
                {
                    // SoftDelete وحده يكفي مع الكيان المتتبع؛ إزالته من المجموعة
                    // تجعل EF Core يعلمه Deleted (حذف فعلي) بسبب علاقة Cascade
                    _unitOfWork.ProductBarCodes.SoftDelete(b);
                }
            }

            // 2. Update existing or add new barcodes
            var newlyAddedBarCodes = new List<ProductBarCode>();
            foreach (var b in dto.BarCodes)
            {
                var trimmedCode = b.BarCode.Trim();
                var existingBc = product.ProductBarCodes
                    .FirstOrDefault(pb => string.Equals(pb.BarCode.Trim(), trimmedCode, StringComparison.OrdinalIgnoreCase));

                if (existingBc != null)
                {
                    existingBc.Title = string.IsNullOrWhiteSpace(b.Title) ? product.Name : b.Title.Trim();
                    existingBc.Description = b.Description;
                }
                else
                {
                    var newBc = new ProductBarCode
                    {
                        ProductId = id,
                        BarCode = trimmedCode,
                        Title = string.IsNullOrWhiteSpace(b.Title) ? product.Name : b.Title.Trim(),
                        Description = b.Description
                    };
                    product.ProductBarCodes.Add(newBc);
                    newlyAddedBarCodes.Add(newBc);
                }
            }

            // 3. Initialize storage stock for any newly added barcodes
            if (newlyAddedBarCodes.Count > 0)
            {
                var storgeWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Storge, ct);
                if (storgeWarehouses.Count == 0)
                {
                    storgeWarehouses = await _unitOfWork.Warehouses.GetAllAsync(ct);
                }

                foreach (var storgeWh in storgeWarehouses)
                {
                    foreach (var newBc in newlyAddedBarCodes)
                    {
                        var storgeStock = new StorgeStock
                        {
                            TenantId = product.TenantId,
                            WarehouseId = storgeWh.Id,
                            ProductBarcode = newBc,
                            Quantity = 0,
                            MinStockLevel = 0
                        };
                        await _unitOfWork.StorgeStocks.AddAsync(storgeStock, ct);
                    }
                }
            }
        }

        try
        {
            // المنتج محمّل متتبعاً؛ SaveChanges يلتقط كل تعديلات الجذر والأبناء (إضافة/تعديل/حذف منطقي)
            await _unitOfWork.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult<ProductResponseDto>.Failure("حدث تعارض أثناء حفظ التعديلات، يرجى إعادة المحاولة", ErrorCodes.ConcurrencyError);
        }

        var updatedProduct = await _unitOfWork.Products.FirstOrDefaultAsync(new ProductWithDetailsSpec(id), ct) ?? product;
        var responseDto = _mapper.Map<ProductResponseDto>(updatedProduct);

        return ServiceResult<ProductResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return ServiceResult.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // منع حذف صنف له حركة مبيعات أو مشتريات أو أرصدة قائمة
        bool usedInSalesInvoices = await _unitOfWork.SalesInvoiceItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInSalesReturns = await _unitOfWork.SalesReturnItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInPurchaseInvoices = await _unitOfWork.PurchaseInvoiceItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInPurchaseReturns = await _unitOfWork.PurchaseReturnItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInStockTransfers = await _unitOfWork.StockTransferItems.ExistsAsync(i => i.ProductId == id, ct);
        bool usedInStockAdjustments = await _unitOfWork.StockAdjustmentItems.ExistsAsync(i => i.ProductId == id, ct);

        var barcodeIds = (await _unitOfWork.ProductBarCodes.FindAsync(b => b.ProductId == id, ct))
            .Select(b => b.Id)
            .ToList();

        bool usedInPurchaseOrders = barcodeIds.Count > 0 && await _unitOfWork.PurchaseOrderItems.ExistsAsync(
            i => barcodeIds.Contains(i.ProductBarCodeId), ct);

        bool hasStorgeStock = barcodeIds.Count > 0 && await _unitOfWork.StorgeStocks.ExistsAsync(
            s => barcodeIds.Contains(s.ProductBarcodeId) && s.Quantity != 0, ct);
        bool hasShowroomStock = await _unitOfWork.ShowroomStocks.ExistsAsync(
            s => s.ProductId == id && s.Quantity != 0, ct);

        if (usedInSalesInvoices || usedInSalesReturns || usedInPurchaseInvoices || usedInPurchaseReturns ||
            usedInStockTransfers || usedInStockAdjustments || usedInPurchaseOrders || hasStorgeStock || hasShowroomStock)
        {
            return ServiceResult.Failure(
                "لا يمكن حذف الصنف لوجود حركة مبيعات أو مشتريات أو أرصدة مخزنية مرتبطة به. يرجى تصفير أرصدته والتأكد من عدم ارتباطه بفواتير أو حركات مخزنية.",
                ErrorCodes.ProductInUse);
        }

        // حذف منطقي متسلسل للأبناء (الباركودات وصورها، الوحدات، صور المنتج، والأرصدة الصفرية)

        var productImages = await _unitOfWork.ProductImages.FindTrackedAsync(img => img.ProductId == id, ct);
        foreach (var img in productImages)
        {
            _unitOfWork.ProductImages.SoftDelete(img);
        }

        var productBarCodes = await _unitOfWork.ProductBarCodes.FindTrackedAsync(b => b.ProductId == id, ct);
        foreach (var bc in productBarCodes)
        {
            _unitOfWork.ProductBarCodes.SoftDelete(bc);
        }

        var productUnits = await _unitOfWork.ProductUnits.FindTrackedAsync(u => u.ProductId == id, ct);
        foreach (var unit in productUnits)
        {
            _unitOfWork.ProductUnits.SoftDelete(unit);
        }

        var showroomStocks = await _unitOfWork.ShowroomStocks.FindTrackedAsync(s => s.ProductId == id, ct);
        foreach (var stock in showroomStocks)
        {
            _unitOfWork.ShowroomStocks.SoftDelete(stock);
        }

        _unitOfWork.Products.SoftDelete(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
