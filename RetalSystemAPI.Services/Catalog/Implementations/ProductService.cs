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
/// تنفيذ خدمة إدارة المنتجات وصنف الكتالوج.
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

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

    public async Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> SearchAsync(string query, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(Array.Empty<ProductSummaryDto>());
        }

        var spec = new ProductSearchSpec(query);
        var products = await _unitOfWork.Products.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<ProductSummaryDto>>(products);

        return ServiceResult<IReadOnlyList<ProductSummaryDto>>.Success(dtos);
    }

    public async Task<ServiceResult<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default)
    {
        var barCodeEntry = await _unitOfWork.ProductBarCodes.FirstOrDefaultAsync(b => b.BarCode == barCode, ct);
        if (barCodeEntry is null)
        {
            return ServiceResult<ProductResponseDto>.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        return await GetByIdAsync(barCodeEntry.ProductId, ct);
    }

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
            product.ProductBarCodes = dto.BarCodes.Select(b => new ProductBarCode
            {
                BarCode = b.BarCode,
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

    public async Task<ServiceResult<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.FirstOrDefaultAsync(new ProductWithDetailsSpec(id), ct);
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

        _mapper.Map(dto, product);
        product.Id = id;

        if (dto.Units != null)
        {
            product.ProductUnits.Clear();
            foreach (var u in dto.Units)
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

        if (dto.BarCodes != null)
        {
            product.ProductBarCodes.Clear();
            foreach (var b in dto.BarCodes)
            {
                product.ProductBarCodes.Add(new ProductBarCode
                {
                    ProductId = id,
                    BarCode = b.BarCode,
                    Title = string.IsNullOrWhiteSpace(b.Title) ? product.Name : b.Title,
                    Description = b.Description
                });
            }
        }

        try
        {
            _unitOfWork.Products.Update(product);
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

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (product is null)
        {
            return ServiceResult.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        _unitOfWork.Products.SoftDelete(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
