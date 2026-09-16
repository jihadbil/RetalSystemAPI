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

namespace RetalSystemAPI.Services.Purchase.Implementations;

/// <summary>
/// تنفيذ خدمة فواتير المشتريات وإدارة استلام المخزون حسب تفصيل النكهات واحتساب التكاليف والضرائب.
/// </summary>
public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة فواتير المشتريات مع حقن وحدة العمل والمحول.
    /// </summary>
    public PurchaseInvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? supplierId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        var spec = new PurchaseInvoiceFilterSpec(
            supplierId, branchId, warehouseId, status, paymentMethod, fromDate, toDate, searchTerm);

        var (items, totalCount) = await _unitOfWork.PurchaseInvoices.GetPagedAsync(spec, pageNumber, pageSize, ct);
        var dtos = _mapper.Map<IReadOnlyList<PurchaseInvoiceSummaryDto>>(items);
        var pagedResult = PagedResult<PurchaseInvoiceSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        var dto = _mapper.Map<PurchaseInvoiceResponseDto>(invoice);
        return ServiceResult<PurchaseInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId, ct);
        if (!supplierExists)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
        }

        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        bool numExists = await _unitOfWork.PurchaseInvoices.ExistsAsync(p => p.InvoiceNumber == dto.InvoiceNumber, ct);
        if (numExists)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("رقم فاتورة المشتريات مستخدم بالفعل", ErrorCodes.PurchaseOrderNumberExists);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("يجب إضافة بند واحد على الأقل لفاتورة المشتريات", ErrorCodes.ValidationError);
        }

        var invoice = _mapper.Map<PurchaseInvoice>(dto);
        invoice.Supplier = null!;
        invoice.Branch = null!;
        invoice.Warehouse = null!;
        invoice.PurchaseOrder = null;

        invoice.Items = dto.Items.Select(item =>
        {
            var invItem = new PurchaseInvoiceItem
            {
                ProductId = item.ProductId,
                ProductBarCodeId = item.ProductBarCodeId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                LineTotal = Math.Max(0, (item.Quantity * item.UnitPrice) - item.DiscountAmount)
            };

            if (item.Breakdowns != null && item.Breakdowns.Any())
            {
                invItem.Breakdowns = item.Breakdowns.Select(b => new PurchaseInvoiceItemBreakdown
                {
                    ProductBarCodeId = b.ProductBarCodeId,
                    PackageQuantity = b.PackageQuantity,
                    UnitsPerPackage = b.UnitsPerPackage,
                    Quantity = b.Quantity > 0 ? b.Quantity : (b.PackageQuantity * (b.UnitsPerPackage > 0 ? b.UnitsPerPackage : 1)),
                    UnitPrice = b.UnitPrice > 0 ? b.UnitPrice : item.UnitPrice
                }).ToList();
            }

            return invItem;
        }).ToList();

        invoice.SubTotal = invoice.Items.Sum(i => i.LineTotal);
        invoice.TotalAmount = Math.Max(0, invoice.SubTotal - invoice.DiscountAmount + invoice.TaxAmount);

        if (invoice.Status == InvoiceStatus.Paid)
        {
            invoice.PaidAmount = invoice.TotalAmount;
            invoice.RemainingAmount = 0;
        }
        else
        {
            invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        }

        await _unitOfWork.PurchaseInvoices.AddAsync(invoice, ct);

        // ── زيادة رصيد المخزون فوراً وتحديث أسعار التكلفة عند إنشاء الفاتورة وإضافة البند ──
        await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: true, ct);
        await UpdateProductsCostAsync(invoice.Items, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        var createdInvoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(invoice.Id), ct) ?? invoice;
        var responseDto = _mapper.Map<PurchaseInvoiceResponseDto>(createdInvoice);

        return ServiceResult<PurchaseInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // قاعدة قفل التعديل: لا يُسمح بتعديل أي فاتورة مشتريات مغلقة أو ملغاة
        if (invoice.Status == InvoiceStatus.Paid)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة مشتريات مغلقة ومرحلة للمخازن", ErrorCodes.ValidationError);
        }

        if (invoice.Status == InvoiceStatus.Cancelled || invoice.Status == InvoiceStatus.Voided)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة مشتريات ملغاة أو باطلة", ErrorCodes.PurchaseOrderInvalidStatus);
        }

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // ── المزامنة التراكمية للبند الجديد المضاف فقط دون المساس بالبنود السابقة ──
        if (dto.Items != null)
        {
            int existingCount = invoice.Items?.Count ?? 0;
            int newTotalCount = dto.Items.Count;

            // 1. إضافة البنود الجديدة فقط (إن تم إدراج بند جديد)
            if (newTotalCount > existingCount)
            {
                var newlyAddedItems = new List<PurchaseInvoiceItem>();

                for (int i = existingCount; i < newTotalCount; i++)
                {
                    var itemDto = dto.Items[i];
                    var invItem = new PurchaseInvoiceItem
                    {
                        PurchaseInvoiceId = invoice.Id,
                        ProductId = itemDto.ProductId,
                        ProductBarCodeId = itemDto.ProductBarCodeId,
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        DiscountAmount = itemDto.DiscountAmount,
                        LineTotal = Math.Max(0, (itemDto.Quantity * itemDto.UnitPrice) - itemDto.DiscountAmount)
                    };

                    if (itemDto.Breakdowns != null && itemDto.Breakdowns.Any())
                    {
                        invItem.Breakdowns = itemDto.Breakdowns.Select(b => new PurchaseInvoiceItemBreakdown
                        {
                            ProductBarCodeId = b.ProductBarCodeId,
                            PackageQuantity = b.PackageQuantity,
                            UnitsPerPackage = b.UnitsPerPackage,
                            Quantity = b.Quantity > 0 ? b.Quantity : (b.PackageQuantity * (b.UnitsPerPackage > 0 ? b.UnitsPerPackage : 1)),
                            UnitPrice = b.UnitPrice > 0 ? b.UnitPrice : itemDto.UnitPrice
                        }).ToList();
                    }

                    newlyAddedItems.Add(invItem);
                    await _unitOfWork.PurchaseInvoiceItems.AddAsync(invItem, ct);
                }

                // زيادة المخزون وتحديث أسعار التكلفة للبند/البنود الجديدة المضافة فقط!
                await AdjustInvoiceItemsStockAsync(newlyAddedItems, dto.WarehouseId, isIncrement: true, ct);
                await UpdateProductsCostAsync(newlyAddedItems, ct);
            }
            // 2. معالجة حذف بند في حال قام المستخدم بحذف بند من الواجهة
            else if (newTotalCount < existingCount && invoice.Items != null)
            {
                var removedCount = existingCount - newTotalCount;
                var itemsToRemove = invoice.Items.Skip(newTotalCount).Take(removedCount).ToList();

                // عكس المخزون للبند المحذوف
                await AdjustInvoiceItemsStockAsync(itemsToRemove, invoice.WarehouseId, isIncrement: false, ct);

                // حذف تفصيلات البنود المحذوفة دفعة واحدة (كيانات متتبعة) بدل استعلام لكل بند وكل تفصيلة
                var removedBreakdownIds = itemsToRemove
                    .Where(i => i.Breakdowns != null)
                    .SelectMany(i => i.Breakdowns!)
                    .Select(b => b.Id)
                    .ToList();
                var removedItemIds = itemsToRemove.Select(i => i.Id).ToList();

                var trackedBreakdowns = await _unitOfWork.PurchaseInvoiceItemBreakdowns.FindTrackedAsync(
                    b => removedBreakdownIds.Contains(b.Id), ct);
                foreach (var trackedBd in trackedBreakdowns)
                {
                    _unitOfWork.PurchaseInvoiceItemBreakdowns.HardDelete(trackedBd);
                }

                var trackedItems = await _unitOfWork.PurchaseInvoiceItems.FindTrackedAsync(
                    i => removedItemIds.Contains(i.Id), ct);
                foreach (var trackedItem in trackedItems)
                {
                    _unitOfWork.PurchaseInvoiceItems.HardDelete(trackedItem);
                }
            }
        }
        else if (dto.WarehouseId != invoice.WarehouseId && invoice.Items != null && invoice.Items.Any())
        {
            // إذا تغير المستودع فقط، ننقل المخزون من المستودع القديم إلى الجديد
            await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: false, ct);
            await AdjustInvoiceItemsStockAsync(invoice.Items, dto.WarehouseId, isIncrement: true, ct);
        }

        invoice.InvoiceNumber = dto.InvoiceNumber;
        invoice.InvoiceDate = dto.InvoiceDate;
        invoice.SupplierId = dto.SupplierId;
        invoice.BranchId = dto.BranchId;
        invoice.WarehouseId = dto.WarehouseId;
        invoice.PaymentMethod = dto.PaymentMethod;
        invoice.DiscountAmount = dto.DiscountAmount;
        invoice.TaxAmount = dto.TaxAmount;
        invoice.Notes = dto.Notes;
        invoice.SubTotal = dto.Items?.Sum(i => Math.Max(0, (i.Quantity * i.UnitPrice) - i.DiscountAmount)) ?? invoice.SubTotal;
        invoice.TotalAmount = Math.Max(0, invoice.SubTotal - invoice.DiscountAmount + invoice.TaxAmount);

        // فحص ما إذا كان الطلب يتضمن إغلاق الفاتورة (التحويل إلى Paid)
        bool isClosing = dto.Status == InvoiceStatus.Paid;
        invoice.Status = dto.Status;

        if (isClosing)
        {
            invoice.PaidAmount = invoice.TotalAmount;
            invoice.RemainingAmount = 0;
        }
        else
        {
            invoice.PaidAmount = dto.PaidAmount;
            invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        }

        // تفريغ كائنات الربط المنفصلة لتجنب أي تعارض في تتبع الكيانات
        invoice.Items = null!;
        invoice.Supplier = null!;
        invoice.Branch = null!;
        invoice.Warehouse = null!;
        invoice.PurchaseOrder = null;

        _unitOfWork.PurchaseInvoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        var updatedInvoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct) ?? invoice;
        var responseDto = _mapper.Map<PurchaseInvoiceResponseDto>(updatedInvoice);

        return ServiceResult<PurchaseInvoiceResponseDto>.Success(responseDto);
    }

    /// <summary>
    /// تطبيق أو عكس تأثير بنود الفاتورة على المخزون (في المستودع أو الصالة) مع دعم تفصيلات النكهات والباركودات.
    /// تُجلب أرصدة المستودع المعنية دفعة واحدة ثم تُعدَّل في الذاكرة (معالجة نمط N+1).
    /// </summary>
    private async Task AdjustInvoiceItemsStockAsync(IEnumerable<PurchaseInvoiceItem> items, Guid warehouseId, bool isIncrement, CancellationToken ct)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId, ct);
        if (warehouse == null) return;

        var itemsList = items.ToList();
        int sign = isIncrement ? 1 : -1;

        if (warehouse.Type == WarehouseType.Storge)
        {
            // جمع كل الباركودات المعنية (تفصيلات + بنود) وحل الباركود الافتراضي دفعة واحدة
            var fallbackProductIds = itemsList
                .Where(i => (i.Breakdowns == null || !i.Breakdowns.Any()) &&
                            (!i.ProductBarCodeId.HasValue || i.ProductBarCodeId.Value == Guid.Empty))
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var defaultBarcodeByProduct = fallbackProductIds.Count > 0
                ? (await _unitOfWork.ProductBarCodes.FindAsync(
                    b => fallbackProductIds.Contains(b.ProductId), ct))
                    .GroupBy(b => b.ProductId)
                    .ToDictionary(g => g.Key, g => g.First())
                : new Dictionary<Guid, ProductBarCode>();

            var allBarcodeIds = new HashSet<Guid>();
            foreach (var item in itemsList)
            {
                if (item.Breakdowns != null && item.Breakdowns.Any())
                {
                    foreach (var bd in item.Breakdowns)
                    {
                        allBarcodeIds.Add(bd.ProductBarCodeId);
                    }
                }
                else if (item.ProductBarCodeId.HasValue && item.ProductBarCodeId.Value != Guid.Empty)
                {
                    allBarcodeIds.Add(item.ProductBarCodeId.Value);
                }
                else if (defaultBarcodeByProduct.TryGetValue(item.ProductId, out var bc))
                {
                    allBarcodeIds.Add(bc.Id);
                }
            }

            var barcodeIdList = allBarcodeIds.ToList();
            var stocksByBarcode = barcodeIdList.Count > 0
                ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && barcodeIdList.Contains(s.ProductBarcodeId), ct))
                    .ToDictionary(s => s.ProductBarcodeId)
                : new Dictionary<Guid, StorgeStock>();

            foreach (var item in itemsList)
            {
                if (item.Breakdowns != null && item.Breakdowns.Any())
                {
                    foreach (var bd in item.Breakdowns)
                    {
                        if (stocksByBarcode.TryGetValue(bd.ProductBarCodeId, out var stock))
                        {
                            stock.Quantity += sign * (int)bd.Quantity;
                        }
                        else if (isIncrement)
                        {
                            var newStock = new StorgeStock
                            {
                                WarehouseId = warehouse.Id,
                                ProductBarcodeId = bd.ProductBarCodeId,
                                Quantity = (int)bd.Quantity,
                                MinStockLevel = 0
                            };
                            await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                            stocksByBarcode[bd.ProductBarCodeId] = newStock;
                        }
                    }
                }
                else
                {
                    var barcodeId = item.ProductBarCodeId;
                    if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty)
                    {
                        if (defaultBarcodeByProduct.TryGetValue(item.ProductId, out var bc))
                        {
                            barcodeId = bc.Id;
                        }
                    }

                    if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty) continue;

                    if (stocksByBarcode.TryGetValue(barcodeId.Value, out var stock))
                    {
                        stock.Quantity += sign * (int)item.Quantity;
                    }
                    else if (isIncrement)
                    {
                        var newStock = new StorgeStock
                        {
                            WarehouseId = warehouse.Id,
                            ProductBarcodeId = barcodeId.Value,
                            Quantity = (int)item.Quantity,
                            MinStockLevel = 0
                        };
                        await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                            stocksByBarcode[barcodeId.Value] = newStock;
                    }
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Show)
        {
            var productIds = itemsList.Select(i => i.ProductId).Distinct().ToList();
            var stocksByProduct = productIds.Count > 0
                ? (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                    .ToDictionary(s => s.ProductId)
                : new Dictionary<Guid, ShowroomStock>();

            foreach (var item in itemsList)
            {
                if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                {
                    stock.Quantity += sign * (int)item.Quantity;
                }
                else if (isIncrement)
                {
                    var newStock = new ShowroomStock
                    {
                        WarehouseId = warehouse.Id,
                        ProductId = item.ProductId,
                        Quantity = (int)item.Quantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                    stocksByProduct[item.ProductId] = newStock;
                }
            }
        }
    }

    /// <summary>
    /// تحديث آخر سعر شراء (CostPrice) ومتوسط التكلفة (AveragePrice) للأصناف — جلب المنتجات المعنية دفعة واحدة.
    /// </summary>
    private async Task UpdateProductsCostAsync(IEnumerable<PurchaseInvoiceItem> items, CancellationToken ct)
    {
        var itemsList = items.ToList();
        var productIds = itemsList.Select(i => i.ProductId).Distinct().ToList();

        var productsById = productIds.Count > 0
            ? (await _unitOfWork.Products.FindTrackedAsync(p => productIds.Contains(p.Id), ct))
                .ToDictionary(p => p.Id)
            : new Dictionary<Guid, Product>();

        foreach (var item in itemsList)
        {
            if (productsById.TryGetValue(item.ProductId, out var product))
            {
                product.CostPrice = item.UnitPrice;
                if (product.AveragePrice <= 0)
                {
                    product.AveragePrice = item.UnitPrice;
                }
                else
                {
                    product.AveragePrice = Math.Round((product.AveragePrice + item.UnitPrice) / 2m, 4);
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return ServiceResult.Failure("الفاتورة ملغاة بالفعل", ErrorCodes.PurchaseOrderInvalidStatus);
        }

        // عكس المخزون عند إلغاء الفاتورة
        if (invoice.Items != null && invoice.Items.Any())
        {
            await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: false, ct);
        }

        invoice.Status = InvoiceStatus.Cancelled;
        _unitOfWork.PurchaseInvoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع لتفادي تضارب النسخ المكررة عند تكرار الصنف في البنود أثناء SoftDelete
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultTrackedAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // منع حذف فاتورة لها مرتجعات مرتبطة — يلزم حذف المرتجعات أولاً
        bool hasReturns = await _unitOfWork.PurchaseReturns.ExistsAsync(r => r.PurchaseInvoiceId == id, ct);
        if (hasReturns)
        {
            return ServiceResult.Failure("لا يمكن حذف فاتورة مشتريات لها مرتجعات مرتبطة — احذف المرتجعات أولاً", ErrorCodes.ValidationError);
        }

        if (invoice.Status != InvoiceStatus.Cancelled && invoice.Items != null && invoice.Items.Any())
        {
            await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: false, ct);
        }

        _unitOfWork.PurchaseInvoices.SoftDelete(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
