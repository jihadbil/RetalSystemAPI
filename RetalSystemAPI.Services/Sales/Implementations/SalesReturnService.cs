using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Interfaces;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Sales.Interfaces;
using RetalSystemAPI.Services.Sales.Specifications;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Sales.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة مرتجعات المبيعات واسترجاع البضائع للمخازن والصالات آلياً وضبط حدود الكميات المرتجعة.
/// </summary>
public class SalesReturnService : ISalesReturnService
{
    // وحدة العمل للتعامل مع مستودعات المرتجعات والفواتير والمخزون
    private readonly IUnitOfWork _unitOfWork;

    // محول النماذج للتحويل بين الكيانات وDTOs
    private readonly IMapper _mapper;

    // مدقق الصلاحيات للتحقق من صلاحية الإرجاع بدون فاتورة أصلية
    private readonly ICurrentPermissionService _permissionHelper;
    private readonly IFinancialPostingService _financialPosting;

    /// <summary>
    /// تهيئة خدمة مرتجعات المبيعات مع حقن وحدة العمل والمحول ومدقق الصلاحيات.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name="mapper">محول الكيانات</param>
    /// <param name="permissionHelper">مدقق صلاحيات المستخدم الحالي</param>
    public SalesReturnService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentPermissionService permissionHelper,
        IFinancialPostingService financialPosting)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;

        // تعيين مرجع المحول
        _mapper = mapper;

        // تعيين مرجع مدقق الصلاحيات
        _permissionHelper = permissionHelper;
        _financialPosting = financialPosting;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام مرتجع المبيعات بالمعرف مع تفاصيل البنود والعميل والفرع والفاتورة
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(id), ct);

        // التحقق من وجود المرتجع
        if (salesReturn is null)
        {
            // إرجاع خطأ عدم وجود المرتجع
            return ServiceResult<SalesReturnResponseDto>.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<SalesReturnResponseDto>(salesReturn);

        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesReturnResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        // استعلام مرتجع المبيعات بواسطة رقم المرتجع
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(returnNumber), ct);

        // التحقق من وجود المرتجع
        if (salesReturn is null)
        {
            // إرجاع خطأ عدم وجود المرتجع
            return ServiceResult<SalesReturnResponseDto>.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<SalesReturnResponseDto>(salesReturn);

        // إرجاع النتيجة بنجاح
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
        // تجهيز مواصفة استعلام القوائم الخفيفة لمرتجعات المبيعات
        var spec = new SalesReturnListSpec(branchId, warehouseId, customerId, reason, fromDate, toDate, search);

        // جلب قائمة المرتجعات
        var returns = await _unitOfWork.SalesReturns.FindAsync(spec, ct);

        // تحويل الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(returns);

        // إرجاع النتيجة بنجاح
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
        // تجهيز مواصفة القوائم الخفيفة لمرتجعات المبيعات
        var spec = new SalesReturnListSpec(branchId, warehouseId, customerId, reason, fromDate, toDate, search);

        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var (items, totalCount) = await _unitOfWork.SalesReturns.GetPagedAsync(spec, pageNumber, pageSize, ct);

        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(items);

        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<SalesReturnSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<SalesReturnSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesReturnResponseDto>> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default)
    {
        // التحقق من وجود الفرع المحدد
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<SalesReturnResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // معالجة الفاتورة الأصلية والتحقق من صلاحيات الإرجاع
        SalesInvoice? originalInvoice = null;
        if (dto.OriginalInvoiceId.HasValue)
        {
            // استعلام الفاتورة الأصلية مع تفاصيل بنودها
            originalInvoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(
                new SalesInvoiceWithDetailsSpec(dto.OriginalInvoiceId.Value), ct);

            // التحقق من وجود الفاتورة
            if (originalInvoice is null)
            {
                // إرجاع خطأ عدم وجود الفاتورة
                return ServiceResult<SalesReturnResponseDto>.Failure("الفاتورة الأصلية المحددة غير موجودة", ErrorCodes.SalesInvoiceNotFound);
            }

            // منع الإرجاع على فاتورة ملغاة
            if (originalInvoice.Status == InvoiceStatus.Cancelled || originalInvoice.Status == InvoiceStatus.Voided)
            {
                // إرجاع خطأ الفاتورة الملغاة
                return ServiceResult<SalesReturnResponseDto>.Failure("لا يمكن إنشاء مرتجع على فاتورة ملغاة", ErrorCodes.ValidationError);
            }

            // المرتجع يعود حصراً لنفس فرع ومستودع الفاتورة الأصلية
            dto.BranchId = originalInvoice.BranchId;
            dto.WarehouseId = originalInvoice.WarehouseId;
            if (originalInvoice.CustomerId.HasValue)
            {
                dto.CustomerId = originalInvoice.CustomerId;
            }
        }
        else if (!_permissionHelper.HasPermission(Permissions.SalesReturns.ReturnWithoutInvoice))
        {
            // التحقق من امتلاك الصلاحية الخاصة للإرجاع دون فاتورة
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب اختيار الفاتورة الأصلية للمرتجع — تسجيل مرتجع بدون فاتورة يتطلب صلاحية خاصة", ErrorCodes.Forbidden);
        }

        // استعلام المستودع أو الصالة
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult<SalesReturnResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        Customer? customer = null;
        // التحقق من وجود العميل إن تم تحديده
        if (dto.CustomerId.HasValue)
        {
            customer = await _unitOfWork.Customers.GetByIdAsync(dto.CustomerId.Value, ct);
            if (customer is null)
            {
                // إرجاع خطأ عدم وجود العميل
                return ServiceResult<SalesReturnResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }
        }

        // التحقق من عدم تكرار رقم المرتجع
        bool numExists = await _unitOfWork.SalesReturns.ExistsAsync(r => r.ReturnNumber == dto.ReturnNumber, ct);
        if (numExists)
        {
            // إرجاع خطأ تكرار رقم المرتجع
            return ServiceResult<SalesReturnResponseDto>.Failure("رقم المرتجع مستخدم بالفعل", ErrorCodes.SalesReturnNumberExists);
        }

        // التحقق من وجود بنود بالمرتجع
        if (dto.Items == null || !dto.Items.Any())
        {
            // إرجاع خطأ تحقق لغياب البنود
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب إضافة بند واحد على الأقل للمرتجع", ErrorCodes.ValidationError);
        }

        // عند وجود فاتورة أصلية: التحقق من أن الأصناف ضمن بنودها وسقف الكمية المرتجعة
        if (originalInvoice != null)
        {
            // تجميع بنود الفاتورة الأصلية حسب المنتج
            var invoiceItemsByProduct = originalInvoice.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.ToList());

            // حساب الكميات المرتجعة سابقاً على نفس الفاتورة لكل صنف
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var previousReturnItems = (await _unitOfWork.SalesReturns.FindItemsAsync(
                ri => ri.SalesReturn != null &&
                      ri.SalesReturn.OriginalInvoiceId == originalInvoice.Id &&
                      productIds.Contains(ri.ProductId), ct))
                .GroupBy(ri => (ri.ProductId, ri.ProductBarCodeId))
                .ToDictionary(g => g.Key, g => g.Sum(ri => ri.Quantity));

            // فحص كل بند مراد إرجاعه
            foreach (var item in dto.Items)
            {
                // التحقق من وجود الصنف في الفاتورة الأصلية
                if (!invoiceItemsByProduct.TryGetValue(item.ProductId, out var matchingInvoiceItems) || matchingInvoiceItems.Count == 0)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("الصنف المحدد غير موجود ضمن بنود الفاتورة الأصلية", ErrorCodes.ValidationError);
                }

                // ربط البند بباركود بند الفاتورة المطابق
                if (!item.ProductBarCodeId.HasValue)
                {
                    var matchedInvoiceItem = matchingInvoiceItems.FirstOrDefault(i => i.ProductBarCodeId.HasValue)
                        ?? matchingInvoiceItems.FirstOrDefault();
                    item.ProductBarCodeId = matchedInvoiceItem?.ProductBarCodeId;
                }
                else if (matchingInvoiceItems.All(i => i.ProductBarCodeId != item.ProductBarCodeId))
                {
                    // التحقق من مطابقة الباركود المرتجع مع الفاتورة
                    return ServiceResult<SalesReturnResponseDto>.Failure("النكهة / الباركود المحدد غير موجود ضمن بنود الفاتورة الأصلية", ErrorCodes.ValidationError);
                }

                // حساب إجمالي الكمية المباعة للصنف
                var soldQuantity = matchingInvoiceItems
                    .Where(i => i.ProductBarCodeId == item.ProductBarCodeId)
                    .Sum(i => i.Quantity);
                previousReturnItems.TryGetValue((item.ProductId, item.ProductBarCodeId), out var previouslyReturned);

                // التحقق من أن كمية الإرجاع موجبة
                if (item.Quantity <= 0)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("كمية المرتجع يجب أن تكون أكبر من صفر", ErrorCodes.ValidationError);
                }

                // التحقق من عدم تجاوز سقف الكمية المباعة
                if (previouslyReturned + item.Quantity > soldQuantity)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure(
                        $"الكمية المرتجعة تتجاوز الكمية المباعة (المباعة: {soldQuantity} — المرتجع سابقاً: {previouslyReturned})",
                        ErrorCodes.SalesReturnExceedsSold);
                }
            }

            foreach (var requestedGroup in dto.Items.GroupBy(i => (i.ProductId, i.ProductBarCodeId)))
            {
                var soldQuantity = originalInvoice.Items
                    .Where(i => i.ProductId == requestedGroup.Key.ProductId &&
                                i.ProductBarCodeId == requestedGroup.Key.ProductBarCodeId)
                    .Sum(i => i.Quantity);
                previousReturnItems.TryGetValue(requestedGroup.Key, out var previouslyReturned);
                if (previouslyReturned + requestedGroup.Sum(i => i.Quantity) > soldQuantity)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure(
                        "إجمالي البنود المكررة في المرتجع يتجاوز الكمية المباعة لنفس الباركود",
                        ErrorCodes.SalesReturnExceedsSold);
                }
            }
        }
        else
        {
            // مرتجع بدون فاتورة: حل الباركودات الافتراضية
            var productIdsNeedingDefaultBarcode = dto.Items
                .Where(i => !i.ProductBarCodeId.HasValue)
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            if (productIdsNeedingDefaultBarcode.Count > 0)
            {
                var defaultBarcodes = await _unitOfWork.Products.FindBarCodesAsync(
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

        var returnProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var productsById = (await _unitOfWork.Products.FindAsync(p => returnProductIds.Contains(p.Id), ct))
            .ToDictionary(p => p.Id);
        if (productsById.Count != returnProductIds.Count)
            return ServiceResult<SalesReturnResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);

        // سعر المرتجع المرتبط بفـاتورة يؤخذ من الفاتورة الأصلية، وإلا من سعر البيع الحالي.
        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return ServiceResult<SalesReturnResponseDto>.Failure("كمية المرتجع يجب أن تكون أكبر من صفر", ErrorCodes.ValidationError);

            if (originalInvoice is not null)
            {
                var matching = originalInvoice.Items
                    .Where(i => i.ProductId == item.ProductId &&
                                (!item.ProductBarCodeId.HasValue || i.ProductBarCodeId == item.ProductBarCodeId))
                    .ToList();
                var quantity = matching.Sum(i => i.Quantity);
                item.UnitPrice = quantity > 0
                    ? matching.Sum(i => i.UnitPrice * i.Quantity) / quantity
                    : productsById[item.ProductId].SalePrice;
            }
            else
            {
                item.UnitPrice = productsById[item.ProductId].SalePrice;
            }
        }

        // تحويل كائن DTO إلى كيان مرتجع المبيعات
        var salesReturn = _mapper.Map<SalesReturn>(dto);
        salesReturn.ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow : dto.ReturnDate;

        // بناء قائمة بنود المرتجع وحساب إجماليات السطور
        salesReturn.Items = dto.Items.Select(item => new SalesReturnItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.Quantity * item.UnitPrice,
            Notes = item.Notes
        }).ToList();

        // حساب إجمالي قيمة المرتجع
        salesReturn.TotalAmount = salesReturn.Items.Sum(i => i.LineTotal);

        decimal refundedAmount;
        var paymentMethod = dto.PaymentMethod ?? originalInvoice?.PaymentMethod ?? PaymentMethod.Cash;
        if (dto.RefundedAmount.HasValue)
        {
            refundedAmount = dto.RefundedAmount.Value;
        }
        else if (originalInvoice is not null)
        {
            var previousReturns = await _unitOfWork.SalesReturns.FindAsync(
                r => r.OriginalInvoiceId == originalInvoice.Id, ct);
            var availableCash = Math.Max(0, originalInvoice.PaidAmount - previousReturns.Sum(r => r.RefundedAmount));
            refundedAmount = Math.Min(salesReturn.TotalAmount, availableCash);
            if (refundedAmount == 0) paymentMethod = PaymentMethod.Credit;
        }
        else
        {
            refundedAmount = customer is null ? salesReturn.TotalAmount : 0;
            if (refundedAmount == 0) paymentMethod = PaymentMethod.Credit;
        }

        if (paymentMethod == PaymentMethod.Credit && refundedAmount > 0)
            return ServiceResult<SalesReturnResponseDto>.Failure("التسوية الآجلة لا يمكن أن تتضمن مبلغاً نقدياً مرتجعاً", ErrorCodes.ValidationError);
        if (refundedAmount < 0 || refundedAmount > salesReturn.TotalAmount)
            return ServiceResult<SalesReturnResponseDto>.Failure("المبلغ المعاد لا يمكن أن يتجاوز إجمالي المرتجع", ErrorCodes.ValidationError);

        var accountCredit = salesReturn.TotalAmount - refundedAmount;
        if (accountCredit > 0 && customer is null)
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب تحديد العميل لقيد جزء من المرتجع في حسابه", ErrorCodes.ValidationError);

        var safeResult = await _financialPosting.ResolveSafeAsync(
            salesReturn.BranchId, dto.RefundSafeId, refundedAmount, outbound: true, ct);
        if (!safeResult.IsSuccess)
            return ServiceResult<SalesReturnResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
        Safe? refundSafe = safeResult.Data;
        salesReturn.RefundedAmount = refundedAmount;
        salesReturn.PaymentMethod = paymentMethod;
        salesReturn.RefundSafeId = refundSafe?.Id;

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {

        // إعادة البضاعة إلى رصيد المخزون (صالة أو مخزن)
        if (warehouse.Type == WarehouseType.Show)
        {
            var productIds = salesReturn.Items.Select(i => i.ProductId).Distinct().ToList();
            var stocksByProduct = (await _unitOfWork.Stock.FindShowroomStockTrackedAsync(
                s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                .ToDictionary(s => s.ProductId);

            // زيادة كمية كل صنف في رصيد الصالة
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
                    await _unitOfWork.Stock.AddShowroomStockAsync(newStock, ct);
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
                ? (await _unitOfWork.Stock.FindStorgeStockTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                    .ToDictionary(s => s.ProductBarcodeId)
                : new Dictionary<Guid, StorgeStock>();

            // زيادة كمية كل باركود في رصيد المستودع
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
                    await _unitOfWork.Stock.AddStorgeStockAsync(newStock, ct);
                    stocksByBarcode[item.ProductBarCodeId.Value] = newStock;
                }
            }
        }

        // إضافة سجل المرتجع للمستودع
        if (salesReturn.Id == Guid.Empty) salesReturn.Id = Guid.NewGuid();
        await _unitOfWork.SalesReturns.AddAsync(salesReturn, ct);

        if (customer is not null)
        {
            await _financialPosting.PostCustomerBalanceAsync(customer, -salesReturn.TotalAmount,
                CustomerTransactionType.SalesReturn, salesReturn.BranchId, FinancialDocumentType.SalesReturn,
                salesReturn.Id, salesReturn.ReturnDate, paymentMethod,
                $"مرتجع مبيعات رقم {salesReturn.ReturnNumber}", ct);
            await _financialPosting.PostCustomerBalanceAsync(customer, refundedAmount,
                CustomerTransactionType.Reversal, salesReturn.BranchId, FinancialDocumentType.SalesReturn,
                salesReturn.Id, salesReturn.ReturnDate, paymentMethod,
                $"رد نقدي لمرتجع مبيعات رقم {salesReturn.ReturnNumber}", ct);
        }

        if (refundSafe is not null)
        {
            await _financialPosting.PostSafeBalanceAsync(refundSafe, -refundedAmount,
                FinancialDocumentType.SalesReturn, salesReturn.Id, salesReturn.ReturnDate,
                $"رد قيمة مرتجع مبيعات رقم {salesReturn.ReturnNumber}", ct);
        }

        // حفظ التغييرات وحركات المخزون في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // إعادة جلب المرتجع مع التفاصيل
        var created = await _unitOfWork.SalesReturns.FirstOrDefaultAsync(new SalesReturnWithDetailsSpec(salesReturn.Id), ct) ?? salesReturn;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SalesReturnResponseDto>(created);

        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesReturnResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام سجل المرتجع ككيان متتبع
        var salesReturn = await _unitOfWork.SalesReturns.FirstOrDefaultTrackedAsync(new SalesReturnWithDetailsSpec(id), ct);

        // التحقق من وجود المرتجع
        if (salesReturn is null)
        {
            // إرجاع خطأ عدم وجود المرتجع
            return ServiceResult.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        var customer = salesReturn.CustomerId.HasValue
            ? await _unitOfWork.Customers.GetByIdAsync(salesReturn.CustomerId.Value, ct)
            : null;
        Safe? refundSafe = null;
        if (salesReturn.RefundedAmount > 0)
        {
            if (!salesReturn.RefundSafeId.HasValue)
                return ServiceResult.Failure("لا يمكن عكس مرتجع نقدي قديم دون خزينة مرتبطة", ErrorCodes.ValidationError);
            var safeResult = await _financialPosting.ResolveSafeAsync(
                salesReturn.BranchId, salesReturn.RefundSafeId, salesReturn.RefundedAmount, outbound: false, ct);
            if (!safeResult.IsSuccess)
                return ServiceResult.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            refundSafe = safeResult.Data;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {

        // إلغاء تأثير المرتجع على المخزون (خصم الكميات التي كانت قد أضيفت سابقاً)
        if (salesReturn.Items != null && salesReturn.Items.Any())
        {
            var warehouse = salesReturn.Warehouse ?? await _unitOfWork.Warehouses.GetByIdAsync(salesReturn.WarehouseId, ct);
            if (warehouse != null)
            {
                if (warehouse.Type == WarehouseType.Show)
                {
                    var productIds = salesReturn.Items.Select(i => i.ProductId).Distinct().ToList();
                    var stocksByProduct = (await _unitOfWork.Stock.FindShowroomStockTrackedAsync(
                        s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                        .ToDictionary(s => s.ProductId);

                    // خصم الكميات من الصالة
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
                        var stocksByBarcode = (await _unitOfWork.Stock.FindStorgeStockTrackedAsync(
                            s => s.WarehouseId == warehouse.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                            .ToDictionary(s => s.ProductBarcodeId);

                        // خصم الكميات من المستودع
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

        // تطبيق الحذف المنطقي لسجل المرتجع
        _unitOfWork.SalesReturns.SoftDelete(salesReturn);

        if (customer is not null)
        {
            await _financialPosting.PostCustomerBalanceAsync(customer, -salesReturn.RefundedAmount,
                CustomerTransactionType.Reversal, salesReturn.BranchId, FinancialDocumentType.SalesReturn,
                salesReturn.Id, DateTime.UtcNow, salesReturn.PaymentMethod,
                $"عكس الرد النقدي لمرتجع محذوف رقم {salesReturn.ReturnNumber}", ct);
            await _financialPosting.PostCustomerBalanceAsync(customer, salesReturn.TotalAmount,
                CustomerTransactionType.Reversal, salesReturn.BranchId, FinancialDocumentType.SalesReturn,
                salesReturn.Id, DateTime.UtcNow, salesReturn.PaymentMethod,
                $"عكس مرتجع مبيعات محذوف رقم {salesReturn.ReturnNumber}", ct);
        }

        if (refundSafe is not null)
        {
            await _financialPosting.PostSafeBalanceAsync(refundSafe, salesReturn.RefundedAmount,
                FinancialDocumentType.SalesReturn, salesReturn.Id, DateTime.UtcNow,
                $"عكس رد مرتجع مبيعات محذوف رقم {salesReturn.ReturnNumber}", ct);
        }

        // حفظ التعديلات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
