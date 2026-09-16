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
/// تنفيذ خدمة إدارة التسويات الجردية وضبط كميات المخزون الفعلية ومعالجة الفوارق المحاسبية.
/// </summary>
public class StockAdjustmentService : IStockAdjustmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة التسويات الجردية مع حقن وحدة العمل والمحول.
    /// </summary>
    public StockAdjustmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
            UnitCost = item.UnitCost,
            // سبب البند إن حُدد، وإلا سبب التسوية العام
            Reason = item.Reason ?? adjustment.Reason
        }).ToList();

        // تحديث أرصدة المخزون بناءً على الكميات الفعلية للجرد — دفعة أرصدة واحدة لكل نوع مستودع
        if (warehouse.Type == WarehouseType.Show)
        {
            var productIds = adjustment.Items.Select(i => i.ProductId).Distinct().ToList();
            var stocksByProduct = productIds.Count > 0
                ? (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                    .ToDictionary(s => s.ProductId)
                : new Dictionary<Guid, ShowroomStock>();

            foreach (var item in adjustment.Items)
            {
                if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                {
                    stock.Quantity = item.ActualQuantity;
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
                    stocksByProduct[item.ProductId] = newStock;
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Storge)
        {
            var barcodeIds = adjustment.Items
                .Where(i => i.ProductBarCodeId.HasValue)
                .Select(i => i.ProductBarCodeId!.Value)
                .Distinct()
                .ToList();

            var stocksByBarcode = barcodeIds.Count > 0
                ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                    .ToDictionary(s => s.ProductBarcodeId)
                : new Dictionary<Guid, StorgeStock>();

            foreach (var item in adjustment.Items)
            {
                if (!item.ProductBarCodeId.HasValue) continue;

                if (stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                {
                    stock.Quantity = item.ActualQuantity;
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
                    stocksByBarcode[item.ProductBarCodeId.Value] = newStock;
                }
            }
        }

        await _unitOfWork.StockAdjustments.AddAsync(adjustment, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.StockAdjustments.FirstOrDefaultAsync(new StockAdjustmentWithDetailsSpec(adjustment.Id), ct) ?? adjustment;
        var responseDto = _mapper.Map<StockAdjustmentResponseDto>(created);

        return ServiceResult<StockAdjustmentResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع لتفادي تضارب النسخ المكررة عند تكرار الصنف في البنود أثناء SoftDelete
        var adjustment = await _unitOfWork.StockAdjustments.FirstOrDefaultTrackedAsync(new StockAdjustmentWithDetailsSpec(id), ct);
        if (adjustment is null)
        {
            return ServiceResult.Failure("التسوية الجردية غير موجودة", ErrorCodes.StockAdjustmentNotFound);
        }

        _unitOfWork.StockAdjustments.SoftDelete(adjustment);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
