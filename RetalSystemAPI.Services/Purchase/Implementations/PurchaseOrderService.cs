using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Purchase.Interfaces;
using RetalSystemAPI.Services.Purchase.Specifications;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Purchase.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة الطلبيات والمشتريات وتقارير الشراء.
/// </summary>
public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PurchaseOrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ServiceResult<PurchaseOrderResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        if (order is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        var dto = _mapper.Map<PurchaseOrderResponseDto>(order);
        return ServiceResult<PurchaseOrderResponseDto>.Success(dto);
    }

    public async Task<ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default)
    {
        var spec = new PurchaseOrderWithDetailsSpec(branchId, warehouseId, status);
        var orders = await _unitOfWork.PurchaseOrders.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<PurchaseOrderSummaryDto>>(orders);

        return ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>.Success(dtos);
    }

    public async Task<ServiceResult<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, string? search = null, CancellationToken ct = default)
    {
        var spec = new PurchaseOrderWithDetailsSpec(branchId, warehouseId, status, search);
        var (items, totalCount) = await _unitOfWork.PurchaseOrders.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<PurchaseOrderSummaryDto>>(items);
        var pagedResult = PagedResult<PurchaseOrderSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<PurchaseOrderSummaryDto>>.Success(pagedResult);
    }

    public async Task<ServiceResult<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        if (dto.WarehouseId.HasValue)
        {
            bool warehouseExists = await _unitOfWork.Warehouses.ExistsAsync(w => w.Id == dto.WarehouseId.Value, ct);
            if (!warehouseExists)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المخزن المحدد غير موجود", ErrorCodes.WarehouseNotFound);
            }
        }

        bool orderNumExists = await _unitOfWork.PurchaseOrders.ExistsAsync(p => p.OrderNumber == dto.OrderNumber, ct);
        if (orderNumExists)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("رقم الطلبية مستخدم بالفعل", ErrorCodes.PurchaseOrderNumberExists);
        }

        var order = _mapper.Map<PurchaseOrder>(dto);
        order.OrderDate = dto.OrderDate == default ? DateTime.UtcNow : dto.OrderDate;
        order.Status = PurchaseOrderStatus.Draft;

        if (dto.Items != null && dto.Items.Any())
        {
            order.Items = dto.Items.Select(item => new PurchaseOrderItem
            {
                ProductBarCodeId = item.ProductBarCodeId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.Quantity * item.UnitPrice
            }).ToList();

            order.TotalAmount = order.Items.Sum(i => i.LineTotal);
        }
        else
        {
            order.TotalAmount = 0;
        }

        await _unitOfWork.PurchaseOrders.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var createdOrder = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(order.Id), ct) ?? order;
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(createdOrder);

        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<PurchaseOrderResponseDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        if (order is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        if (dto.WarehouseId.HasValue)
        {
            bool warehouseExists = await _unitOfWork.Warehouses.ExistsAsync(w => w.Id == dto.WarehouseId.Value, ct);
            if (!warehouseExists)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المخزن المحدد غير موجود", ErrorCodes.WarehouseNotFound);
            }
        }

        order.WarehouseId = dto.WarehouseId;
        order.ExpectedDate = dto.ExpectedDate;

        if (dto.Items != null)
        {
            if (order.Items != null && order.Items.Any())
            {
                foreach (var existingItem in order.Items.ToList())
                {
                    _unitOfWork.PurchaseOrderItems.HardDelete(existingItem);
                }
            }

            order.Items = dto.Items.Select(item => new PurchaseOrderItem
            {
                PurchaseOrderId = id,
                ProductBarCodeId = item.ProductBarCodeId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.Quantity * item.UnitPrice
            }).ToList();

            order.TotalAmount = order.Items.Sum(i => i.LineTotal);
        }

        _unitOfWork.PurchaseOrders.Update(order);
        await _unitOfWork.SaveChangesAsync(ct);

        var updatedOrder = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct) ?? order;
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(updatedOrder);

        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult<PurchaseOrderResponseDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        if (order is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        bool isNewlyReceived = (status == PurchaseOrderStatus.Received && order.Status != PurchaseOrderStatus.Received);

        var orderToUpdate = await _unitOfWork.PurchaseOrders.GetByIdAsync(id, ct);
        if (orderToUpdate is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        orderToUpdate.Status = status;
        _unitOfWork.PurchaseOrders.Update(orderToUpdate);

        if (isNewlyReceived && order.WarehouseId.HasValue && order.Items != null && order.Items.Count > 0)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(order.WarehouseId.Value, ct);
            if (warehouse != null)
            {
                if (warehouse.Type == WarehouseType.Storge)
                {
                    foreach (var item in order.Items)
                    {
                        if (item.ProductBarCodeId == Guid.Empty) continue;

                        var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
                            s => s.WarehouseId == order.WarehouseId.Value && s.ProductBarcodeId == item.ProductBarCodeId, ct);

                        if (stock != null)
                        {
                            var stockToUpdate = await _unitOfWork.StorgeStocks.GetByIdAsync(stock.Id, ct);
                            if (stockToUpdate != null)
                            {
                                stockToUpdate.Quantity += (int)item.Quantity;
                                _unitOfWork.StorgeStocks.Update(stockToUpdate);
                            }
                        }
                        else
                        {
                            var newStock = new StorgeStock
                            {
                                TenantId = order.TenantId,
                                WarehouseId = order.WarehouseId.Value,
                                ProductBarcodeId = item.ProductBarCodeId,
                                Quantity = (int)item.Quantity,
                                MinStockLevel = 0
                            };
                            await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                        }
                    }
                }
                else if (warehouse.Type == WarehouseType.Show)
                {
                    foreach (var item in order.Items)
                    {
                        if (item.ProductBarCodeId == Guid.Empty) continue;

                        Guid productId = item.ProductBarCode?.ProductId ?? Guid.Empty;
                        if (productId == Guid.Empty)
                        {
                            var bc = await _unitOfWork.ProductBarCodes.GetByIdAsync(item.ProductBarCodeId, ct);
                            if (bc != null) productId = bc.ProductId;
                        }

                        if (productId == Guid.Empty) continue;

                        var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
                            s => s.WarehouseId == order.WarehouseId.Value && s.ProductId == productId, ct);

                        if (stock != null)
                        {
                            var stockToUpdate = await _unitOfWork.ShowroomStocks.GetByIdAsync(stock.Id, ct);
                            if (stockToUpdate != null)
                            {
                                stockToUpdate.Quantity += (int)item.Quantity;
                                _unitOfWork.ShowroomStocks.Update(stockToUpdate);
                            }
                        }
                        else
                        {
                            var newStock = new ShowroomStock
                            {
                                TenantId = order.TenantId,
                                WarehouseId = order.WarehouseId.Value,
                                ProductId = productId,
                                Quantity = (int)item.Quantity,
                                MinStockLevel = 0
                            };
                            await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                        }
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var updatedOrder = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct) ?? order;
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(updatedOrder);

        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        if (order is null)
        {
            return ServiceResult.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        _unitOfWork.PurchaseOrders.SoftDelete(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
