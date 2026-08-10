using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

public class ProductUnitsByProductSpec : BaseSpecification<ProductUnit>
{
    public ProductUnitsByProductSpec(Guid productId) : base(pu => pu.ProductId == productId)
    {
        AddInclude(pu => pu.Unit!);
    }
}

/// <summary>
/// تنفيذ خدمة وحدات المنتجات ومعاملات التحويل.
/// </summary>
public class ProductUnitService : IProductUnitService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductUnitService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<IReadOnlyList<ProductUnitResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        var spec = new ProductUnitsByProductSpec(productId);
        var productUnits = await _unitOfWork.ProductUnits.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<ProductUnitResponseDto>>(productUnits);

        return ServiceResult<IReadOnlyList<ProductUnitResponseDto>>.Success(dtos);
    }

    public async Task<ServiceResult<ProductUnitResponseDto>> AddUnitToProductAsync(
        Guid productId,
        CreateProductUnitDto dto,
        CancellationToken ct = default)
    {
        bool productExists = await _unitOfWork.Products.ExistsAsync(p => p.Id == productId, ct);
        if (!productExists)
        {
            return ServiceResult<ProductUnitResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        bool unitExists = await _unitOfWork.Units.ExistsAsync(u => u.Id == dto.UnitId, ct);
        if (!unitExists)
        {
            return ServiceResult<ProductUnitResponseDto>.Failure("الوحدة المحددة غير موجودة", ErrorCodes.UnitNotFound);
        }

        bool duplicate = await _unitOfWork.ProductUnits.ExistsAsync(pu => pu.ProductId == productId && pu.UnitId == dto.UnitId, ct);
        if (duplicate)
        {
            return ServiceResult<ProductUnitResponseDto>.Failure("هذه الوحدة مضافة بالفعل للمنتج", ErrorCodes.ProductUnitDuplicate);
        }

        if (dto.ConversionFactor <= 0)
        {
            return ServiceResult<ProductUnitResponseDto>.Failure("معامل التحويل يجب أن يكون أكبر من صفر", ErrorCodes.ValidationError);
        }

        bool hasUnits = await _unitOfWork.ProductUnits.ExistsAsync(pu => pu.ProductId == productId, ct);

        var productUnit = _mapper.Map<ProductUnit>(dto);
        productUnit.ProductId = productId;
        productUnit.IsDefault = dto.IsDefault || !hasUnits; // إذا كانت أول وحدة تجعل افتراضية تلقائياً

        await _unitOfWork.ProductUnits.AddAsync(productUnit, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var unit = await _unitOfWork.Units.GetByIdAsync(dto.UnitId, ct);
        productUnit.Unit = unit;

        var responseDto = _mapper.Map<ProductUnitResponseDto>(productUnit);
        return ServiceResult<ProductUnitResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> RemoveUnitFromProductAsync(Guid productUnitId, CancellationToken ct = default)
    {
        var productUnit = await _unitOfWork.ProductUnits.GetByIdAsync(productUnitId, ct);
        if (productUnit is null)
        {
            return ServiceResult.Failure("وحدة المنتج غير موجودة", ErrorCodes.ProductUnitNotFound);
        }

        _unitOfWork.ProductUnits.HardDelete(productUnit);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SetDefaultUnitAsync(Guid productUnitId, CancellationToken ct = default)
    {
        var targetUnit = await _unitOfWork.ProductUnits.GetByIdAsync(productUnitId, ct);
        if (targetUnit is null)
        {
            return ServiceResult.Failure("وحدة المنتج غير موجودة", ErrorCodes.ProductUnitNotFound);
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var allUnits = await _unitOfWork.ProductUnits.FindAsync(pu => pu.ProductId == targetUnit.ProductId, ct);
            foreach (var pu in allUnits)
            {
                pu.IsDefault = (pu.Id == productUnitId);
                _unitOfWork.ProductUnits.Update(pu);
            }

            await _unitOfWork.CommitTransactionAsync(ct);
            return ServiceResult.Success();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
    }
}
