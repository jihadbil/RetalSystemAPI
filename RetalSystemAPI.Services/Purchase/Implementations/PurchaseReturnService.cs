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
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Purchase.Interfaces;
using RetalSystemAPI.Services.Purchase.Specifications;

namespace RetalSystemAPI.Services.Purchase.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة مرتجعات المشتريات وخصم البضائع المرتجعة من المخازن بدقة على مستوى النكهات والباركودات.
/// </summary>
public class PurchaseReturnService : IPurchaseReturnService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public PurchaseReturnService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var purchaseReturn = await _unitOfWork.PurchaseReturns.FirstOrDefaultAsync(new PurchaseReturnWithDetailsSpec(id), ct);
        if (purchaseReturn is null)
        {
            return ServiceResult<PurchaseReturnResponseDto>.Failure("مرتجع المشتريات غير موجود", ErrorCodes.PurchaseReturnNotFound);
        }

        var dto = _mapper.Map<PurchaseReturnResponseDto>(purchaseReturn);
        return ServiceResult<PurchaseReturnResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        var purchaseReturn = await _unitOfWork.PurchaseReturns.FirstOrDefaultAsync(new PurchaseReturnWithDetailsSpec(returnNumber), ct);
        if (purchaseReturn is null)
        {
            return ServiceResult<PurchaseReturnResponseDto>.Failure("مرتجع المشتريات غير موجود", ErrorCodes.PurchaseReturnNotFound);
        }

        var dto = _mapper.Map<PurchaseReturnResponseDto>(purchaseReturn);
        return ServiceResult<PurchaseReturnResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<PurchaseReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new PurchaseReturnListSpec(branchId, warehouseId, supplierId, reason, paymentMethod, fromDate, toDate, search);
        var returns = await _unitOfWork.PurchaseReturns.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<PurchaseReturnSummaryDto>>(returns);

        return ServiceResult<IReadOnlyList<PurchaseReturnSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<PurchaseReturnSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new PurchaseReturnListSpec(branchId, warehouseId, supplierId, reason, paymentMethod, fromDate, toDate, search);
        var (items, totalCount) = await _unitOfWork.PurchaseReturns.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<PurchaseReturnSummaryDto>>(items);
        var pagedResult = PagedResult<PurchaseReturnSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<PurchaseReturnSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseReturnResponseDto>> CreateAsync(CreatePurchaseReturnDto dto, CancellationToken ct = default)
    {
        // 1. التحقق من وجود المورد
        bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId, ct);
        if (!supplierExists)
        {
            return ServiceResult<PurchaseReturnResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // 2. التحقق من وجود الفرع
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<PurchaseReturnResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // 3. التحقق من وجود المستودع
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<PurchaseReturnResponseDto>.Failure("المستودع المحدد غير موجود", ErrorCodes.WarehouseNotFound);
        }

        // 4. قاعدة العمل الصارمة: الإرجاع للموردين يتم حصرياً من المستودعات الرئيسية (المخازن) لضمان دقة النكهات
        if (warehouse.Type != WarehouseType.Storge)
        {
            return ServiceResult<PurchaseReturnResponseDto>.Failure(
                "مرتجع المشتريات متاح فقط من المستودع الرئيسي (المخزن) لضمان دقة النكهات والباركودات. يرجى تحويل البضاعة من الصالة إلى المخزن أولاً عبر التحويلات المخزنية.",
                ErrorCodes.PurchaseReturnInvalidWarehouse);
        }

        // 5. التحقق من عدم تكرار رقم المرتجع
        bool numExists = await _unitOfWork.PurchaseReturns.ExistsAsync(r => r.ReturnNumber == dto.ReturnNumber, ct);
        if (numExists)
        {
            return ServiceResult<PurchaseReturnResponseDto>.Failure("رقم إشعار المرتجع مستخدم بالفعل", ErrorCodes.PurchaseReturnNumberExists);
        }

        // 6. الربط بالفاتورة الأصلية: يرتبط حصراً بمورد ومستودع الفاتورة وبنودها مع سقف الكمية المرتجعة
        PurchaseInvoice? originalInvoice = null;
        if (dto.PurchaseInvoiceId.HasValue)
        {
            originalInvoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(
                new PurchaseInvoiceWithDetailsSpec(dto.PurchaseInvoiceId.Value), ct);
            if (originalInvoice is null)
            {
                return ServiceResult<PurchaseReturnResponseDto>.Failure("فاتورة الشراء الأصلية المحددة غير موجودة", ErrorCodes.PurchaseOrderNotFound);
            }
            if (originalInvoice.SupplierId != dto.SupplierId)
            {
                return ServiceResult<PurchaseReturnResponseDto>.Failure("فاتورة الشراء الأصلية لا تنتمي للمورد المختار", ErrorCodes.ValidationError);
            }
            if (originalInvoice.Status == InvoiceStatus.Cancelled || originalInvoice.Status == InvoiceStatus.Voided)
            {
                return ServiceResult<PurchaseReturnResponseDto>.Failure("لا يمكن إنشاء مرتجع على فاتورة ملغاة", ErrorCodes.ValidationError);
            }

            // المرتجع يخرج حصراً من نفس فرع ومستودع الفاتورة الأصلية
            dto.BranchId = originalInvoice.BranchId;
            dto.WarehouseId = originalInvoice.WarehouseId;
        }

        // 7. التحقق من وجود بنود
        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<PurchaseReturnResponseDto>.Failure("يجب إضافة بند واحد على الأقل لمرتجع المشتريات", ErrorCodes.ValidationError);
        }

        // 8. التحقق من كفاية المخزون في المخزن لكل صنف ونكهة/باركود — دفعات موحدة
        var itemProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var productsById = (await _unitOfWork.Products.FindAsync(p => itemProductIds.Contains(p.Id), ct))
            .ToDictionary(p => p.Id);

        // جمع كل الباركودات المعنية وحل الافتراضي دفعة واحدة
        var fallbackProductIds = dto.Items
            .Where(i => !i.ProductBarCodeId.HasValue || i.ProductBarCodeId.Value == Guid.Empty)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        var defaultBarcodeByProduct = fallbackProductIds.Count > 0
            ? (await _unitOfWork.ProductBarCodes.FindAsync(
                b => fallbackProductIds.Contains(b.ProductId), ct))
                .GroupBy(b => b.ProductId)
                .ToDictionary(g => g.Key, g => g.First())
            : new Dictionary<Guid, ProductBarCode>();

        var validatedItems = new List<(CreatePurchaseReturnItemDto dtoItem, Guid barcodeId)>();
        foreach (var item in dto.Items)
        {
            if (!productsById.TryGetValue(item.ProductId, out var product))
            {
                return ServiceResult<PurchaseReturnResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
            }

            var barcodeId = item.ProductBarCodeId;
            if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty)
            {
                if (defaultBarcodeByProduct.TryGetValue(item.ProductId, out var bc))
                {
                    barcodeId = bc.Id;
                }
            }

            if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty)
            {
                return ServiceResult<PurchaseReturnResponseDto>.Failure(
                    $"الصنف '{product.Name}' لا يحتوي على باركود/نكهة مسجلة في المخزن.",
                    ErrorCodes.BarCodeNotFound);
            }

            // مع فاتورة أصلية: البند يجب أن يكون ضمن بنودها (باركود البند المطابق) وسقف الكمية المرتجعة ≤ المشتُرى
            if (originalInvoice != null)
            {
                var matchingInvoiceItems = originalInvoice.Items
                    .Where(i => i.ProductId == item.ProductId)
                    .ToList();

                if (matchingInvoiceItems.Count == 0)
                {
                    return ServiceResult<PurchaseReturnResponseDto>.Failure(
                        $"الصنف '{product.Name}' غير موجود ضمن بنود الفاتورة الأصلية.",
                        ErrorCodes.ValidationError);
                }

                var matchedInvoiceItem = matchingInvoiceItems.FirstOrDefault(i => i.ProductBarCodeId == barcodeId.Value)
                    ?? matchingInvoiceItems.FirstOrDefault(i => i.ProductBarCodeId.HasValue);

                if (matchedInvoiceItem == null || matchedInvoiceItem.ProductBarCodeId != barcodeId.Value)
                {
                    return ServiceResult<PurchaseReturnResponseDto>.Failure(
                        $"النكهة / الباركود المحدد للصنف '{product.Name}' غير موجود ضمن بنود الفاتورة الأصلية.",
                        ErrorCodes.ValidationError);
                }

                var purchasedQuantity = matchingInvoiceItems
                    .Where(i => i.ProductBarCodeId == barcodeId.Value)
                    .Sum(i => i.Quantity);

                var previouslyReturned = (await _unitOfWork.PurchaseReturnItems.FindAsync(
                    ri => ri.PurchaseReturn != null &&
                          ri.PurchaseReturn.PurchaseInvoiceId == originalInvoice.Id &&
                          ri.ProductBarCodeId == barcodeId.Value, ct))
                    .Sum(ri => ri.Quantity);

                if (previouslyReturned + item.Quantity > purchasedQuantity)
                {
                    return ServiceResult<PurchaseReturnResponseDto>.Failure(
                        $"الكمية المرتجعة للصنف '{product.Name}' تتجاوز الكمية المشتراة (المشتراة: {purchasedQuantity} — المرتجع سابقاً: {previouslyReturned}).",
                        ErrorCodes.PurchaseReturnInvalidQuantity);
                }
            }

            validatedItems.Add((item, barcodeId.Value));
        }

        // أرصدة المخزن المعنية دفعة واحدة
        var validatedBarcodeIds = validatedItems.Select(v => v.barcodeId).Distinct().ToList();
        var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
            s => s.WarehouseId == warehouse.Id && validatedBarcodeIds.Contains(s.ProductBarcodeId), ct))
            .ToDictionary(s => s.ProductBarcodeId);

        foreach (var (dtoItem, barcodeId) in validatedItems)
        {
            stocksByBarcode.TryGetValue(barcodeId, out var stock);
            int returnQty = (int)Math.Ceiling(dtoItem.Quantity);
            if (stock is null || stock.Quantity < returnQty)
            {
                int available = stock?.Quantity ?? 0;
                return ServiceResult<PurchaseReturnResponseDto>.Failure(
                    $"رصيد المخزون المتوفر في المخزن للصنف '{productsById[dtoItem.ProductId].Name}' ({available}) غير كافٍ لإتمام الإرجاع ({returnQty}).",
                    ErrorCodes.InsufficientStock);
            }
        }

        // 9. إنشاء كيان المرتجع
        var purchaseReturn = _mapper.Map<PurchaseReturn>(dto);
        purchaseReturn.ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow : dto.ReturnDate;
        purchaseReturn.Supplier = null!;
        purchaseReturn.Branch = null!;
        purchaseReturn.Warehouse = null!;
        purchaseReturn.PurchaseInvoice = null;

        purchaseReturn.Items = validatedItems.Select(v => new PurchaseReturnItem
        {
            ProductId = v.dtoItem.ProductId,
            ProductBarCodeId = v.barcodeId,
            Quantity = v.dtoItem.Quantity,
            UnitPrice = v.dtoItem.UnitPrice,
            LineTotal = Math.Max(0, v.dtoItem.Quantity * v.dtoItem.UnitPrice),
            Notes = v.dtoItem.Notes
        }).ToList();

        purchaseReturn.TotalAmount = purchaseReturn.Items.Sum(i => i.LineTotal);

        // 10. خصم البضاعة المرتجعة من مخزون المستودع الرئيسي — على نفس الأرصدة المتتبعة أعلاه
        foreach (var item in purchaseReturn.Items)
        {
            if (stocksByBarcode.TryGetValue(item.ProductBarCodeId!.Value, out var stock))
            {
                stock.Quantity -= (int)item.Quantity;
            }
        }

        await _unitOfWork.PurchaseReturns.AddAsync(purchaseReturn, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var createdReturn = await _unitOfWork.PurchaseReturns.FirstOrDefaultAsync(new PurchaseReturnWithDetailsSpec(purchaseReturn.Id), ct) ?? purchaseReturn;
        var responseDto = _mapper.Map<PurchaseReturnResponseDto>(createdReturn);

        return ServiceResult<PurchaseReturnResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع: التحميل بدون تتبع ينشئ نسخاً متعددة للكيان نفسه عند تكرار
        // الصنف في البنود، ثم يفشل Attach داخل SoftDelete بتضارب المفاتيح
        var purchaseReturn = await _unitOfWork.PurchaseReturns.FirstOrDefaultTrackedAsync(new PurchaseReturnWithDetailsSpec(id), ct);
        if (purchaseReturn is null)
        {
            return ServiceResult.Failure("مرتجع المشتريات غير موجود", ErrorCodes.PurchaseReturnNotFound);
        }

        // إعادة البضاعة المرتجعة إلى المخزن عند حذف أو إلغاء سجل المرتجع — دفعة أرصدة واحدة
        if (purchaseReturn.Items != null && purchaseReturn.Items.Any())
        {
            var barcodeIds = purchaseReturn.Items
                .Where(i => i.ProductBarCodeId.HasValue)
                .Select(i => i.ProductBarCodeId!.Value)
                .Distinct()
                .ToList();

            if (barcodeIds.Count > 0)
            {
                var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => s.WarehouseId == purchaseReturn.WarehouseId && barcodeIds.Contains(s.ProductBarcodeId), ct))
                    .ToDictionary(s => s.ProductBarcodeId);

                foreach (var item in purchaseReturn.Items)
                {
                    if (item.ProductBarCodeId.HasValue &&
                        stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                    {
                        stock.Quantity += (int)item.Quantity;
                    }
                }
            }
        }

        _unitOfWork.PurchaseReturns.SoftDelete(purchaseReturn);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
