using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Warehouses.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة التحويلات المخزنية ونقل البضائع وتحديث الأرصدة في المصدر والوجهة.
/// </summary>
public class StockTransferService : IStockTransferService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة التحويلات المخزنية مع حقن وحدة العمل والمحول.
    /// </summary>
    public StockTransferService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<StockTransferResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var transfer = await _unitOfWork.StockTransfers.FirstOrDefaultAsync(new StockTransferWithDetailsSpec(id), ct);
        if (transfer is null)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        }

        var dto = _mapper.Map<StockTransferResponseDto>(transfer);
        return ServiceResult<StockTransferResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<StockTransferResponseDto>> GetByTransferNumberAsync(string transferNumber, CancellationToken ct = default)
    {
        var transfer = await _unitOfWork.StockTransfers.FirstOrDefaultAsync(new StockTransferWithDetailsSpec(transferNumber), ct);
        if (transfer is null)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        }

        var dto = _mapper.Map<StockTransferResponseDto>(transfer);
        return ServiceResult<StockTransferResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<StockTransferSummaryDto>>> GetAllAsync(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new StockTransferListSpec(fromWarehouseId, toWarehouseId, status, fromDate, toDate, search);
        var transfers = await _unitOfWork.StockTransfers.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<StockTransferSummaryDto>>(transfers);

        return ServiceResult<IReadOnlyList<StockTransferSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<StockTransferSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new StockTransferListSpec(fromWarehouseId, toWarehouseId, status, fromDate, toDate, search);
        var (items, totalCount) = await _unitOfWork.StockTransfers.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<StockTransferSummaryDto>>(items);
        var pagedResult = PagedResult<StockTransferSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<StockTransferSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<StockTransferResponseDto>> CreateAsync(CreateStockTransferDto dto, CancellationToken ct = default)
    {
        if (dto.FromWarehouseId == dto.ToWarehouseId)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("لا يمكن التحويل لنفس المستودع أو الصالة", ErrorCodes.StockTransferSameWarehouse);
        }

        var fromWh = await _unitOfWork.Warehouses.GetByIdAsync(dto.FromWarehouseId, ct);
        if (fromWh is null)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("المستودع المصدر غير موجود", ErrorCodes.WarehouseNotFound);
        }

        var toWh = await _unitOfWork.Warehouses.GetByIdAsync(dto.ToWarehouseId, ct);
        if (toWh is null)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("المستودع الوجهة غير موجود", ErrorCodes.WarehouseNotFound);
        }

        bool numExists = await _unitOfWork.StockTransfers.ExistsAsync(t => t.TransferNumber == dto.TransferNumber, ct);
        if (numExists)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("رقم أمر التحويل مستخدم بالفعل", ErrorCodes.StockTransferNumberExists);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<StockTransferResponseDto>.Failure("يجب إضافة بند واحد على الأقل لأمر التحويل", ErrorCodes.ValidationError);
        }

        var transfer = _mapper.Map<StockTransfer>(dto);
        transfer.TransferDate = dto.TransferDate == default ? DateTime.UtcNow : dto.TransferDate;
        transfer.Status = StockTransferStatus.Draft;

        transfer.Items = dto.Items.Select(item => new StockTransferItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = item.Quantity,
            Notes = item.Notes
        }).ToList();

        await _unitOfWork.StockTransfers.AddAsync(transfer, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.StockTransfers.FirstOrDefaultAsync(new StockTransferWithDetailsSpec(transfer.Id), ct) ?? transfer;
        var responseDto = _mapper.Map<StockTransferResponseDto>(created);

        return ServiceResult<StockTransferResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<StockTransferResponseDto>> UpdateAsync(Guid id, UpdateStockTransferDto dto, CancellationToken ct = default)
    {
        var transfer = await _unitOfWork.StockTransfers.FirstOrDefaultAsync(new StockTransferWithDetailsSpec(id), ct);
        if (transfer is null)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        }

        if (transfer.Status == StockTransferStatus.Completed)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("لا يمكن تعديل أمر تحويل تم ترحيله وتنفيذه بالفعل", ErrorCodes.StockTransferInvalidStatus);
        }

        transfer.Notes = dto.Notes;
        if (dto.Status != transfer.Status)
        {
            return await UpdateStatusAsync(id, dto.Status, ct);
        }

        _unitOfWork.StockTransfers.Update(transfer);
        await _unitOfWork.SaveChangesAsync(ct);

        var responseDto = _mapper.Map<StockTransferResponseDto>(transfer);
        return ServiceResult<StockTransferResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<StockTransferResponseDto>> UpdateStatusAsync(Guid id, StockTransferStatus status, CancellationToken ct = default)
    {
        var transfer = await _unitOfWork.StockTransfers.FirstOrDefaultTrackedAsync(new StockTransferWithDetailsSpec(id), ct);
        if (transfer is null)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        }

        if (transfer.Status == StockTransferStatus.Completed)
        {
            return ServiceResult<StockTransferResponseDto>.Failure("أمر التحويل منفذ ومرحل بالفعل مسبقاً", ErrorCodes.StockTransferInvalidStatus);
        }

        if (status == StockTransferStatus.Completed)
        {
            var fromWh = await _unitOfWork.Warehouses.GetByIdAsync(transfer.FromWarehouseId, ct);
            var toWh = await _unitOfWork.Warehouses.GetByIdAsync(transfer.ToWarehouseId, ct);

            if (fromWh == null || toWh == null)
            {
                return ServiceResult<StockTransferResponseDto>.Failure("أحد المستودعات غير موجود", ErrorCodes.WarehouseNotFound);
            }

            // التحقق من كفاية المخزون في المستودع المصدر — دفعة أرصدة واحدة
            if (fromWh.Type == WarehouseType.Storge)
            {
                var barcodeIds = transfer.Items
                    .Where(i => i.ProductBarCodeId.HasValue)
                    .Select(i => i.ProductBarCodeId!.Value)
                    .Distinct()
                    .ToList();

                if (barcodeIds.Count > 0)
                {
                    var barcodeTotals = transfer.Items
                        .Where(i => i.ProductBarCodeId.HasValue)
                        .GroupBy(i => i.ProductBarCodeId!.Value)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                    var sourceStocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                        s => s.WarehouseId == fromWh.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                        .ToDictionary(s => s.ProductBarcodeId);

                    foreach (var (productBarcodeId, totalQuantity) in barcodeTotals)
                    {
                        if (!sourceStocksByBarcode.TryGetValue(productBarcodeId, out var stock) || stock.Quantity < totalQuantity)
                        {
                            return ServiceResult<StockTransferResponseDto>.Failure("الكمية المطلوبة غير متوفرة في المخزن المصدر للنكهة/الباركود المحدد", ErrorCodes.InsufficientStock);
                        }
                    }
                }
            }
            else if (fromWh.Type == WarehouseType.Show)
            {
                var productIds = transfer.Items
                    .Select(i => i.ProductId)
                    .Distinct()
                    .ToList();

                if (productIds.Count > 0)
                {
                    var productTotals = transfer.Items
                        .GroupBy(i => i.ProductId)
                        .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

                    var sourceStocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                        s => s.WarehouseId == fromWh.Id && productIds.Contains(s.ProductId), ct))
                        .ToDictionary(s => s.ProductId);

                    foreach (var (productId, totalQuantity) in productTotals)
                    {
                        if (!sourceStocksByProduct.TryGetValue(productId, out var stock) || stock.Quantity < totalQuantity)
                        {
                            return ServiceResult<StockTransferResponseDto>.Failure("الكمية المطلوبة غير متوفرة في صالة العرض المصدر للصنف المحدد", ErrorCodes.InsufficientStock);
                        }
                    }
                }
            }

            // تنفيذ الخصم من المصدر على دفعة الأرصدة المتتبعة (المصدر ثم الوجهة)
            if (fromWh.Type == WarehouseType.Storge)
            {
                var fromBarcodeIds = transfer.Items
                    .Where(i => i.ProductBarCodeId.HasValue)
                    .Select(i => i.ProductBarCodeId!.Value)
                    .Distinct()
                    .ToList();

                if (fromBarcodeIds.Count > 0)
                {
                    var sourceStocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                        s => s.WarehouseId == fromWh.Id && fromBarcodeIds.Contains(s.ProductBarcodeId), ct))
                        .ToDictionary(s => s.ProductBarcodeId);

                    foreach (var item in transfer.Items)
                    {
                        if (item.ProductBarCodeId.HasValue &&
                            sourceStocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                        {
                            stock.Quantity -= item.Quantity;
                        }
                    }
                }
            }
            else if (fromWh.Type == WarehouseType.Show)
            {
                var fromProductIds = transfer.Items.Select(i => i.ProductId).Distinct().ToList();

                if (fromProductIds.Count > 0)
                {
                    var sourceStocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                        s => s.WarehouseId == fromWh.Id && fromProductIds.Contains(s.ProductId), ct))
                        .ToDictionary(s => s.ProductId);

                    foreach (var item in transfer.Items)
                    {
                        if (sourceStocksByProduct.TryGetValue(item.ProductId, out var stock))
                        {
                            stock.Quantity -= item.Quantity;
                        }
                    }
                }
            }

            // 2. الإضافة إلى الوجهة — دفعة واحدة لكل نوع
            if (toWh.Type == WarehouseType.Storge)
            {
                var toBarcodeIds = transfer.Items
                    .Where(i => i.ProductBarCodeId.HasValue)
                    .Select(i => i.ProductBarCodeId!.Value)
                    .Distinct()
                    .ToList();

                var targetStocksByBarcode = toBarcodeIds.Count > 0
                    ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                        s => s.WarehouseId == toWh.Id && toBarcodeIds.Contains(s.ProductBarcodeId), ct))
                        .ToDictionary(s => s.ProductBarcodeId)
                    : new Dictionary<Guid, StorgeStock>();

                foreach (var item in transfer.Items)
                {
                    if (!item.ProductBarCodeId.HasValue) continue;

                    if (targetStocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                    {
                        stock.Quantity += item.Quantity;
                    }
                    else
                    {
                        var newStock = new StorgeStock
                        {
                            TenantId = transfer.TenantId,
                            WarehouseId = toWh.Id,
                            ProductBarcodeId = item.ProductBarCodeId.Value,
                            Quantity = item.Quantity,
                            MinStockLevel = 0
                        };
                        await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                        targetStocksByBarcode[item.ProductBarCodeId.Value] = newStock;
                    }
                }
            }
            else if (toWh.Type == WarehouseType.Show)
            {
                var toProductIds = transfer.Items.Select(i => i.ProductId).Distinct().ToList();

                var targetStocksByProduct = toProductIds.Count > 0
                    ? (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                        s => s.WarehouseId == toWh.Id && toProductIds.Contains(s.ProductId), ct))
                        .ToDictionary(s => s.ProductId)
                    : new Dictionary<Guid, ShowroomStock>();

                foreach (var item in transfer.Items)
                {
                    if (targetStocksByProduct.TryGetValue(item.ProductId, out var stock))
                    {
                        stock.Quantity += item.Quantity;
                    }
                    else
                    {
                        var newStock = new ShowroomStock
                        {
                            TenantId = transfer.TenantId,
                            WarehouseId = toWh.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            MinStockLevel = 0
                        };
                        await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                        targetStocksByProduct[item.ProductId] = newStock;
                    }
                }
            }
        }

        transfer.Status = status;
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.StockTransfers.FirstOrDefaultAsync(new StockTransferWithDetailsSpec(id), ct) ?? transfer;
        var responseDto = _mapper.Map<StockTransferResponseDto>(updated);

        return ServiceResult<StockTransferResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع لتفادي تضارب النسخ المكررة عند تكرار الصنف في البنود أثناء SoftDelete
        var transfer = await _unitOfWork.StockTransfers.FirstOrDefaultTrackedAsync(new StockTransferWithDetailsSpec(id), ct);
        if (transfer is null)
        {
            return ServiceResult.Failure("أمر التحويل المخزني غير موجود", ErrorCodes.StockTransferNotFound);
        }

        if (transfer.Status == StockTransferStatus.Completed)
        {
            return ServiceResult.Failure("لا يمكن حذف أمر تحويل تم ترحيله وتنفيذه", ErrorCodes.StockTransferInvalidStatus);
        }

        _unitOfWork.StockTransfers.SoftDelete(transfer);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
