using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Interfaces;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Sales.Interfaces;
using RetalSystemAPI.Services.Sales.Specifications;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Sales.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة مرتجعات المبيعات واسترجاع البضائع للمخازن والصالات آلياً.
/// </summary>
public class SalesReturnService : ISalesReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ICurrentPermissionService _permissionHelper;

    /// <summary>
    /// تهيئة خدمة مرتجعات المبيعات مع حقن وحدة العمل والمحول ومدقق الصلاحيات.
    /// </summary>
    public SalesReturnService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentPermissionService permissionHelper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _permissionHelper = permissionHelper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(id), ct);
        if (salesReturn is null)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        var dto = _mapper.Map<SalesReturnResponseDto>(salesReturn);
        return ServiceResult<SalesReturnResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(returnNumber), ct);
        if (salesReturn is null)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        var dto = _mapper.Map<SalesReturnResponseDto>(salesReturn);
        return ServiceResult<SalesReturnResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new SalesReturnListSpec(branchId, warehouseId, customerId, reason, fromDate, toDate, search);
        var returns = await _unitOfWork.SalesReturns.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(returns);

        return ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<SalesReturnSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new SalesReturnListSpec(branchId, warehouseId, customerId, reason, fromDate, toDate, search);
        var (items, totalCount) = await _unitOfWork.SalesReturns.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(items);
        var pagedResult = PagedResult<SalesReturnSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<SalesReturnSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesReturnResponseDto>> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default)
    {
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // المرتجع يجب أن يكون مربوطاً بفاتورة أصلية — ويرتبط حصراً بفرع ومستودع الفاتورة نفسها
        SalesInvoice? originalInvoice = null;
        if (dto.OriginalInvoiceId.HasValue)
        {
            originalInvoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(
                new SalesInvoiceWithDetailsSpec(dto.OriginalInvoiceId.Value), ct);

            if (originalInvoice is null)
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("الفاتورة الأصلية المحددة غير موجودة", ErrorCodes.SalesInvoiceNotFound);
            }

            if (originalInvoice.Status == InvoiceStatus.Cancelled || originalInvoice.Status == InvoiceStatus.Voided)
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("لا يمكن إنشاء مرتجع على فاتورة ملغاة", ErrorCodes.ValidationError);
            }

            // المرتجع يعود لنفس فرع ومستودع الفاتورة الأصلية (صالة أو مخزن البيع الفعلي)
            dto.BranchId = originalInvoice.BranchId;
            dto.WarehouseId = originalInvoice.WarehouseId;
            if (originalInvoice.CustomerId.HasValue)
            {
                dto.CustomerId = originalInvoice.CustomerId;
            }
        }
        else if (!_permissionHelper.HasPermission(Permissions.SalesReturns.ReturnWithoutInvoice))
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب اختيار الفاتورة الأصلية للمرتجع — تسجيل مرتجع بدون فاتورة يتطلب صلاحية خاصة", ErrorCodes.Forbidden);
        }

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        if (dto.CustomerId.HasValue)
        {
            bool customerExists = await _unitOfWork.Customers.ExistsAsync(c => c.Id == dto.CustomerId.Value, ct);
            if (!customerExists)
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }
        }

        bool numExists = await _unitOfWork.SalesReturns.ExistsAsync(r => r.ReturnNumber == dto.ReturnNumber, ct);
        if (numExists)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("رقم المرتجع مستخدم بالفعل", ErrorCodes.SalesReturnNumberExists);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب إضافة بند واحد على الأقل للمرتجع", ErrorCodes.ValidationError);
        }

        // عند وجود فاتورة أصلية: التحقق من أن الأصناف ضمن بنودها مع باركود البند المطابق وسقف الكمية المرتجعة
        if (originalInvoice != null)
        {
            var invoiceItemsByProduct = originalInvoice.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // الكميات المرتجعة سابقاً على نفس الفاتورة لكل صنف
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var previousReturnItems = (await _unitOfWork.SalesReturnItems.FindAsync(
                ri => ri.SalesReturn != null &&
                      ri.SalesReturn.OriginalInvoiceId == originalInvoice.Id &&
                      productIds.Contains(ri.ProductId), ct))
                .GroupBy(ri => ri.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(ri => ri.Quantity));

            foreach (var item in dto.Items)
            {
                if (!invoiceItemsByProduct.TryGetValue(item.ProductId, out var matchingInvoiceItems) || matchingInvoiceItems.Count == 0)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("الصنف المحدد غير موجود ضمن بنود الفاتورة الأصلية", ErrorCodes.ValidationError);
                }

                // ربط البند بباركود بند الفاتورة المطابق للصنف — وليس أي باركود آخر للمنتج
                if (!item.ProductBarCodeId.HasValue)
                {
                    var matchedInvoiceItem = matchingInvoiceItems.FirstOrDefault(i => i.ProductBarCodeId.HasValue)
                        ?? matchingInvoiceItems.FirstOrDefault();
                    item.ProductBarCodeId = matchedInvoiceItem?.ProductBarCodeId;
                }
                else if (matchingInvoiceItems.All(i => i.ProductBarCodeId != item.ProductBarCodeId))
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("النكهة / الباركود المحدد غير موجود ضمن بنود الفاتورة الأصلية", ErrorCodes.ValidationError);
                }

                var soldQuantity = matchingInvoiceItems.Sum(i => i.Quantity);
                previousReturnItems.TryGetValue(item.ProductId, out var previouslyReturned);

                if (item.Quantity <= 0)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("كمية المرتجع يجب أن تكون أكبر من صفر", ErrorCodes.ValidationError);
                }

                if (previouslyReturned + item.Quantity > soldQuantity)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure(
                        $"الكمية المرتجعة تتجاوز الكمية المباعة (المباعة: {soldQuantity} — المرتجع سابقاً: {previouslyReturned})",
                        ErrorCodes.SalesReturnExceedsSold);
                }
            }
        }
        else
        {
            // مرتجع بدون فاتورة (بصلاحية تجاوز خاصة) — حل الباركود الافتراضي للأصناف التي لم يحدد لها باركود
            var productIdsNeedingDefaultBarcode = dto.Items
                .Where(i => !i.ProductBarCodeId.HasValue)
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            if (productIdsNeedingDefaultBarcode.Count > 0)
            {
                var defaultBarcodes = await _unitOfWork.ProductBarCodes.FindAsync(
                    b => productIdsNeedingDefaultBarcode.Contains(b.ProductId), ct);
                var defaultBarcodeByProduct = defaultBarcodes
                    .GroupBy(b => b.ProductId)
                    .ToDictionary(g => g.Key, g => g.First());

                foreach (var item in dto.Items)
                {
                    if (!item.ProductBarCodeId.HasValue &&
                        defaultBarcodeByProduct.TryGetValue(item.ProductId, out var defaultBarcode))
                    {
                        item.ProductBarCodeId = defaultBarcode.Id;
                    }
                }
            }
        }

        var salesReturn = _mapper.Map<SalesReturn>(dto);
        salesReturn.ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow : dto.ReturnDate;

        salesReturn.Items = dto.Items.Select(item => new SalesReturnItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.Quantity * item.UnitPrice,
            Notes = item.Notes
        }).ToList();

        salesReturn.TotalAmount = salesReturn.Items.Sum(i => i.LineTotal);

        // إعادة البضاعة إلى المخزون — جلب الأرصدة المعنية دفعة واحدة ثم التعديل في الذاكرة
        if (warehouse.Type == WarehouseType.Show)
        {
            var productIds = salesReturn.Items.Select(i => i.ProductId).Distinct().ToList();
            var stocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                .ToDictionary(s => s.ProductId);

            foreach (var item in salesReturn.Items)
            {
                if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                {
                    stock.Quantity += item.Quantity;
                }
                else
                {
                    var newStock = new ShowroomStock
                    {
                        TenantId = salesReturn.TenantId,
                        WarehouseId = warehouse.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                    stocksByProduct[item.ProductId] = newStock;
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Storge)
        {
            var barcodeIds = salesReturn.Items
                .Where(i => i.ProductBarCodeId.HasValue)
                .Select(i => i.ProductBarCodeId!.Value)
                .Distinct()
                .ToList();

            var stocksByBarcode = barcodeIds.Count > 0
                ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                    .ToDictionary(s => s.ProductBarcodeId)
                : new Dictionary<Guid, StorgeStock>();

            foreach (var item in salesReturn.Items)
            {
                if (!item.ProductBarCodeId.HasValue) continue;

                if (stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                {
                    stock.Quantity += item.Quantity;
                }
                else
                {
                    var newStock = new StorgeStock
                    {
                        TenantId = salesReturn.TenantId,
                        WarehouseId = warehouse.Id,
                        ProductBarcodeId = item.ProductBarCodeId.Value,
                        Quantity = item.Quantity,
                        MinStockLevel = 0
                    };
                    await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                    stocksByBarcode[item.ProductBarCodeId.Value] = newStock;
                }
            }
        }

        await _unitOfWork.SalesReturns.AddAsync(salesReturn, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(salesReturn.Id), ct) ?? salesReturn;
        var responseDto = _mapper.Map<SalesReturnResponseDto>(created);

        return ServiceResult<SalesReturnResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultTrackedAsync(new SalesReturnWithDetailsSpec(id), ct);
        if (salesReturn is null)
        {
            return ServiceResult.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        // إلغاء تأثير المرتجع على المخزون (خصم الكميات التي كانت قد أضيفت للمخزن أو الصالة) — دفعة أرصدة واحدة
        if (salesReturn.Items != null && salesReturn.Items.Any())
        {
            var warehouse = salesReturn.Warehouse ?? await _unitOfWork.Warehouses.GetByIdAsync(salesReturn.WarehouseId, ct);
            if (warehouse != null)
            {
                if (warehouse.Type == WarehouseType.Show)
                {
                    var productIds = salesReturn.Items.Select(i => i.ProductId).Distinct().ToList();
                    var stocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                        s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                        .ToDictionary(s => s.ProductId);

                    foreach (var item in salesReturn.Items)
                    {
                        if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                        {
                            stock.Quantity -= item.Quantity;
                        }
                    }
                }
                else if (warehouse.Type == WarehouseType.Storge)
                {
                    var barcodeIds = salesReturn.Items
                        .Where(i => i.ProductBarCodeId.HasValue)
                        .Select(i => i.ProductBarCodeId!.Value)
                        .Distinct()
                        .ToList();

                    if (barcodeIds.Count > 0)
                    {
                        var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                            s => s.WarehouseId == warehouse.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                            .ToDictionary(s => s.ProductBarcodeId);

                        foreach (var item in salesReturn.Items)
                        {
                            if (item.ProductBarCodeId.HasValue &&
                                stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                            {
                                stock.Quantity -= item.Quantity;
                            }
                        }
                    }
                }
            }
        }

        _unitOfWork.SalesReturns.SoftDelete(salesReturn);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
