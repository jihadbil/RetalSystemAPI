using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Warehouses.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة التسويات الجردية وضبط أرصدة المخزون الفعلية.
/// </summary>
public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StockAdjustmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<StockAdjustmentResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var adjustment = await _unitOfWork.StockAdjustments.FirstOrDefaultAsync(new StockAdjustmentWithDetailsSpec(id), ct);
        if (adjustment is null)
        {
            return ServiceResult<StockAdjustmentResponseDto>.Failure("التسوية الجردية غير موجودة", ErrorCodes.StockAdjustmentNotFound);
        }

        var dto = _mapper.Map<StockAdjustmentResponseDto>(adjustment);
        return ServiceResult<StockAdjustmentResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<StockAdjustmentResponseDto>> GetByAdjustmentNumberAsync(string adjustmentNumber, CancellationToken ct = default)
    {
        var adjustment = await _unitOfWork.StockAdjustments.FirstOrDefaultAsync(new StockAdjustmentWithDetailsSpec(adjustmentNumber), ct);
        if (adjustment is null)
        {
            return ServiceResult<StockAdjustmentResponseDto>.Failure("التسوية الجردية غير موجودة", ErrorCodes.StockAdjustmentNotFound);
        }

        var dto = _mapper.Map<StockAdjustmentResponseDto>(adjustment);
        return ServiceResult<StockAdjustmentResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>> GetAllAsync(
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new StockAdjustmentWithDetailsSpec(warehouseId, reason, fromDate, toDate, search);
        var adjustments = await _unitOfWork.StockAdjustments.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<StockAdjustmentSummaryDto>>(adjustments);

        return ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PagedResult<StockAdjustmentSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new StockAdjustmentWithDetailsSpec(warehouseId, reason, fromDate, toDate, search);
        var (items, totalCount) = await _unitOfWork.StockAdjustments.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<StockAdjustmentSummaryDto>>(items);
        var pagedResult = PagedResult<StockAdjustmentSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<StockAdjustmentSummaryDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<StockAdjustmentResponseDto>> CreateAsync(CreateStockAdjustmentDto dto, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<StockAdjustmentResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        bool numExists = await _unitOfWork.StockAdjustments.ExistsAsync(a => a.AdjustmentNumber == dto.AdjustmentNumber, ct);
        if (numExists)
        {
            return ServiceResult<StockAdjustmentResponseDto>.Failure("رقم التسوية الجردية مستخدم بالفعل", ErrorCodes.StockAdjustmentNumberExists);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<StockAdjustmentResponseDto>.Failure("يجب إضافة بند واحد على الأقل للتسوية الجردية", ErrorCodes.ValidationError);
        }

        var adjustment = _mapper.Map<StockAdjustment>(dto);
        adjustment.AdjustmentDate = dto.AdjustmentDate == default ? DateTime.UtcNow : dto.AdjustmentDate;

        adjustment.Items = dto.Items.Select(item => new StockAdjustmentItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            SystemQuantity = item.SystemQuantity,
            ActualQuantity = item.ActualQuantity,
            DifferenceQuantity = item.ActualQuantity - item.SystemQuantity,
            UnitCost = item.UnitCost
        }).ToList();

        // تحديث أرصدة المخزون بناءً على الكميات الفعلية للجرد
        foreach (var item in adjustment.Items)
        {
            if (warehouse.Type == WarehouseType.Show)
            {
                var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductId == item.ProductId, ct);
                if (stock != null)
                {
                    stock.Quantity = item.ActualQuantity;
                    _unitOfWork.ShowroomStocks.Update(stock);
                }
                else
                {
                    var newStock = new ShowroomStock
                    {
                        TenantId = adjustment.TenantId,
                        WarehouseId = warehouse.Id,
                        ProductId = item.ProductId,
                        Quantity = item.ActualQuantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                }
            }
            else if (warehouse.Type == WarehouseType.Storge && item.ProductBarCodeId.HasValue)
            {
                var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                    s => s.WarehouseId == warehouse.Id && s.ProductBarcodeId == item.ProductBarCodeId.Value, ct);
                if (stock != null)
                {
                    stock.Quantity = item.ActualQuantity;
                    _unitOfWork.StorgeStocks.Update(stock);
                }
                else
                {
                    var newStock = new StorgeStock
                    {
                        TenantId = adjustment.TenantId,
                        WarehouseId = warehouse.Id,
                        ProductBarcodeId = item.ProductBarCodeId.Value,
                        Quantity = item.ActualQuantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                }
            }
        }

        await _unitOfWork.StockAdjustments.AddAsync(adjustment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.StockAdjustments.FirstOrDefaultAsync(new StockAdjustmentWithDetailsSpec(adjustment.Id), ct) ?? adjustment;
        var responseDto = _mapper.Map<StockAdjustmentResponseDto>(created);

        return ServiceResult<StockAdjustmentResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var adjustment = await _unitOfWork.StockAdjustments.FirstOrDefaultAsync(new StockAdjustmentWithDetailsSpec(id), ct);
        if (adjustment is null)
        {
            return ServiceResult.Failure("التسوية الجردية غير موجودة", ErrorCodes.StockAdjustmentNotFound);
        }

        _unitOfWork.StockAdjustments.SoftDelete(adjustment);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
