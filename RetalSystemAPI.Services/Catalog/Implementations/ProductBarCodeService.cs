using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

public class ProductBarCodesByProductSpec : BaseSpecification<ProductBarCode>
{
    public ProductBarCodesByProductSpec(Guid productId) : base(b => b.ProductId == productId)
    {
        AddInclude(b => b.Product!);
        AddInclude(b => b.ProductImages);
    }
}

public class AllProductBarCodesSpec : BaseSpecification<ProductBarCode>
{
    public AllProductBarCodesSpec(string? search = null)
        : base(b => string.IsNullOrWhiteSpace(search) ||
                   b.BarCode.Contains(search) ||
                   b.Title.Contains(search) ||
                   (b.Product != null && b.Product.Name.Contains(search)))
    {
        AddInclude(b => b.Product!);
        AddInclude(b => b.ProductImages);
    }
}

/// <summary>
/// تنفيذ خدمة إدارة باركودات المنتجات.
/// </summary>
public class ProductBarCodeService : IProductBarCodeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductBarCodeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        var spec = new AllProductBarCodesSpec(search);
        var barCodes = await _unitOfWork.ProductBarCodes.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<ProductBarCodeResponseDto>>(barCodes);

        return ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>.Success(dtos);
    }

    public async Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        var spec = new ProductBarCodesByProductSpec(productId);
        var barCodes = await _unitOfWork.ProductBarCodes.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<ProductBarCodeResponseDto>>(barCodes);

        return ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>.Success(dtos);
    }

    public async Task<ServiceResult<ProductBarCodeResponseDto>> AddBarCodeAsync(
        Guid productId,
        CreateProductBarCodeDto dto,
        CancellationToken ct = default)
    {
        bool productExists = await _unitOfWork.Products.ExistsAsync(p => p.Id == productId, ct);
        if (!productExists)
        {
            return ServiceResult<ProductBarCodeResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        bool barCodeExists = await _unitOfWork.ProductBarCodes.ExistsAsync(b => b.BarCode == dto.BarCode, ct);
        if (barCodeExists)
        {
            return ServiceResult<ProductBarCodeResponseDto>.Failure("قيمة الباركود مستخدمة بالفعل لمنتج آخر", ErrorCodes.BarCodeDuplicate);
        }

        var barCodeEntity = _mapper.Map<ProductBarCode>(dto);
        barCodeEntity.ProductId = productId;

        await _unitOfWork.ProductBarCodes.AddAsync(barCodeEntity, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // توليد سجل مخزون التخزين لكل مخزن تخزين قائم لهذه النكهة الجديدة
        var storgeWarehouses = await _unitOfWork.Warehouses.FindAsync(w => w.Type == WarehouseType.Storge, ct);
        foreach (var storgeWh in storgeWarehouses)
        {
            var storgeStock = new StorgeStock
            {
                WarehouseId = storgeWh.Id,
                ProductBarcodeId = barCodeEntity.Id,
                Quantity = dto.InitialQuantity,
                MinStockLevel = 0
            };
            await _unitOfWork.StorgeStocks.AddAsync(storgeStock, ct);
        }
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<ProductBarCodeResponseDto>(barCodeEntity);
        return ServiceResult<ProductBarCodeResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<ProductBarCodeResponseDto>> UpdateBarCodeAsync(
        Guid barCodeId,
        UpdateProductBarCodeDto dto,
        CancellationToken ct = default)
    {
        var barCodeEntity = await _unitOfWork.ProductBarCodes.GetByIdAsync(barCodeId, ct);
        if (barCodeEntity is null)
        {
            return ServiceResult<ProductBarCodeResponseDto>.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        bool barCodeExists = await _unitOfWork.ProductBarCodes.ExistsAsync(b => b.BarCode == dto.BarCode && b.Id != barCodeId, ct);
        if (barCodeExists)
        {
            return ServiceResult<ProductBarCodeResponseDto>.Failure("قيمة الباركود مستخدمة بالفعل لمنتج آخر", ErrorCodes.BarCodeDuplicate);
        }

        barCodeEntity.BarCode = dto.BarCode;
        barCodeEntity.Title = dto.Title;
        barCodeEntity.Description = dto.Description;

        _unitOfWork.ProductBarCodes.Update(barCodeEntity);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<ProductBarCodeResponseDto>(barCodeEntity);
        return ServiceResult<ProductBarCodeResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> RemoveBarCodeAsync(Guid barCodeId, CancellationToken ct = default)
    {
        var barCodeEntity = await _unitOfWork.ProductBarCodes.GetByIdAsync(barCodeId, ct);
        if (barCodeEntity is null)
        {
            return ServiceResult.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        _unitOfWork.ProductBarCodes.HardDelete(barCodeEntity);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
