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
/// تنفيذ خدمة إدارة التحويلات المخزنية ونقل البضائع وتحديث الأرصدة.
/// </summary>
public class StockTransferService : IStockTransferService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public StockTransferService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

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

    public async Task<ServiceResult<IReadOnlyList<StockTransferSummaryDto>>> GetAllAsync(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new StockTransferWithDetailsSpec(fromWarehouseId, toWarehouseId, status, fromDate, toDate, search);
        var transfers = await _unitOfWork.StockTransfers.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<StockTransferSummaryDto>>(transfers);

        return ServiceResult<IReadOnlyList<StockTransferSummaryDto>>.Success(dtos);
    }

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
        var spec = new StockTransferWithDetailsSpec(fromWarehouseId, toWarehouseId, status, fromDate, toDate, search);
        var (items, totalCount) = await _unitOfWork.StockTransfers.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<StockTransferSummaryDto>>(items);
        var pagedResult = PagedResult<StockTransferSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<StockTransferSummaryDto>>.Success(pagedResult);
    }

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

    public async Task<ServiceResult<StockTransferResponseDto>> UpdateStatusAsync(Guid id, StockTransferStatus status, CancellationToken ct = default)
    {
        var transfer = await _unitOfWork.StockTransfers.FirstOrDefaultAsync(new StockTransferWithDetailsSpec(id), ct);
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

            // التحقق من كفاية المخزون في المستودع المصدر
            foreach (var item in transfer.Items)
            {
                if (fromWh.Type == WarehouseType.Storge && item.ProductBarCodeId.HasValue)
                {
                    var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                        s => s.WarehouseId == fromWh.Id && s.ProductBarcodeId == item.ProductBarCodeId.Value, ct);
                    if (stock is null || stock.Quantity < item.Quantity)
                    {
                        return ServiceResult<StockTransferResponseDto>.Failure($"الكمية غير متوفرة في المخزن المصدر", ErrorCodes.InsufficientStock);
                    }
                }
                else if (fromWh.Type == WarehouseType.Show)
                {
                    var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                        s => s.WarehouseId == fromWh.Id && s.ProductId == item.ProductId, ct);
                    if (stock is null || stock.Quantity < item.Quantity)
                    {
                        return ServiceResult<StockTransferResponseDto>.Failure($"الكمية غير متوفرة في صالة العرض المصدر", ErrorCodes.InsufficientStock);
                    }
                }
            }

            // تنفيذ الخصم والإضافة
            foreach (var item in transfer.Items)
            {
                // 1. الخصم من المصدر
                if (fromWh.Type == WarehouseType.Storge && item.ProductBarCodeId.HasValue)
                {
                    var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                        s => s.WarehouseId == fromWh.Id && s.ProductBarcodeId == item.ProductBarCodeId.Value, ct);
                    if (stock != null)
                    {
                        stock.Quantity -= item.Quantity;
                        _unitOfWork.StorgeStocks.Update(stock);
                    }
                }
                else if (fromWh.Type == WarehouseType.Show)
                {
                    var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                        s => s.WarehouseId == fromWh.Id && s.ProductId == item.ProductId, ct);
                    if (stock != null)
                    {
                        stock.Quantity -= item.Quantity;
                        _unitOfWork.ShowroomStocks.Update(stock);
                    }
                }

                // 2. الإضافة إلى الوجهة
                if (toWh.Type == WarehouseType.Storge && item.ProductBarCodeId.HasValue)
                {
                    var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                        s => s.WarehouseId == toWh.Id && s.ProductBarcodeId == item.ProductBarCodeId.Value, ct);
                    if (stock != null)
                    {
                        stock.Quantity += item.Quantity;
                        _unitOfWork.StorgeStocks.Update(stock);
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
                    }
                }
                else if (toWh.Type == WarehouseType.Show)
                {
                    var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                        s => s.WarehouseId == toWh.Id && s.ProductId == item.ProductId, ct);
                    if (stock != null)
                    {
                        stock.Quantity += item.Quantity;
                        _unitOfWork.ShowroomStocks.Update(stock);
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
                    }
                }
            }
        }

        transfer.Status = status;
        _unitOfWork.StockTransfers.Update(transfer);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.StockTransfers.FirstOrDefaultAsync(new StockTransferWithDetailsSpec(id), ct) ?? transfer;
        var responseDto = _mapper.Map<StockTransferResponseDto>(updated);

        return ServiceResult<StockTransferResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var transfer = await _unitOfWork.StockTransfers.FirstOrDefaultAsync(new StockTransferWithDetailsSpec(id), ct);
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
