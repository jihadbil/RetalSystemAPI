using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
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
/// تنفيذ خدمة إدارة أوامر الشراء وحساب إجماليات وتفاصيل الأصناف وتغيير الحالات.
/// </summary>
public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة أوامر الشراء مع حقن وحدة العمل والمحول.
    /// </summary>
    public PurchaseOrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default)
    {
        var spec = new PurchaseOrderListSpec(branchId, warehouseId, status);
        var orders = await _unitOfWork.PurchaseOrders.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<PurchaseOrderSummaryDto>>(orders);

        return ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, string? search = null, CancellationToken ct = default)
    {
        var spec = new PurchaseOrderListSpec(branchId, warehouseId, status, search);
        var (items, totalCount) = await _unitOfWork.PurchaseOrders.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<PurchaseOrderSummaryDto>>(items);
        var pagedResult = PagedResult<PurchaseOrderSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<PurchaseOrderSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        if (dto.SupplierId.HasValue && dto.SupplierId.Value != Guid.Empty)
        {
            bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId.Value, ct);
            if (!supplierExists)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
            }
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
        order.SupplierId = (dto.SupplierId.HasValue && dto.SupplierId.Value != Guid.Empty) ? dto.SupplierId.Value : null;
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

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseOrderResponseDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        if (order is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        if (order.Status == PurchaseOrderStatus.Received)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("لا يمكن تعديل طلبية تم استلامها وإغلاقها بالفعل", ErrorCodes.ValidationError);
        }

        if (dto.SupplierId.HasValue)
        {
            if (dto.SupplierId.Value != Guid.Empty)
            {
                bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId.Value, ct);
                if (!supplierExists)
                {
                    return ServiceResult<PurchaseOrderResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
                }
            }
            order.SupplierId = dto.SupplierId.Value == Guid.Empty ? null : dto.SupplierId;
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

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseOrderResponseDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default)
    {
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        if (order is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // الحماية الصارمة من تكرار الاستلام أو التلاعب بالطلبيات المغلقة
        if (order.Status == PurchaseOrderStatus.Received)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الطلبية مستلمة ومغلقة نهائياً، لا يمكن تعديل حالتها مجدداً منعاً لتكرار المخزون", ErrorCodes.ValidationError);
        }

        if (order.Status == PurchaseOrderStatus.Cancelled)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الطلبية ملغاة ولا يمكن إعادة تفعيلها", ErrorCodes.ValidationError);
        }

        bool isNewlyReceived = (status == PurchaseOrderStatus.Received);

        if (isNewlyReceived)
        {
            if (!order.SupplierId.HasValue || order.SupplierId.Value == Guid.Empty)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("يجب تحديد المورد أولاً قبل إتمام استلام الطلبية وتوليد فاتورتها", ErrorCodes.ValidationError);
            }

            if (!order.WarehouseId.HasValue || order.WarehouseId.Value == Guid.Empty)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("يجب تحديد المستودع أو الصالة المستلمة أولاً قبل إتمام استلام الطلبية", ErrorCodes.ValidationError);
            }
        }

        var orderToUpdate = await _unitOfWork.PurchaseOrders.GetByIdAsync(id, ct);
        if (orderToUpdate is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        orderToUpdate.Status = status;
        _unitOfWork.PurchaseOrders.Update(orderToUpdate);

        // خريطة استنباط ProductId من الباركود دفعة واحدة — تخدم تحديث المخزون وتوليد الفاتورة معاً
        var receivedProductIds = new Dictionary<Guid, Guid>();

        if (isNewlyReceived && order.WarehouseId.HasValue && order.Items != null && order.Items.Count > 0)
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(order.WarehouseId.Value, ct);
            if (warehouse != null)
            {
                if (warehouse.Type == WarehouseType.Storge)
                {
                    // جلب كل الباركودات المعنية (مع استنباط ProductId منها دفعة واحدة) ثم أرصدة المخزن دفعة واحدة
                    var validItems = order.Items.Where(i => i.ProductBarCodeId != Guid.Empty).ToList();
                    var barcodeIds = validItems.Select(i => i.ProductBarCodeId).Distinct().ToList();

                    var barcodesById = barcodeIds.Count > 0
                        ? (await _unitOfWork.ProductBarCodes.FindAsync(b => barcodeIds.Contains(b.Id), ct))
                            .ToDictionary(b => b.Id)
                        : new Dictionary<Guid, ProductBarCode>();

                    var stocksByBarcode = barcodeIds.Count > 0
                        ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                            s => s.WarehouseId == order.WarehouseId.Value && barcodeIds.Contains(s.ProductBarcodeId), ct))
                            .ToDictionary(s => s.ProductBarcodeId)
                        : new Dictionary<Guid, StorgeStock>();

                    foreach (var item in validItems)
                    {
                        if (stocksByBarcode.TryGetValue(item.ProductBarCodeId, out var stock))
                        {
                            stock.Quantity += (int)item.Quantity;
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
                            stocksByBarcode[item.ProductBarCodeId] = newStock;
                        }
                    }

                    // استنباط ProductId للبنود التي تحتاجه (للفاتورة المولدة أدناه) من نفس دفعة الباركودات
                    foreach (var item in validItems)
                    {
                        receivedProductIds.TryAdd(item.ProductBarCodeId,
                            barcodesById.TryGetValue(item.ProductBarCodeId, out var bc) ? bc.ProductId : Guid.Empty);
                    }
                }
                else if (warehouse.Type == WarehouseType.Show)
                {
                    var validItems = order.Items.Where(i => i.ProductBarCodeId != Guid.Empty).ToList();

                    // استنباط ProductId من الباركود دفعة واحدة
                    var barcodeIds = validItems.Select(i => i.ProductBarCodeId).Distinct().ToList();
                    var barcodesById = barcodeIds.Count > 0
                        ? (await _unitOfWork.ProductBarCodes.FindAsync(b => barcodeIds.Contains(b.Id), ct))
                            .ToDictionary(b => b.Id)
                        : new Dictionary<Guid, ProductBarCode>();

                    var productIds = validItems
                        .Select(i => barcodesById.TryGetValue(i.ProductBarCodeId, out var bc) ? bc.ProductId : Guid.Empty)
                        .Where(pid => pid != Guid.Empty)
                        .Distinct()
                        .ToList();

                    var stocksByProduct = productIds.Count > 0
                        ? (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                            s => s.WarehouseId == order.WarehouseId.Value && productIds.Contains(s.ProductId), ct))
                            .ToDictionary(s => s.ProductId)
                        : new Dictionary<Guid, ShowroomStock>();

                    foreach (var item in validItems)
                    {
                        if (!barcodesById.TryGetValue(item.ProductBarCodeId, out var bc) || bc.ProductId == Guid.Empty) continue;

                        if (stocksByProduct.TryGetValue(bc.ProductId, out var stock))
                        {
                            stock.Quantity += (int)item.Quantity;
                        }
                        else
                        {
                            var newStock = new ShowroomStock
                            {
                                TenantId = order.TenantId,
                                WarehouseId = order.WarehouseId.Value,
                                ProductId = bc.ProductId,
                                Quantity = (int)item.Quantity,
                                MinStockLevel = 0
                            };
                            await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                            stocksByProduct[bc.ProductId] = newStock;
                        }
                    }

                    // استنباط ProductId للفاتورة المولدة
                    foreach (var item in validItems)
                    {
                        if (barcodesById.TryGetValue(item.ProductBarCodeId, out var bc))
                        {
                            receivedProductIds.TryAdd(item.ProductBarCodeId, bc.ProductId);
                        }
                    }
                }
            }

            // توليد فاتورة مشتريات معتمدة آلياً لتوثيق استلام الطلبية
            var invoiceNumber = $"PINV-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var purchaseInvoice = new PurchaseInvoice
            {
                TenantId = order.TenantId,
                InvoiceNumber = invoiceNumber,
                InvoiceDate = DateTime.UtcNow,
                SupplierId = order.SupplierId!.Value,
                BranchId = order.BranchId,
                WarehouseId = order.WarehouseId.Value,
                Status = InvoiceStatus.Paid,
                PaymentMethod = PaymentMethod.Cash,
                PurchaseOrderId = order.Id,
                Notes = $"فاتورة مشتريات منشأة آلياً بموجب استلام طلبية الشراء رقم {order.OrderNumber}",
                Items = new List<PurchaseInvoiceItem>()
            };

            foreach (var item in order.Items)
            {
                Guid productId = Guid.Empty;
                if (receivedProductIds.TryGetValue(item.ProductBarCodeId, out var mappedPid))
                {
                    productId = mappedPid;
                }
                else if (item.ProductBarCode != null)
                {
                    productId = item.ProductBarCode.ProductId;
                }

                if (productId == Guid.Empty) continue;

                purchaseInvoice.Items.Add(new PurchaseInvoiceItem
                {
                    TenantId = order.TenantId,
                    ProductId = productId,
                    ProductBarCodeId = item.ProductBarCodeId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    DiscountAmount = 0,
                    LineTotal = item.Quantity * item.UnitPrice
                });
            }

            purchaseInvoice.SubTotal = purchaseInvoice.Items.Sum(i => i.LineTotal);
            purchaseInvoice.TotalAmount = purchaseInvoice.SubTotal;
            purchaseInvoice.PaidAmount = purchaseInvoice.TotalAmount;
            purchaseInvoice.RemainingAmount = 0;

            await _unitOfWork.PurchaseInvoices.AddAsync(purchaseInvoice, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var updatedOrder = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct) ?? order;
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(updatedOrder);

        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع لتفادي تضارب النسخ المكررة عند تكرار الصنف في البنود أثناء SoftDelete
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultTrackedAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        if (order is null)
        {
            return ServiceResult.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        _unitOfWork.PurchaseOrders.SoftDelete(order);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
