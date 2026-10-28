using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Sales.Interfaces;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Services.Common.Interfaces;

namespace RetalSystemAPI.Services.Sales.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة مرتجعات المبيعات واسترجاع البضائع لمخزون الصالة أو المخزن وعكس الأثر المالي والمدفوعات.
/// </summary>
public class SalesReturnService : ISalesReturnService
{
    // وحدة العمل للوصول إلى مستودعات الفواتير والمرتجعات والمخزون
    private readonly IUnitOfWork _unitOfWork;
    // محول النماذج للتحويل بين الكيانات وDTOs
    private readonly IMapper _mapper;
    // خدمة التحقق من صلاحيات المستخدم الحالي
    private readonly ICurrentPermissionService _permissionHelper;
    // خدمة الترحيل المحاسبي لحركات الخزائن والعملاء
    private readonly IFinancialPostingService _financialPosting;
    // خدمة مدفوعات نقاط البيع لعكس حركات القبض
    private readonly IPosPaymentService? _posPayments;

    /// <summary>
    /// تهيئة خدمة مرتجعات المبيعات مع حقن التبعيات اللازمة.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name="mapper">محول الكيانات</param>
    /// <param name="permissionHelper">خدمة فحص الصلاحيات</param>
    /// <param name="financialPosting">خدمة الترحيل المحاسبي</param>
    /// <param name="posPayments">خدمة مدفوعات نقاط البيع</param>
    public SalesReturnService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ICurrentPermissionService permissionHelper,
        IFinancialPostingService financialPosting,
        IPosPaymentService? posPayments = null)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول النماذج
        _mapper = mapper;
        // تعيين خدمة فحص الصلاحيات
        _permissionHelper = permissionHelper;
        // تعيين خدمة الترحيل المالي
        _financialPosting = financialPosting;
        // تعيين خدمة مدفوعات نقاط البيع
        _posPayments = posPayments;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام مرتجع المبيعات بالمعرف مع تفاصيل البنود والفرع والمستودع
        var salesReturn = await _unitOfWork.SalesReturns.GetDetailsAsync(id, ct);

        // التحقق من وجود المرتجع في قاعدة البيانات
        if (salesReturn is null)
        {
            // إرجاع خطأ عدم وجود المرتجع
            return ServiceResult<SalesReturnResponseDto>.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        // تحويل الكيان إلى كائن استجابة DTO
        var dto = _mapper.Map<SalesReturnResponseDto>(salesReturn);

        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesReturnResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        // استعلام المرتجع بواسطة رقم المرتجع المميز
        var salesReturn = await _unitOfWork.SalesReturns.GetByNumberAsync(returnNumber, ct);

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

    /// <inheritdoc/>
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
        // استرجاع قائمة ملخصات المرتجعات المطابقة لمعايير الفلترة المحددة
        var returns = await _unitOfWork.SalesReturns.ListAsync(new SalesReturnQuery
        {
            BranchId = branchId,
            WarehouseId = warehouseId,
            CustomerId = customerId,
            Reason = reason,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search
        }, ct);

        // تحويل الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(returns);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
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
        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var (items, totalCount) = await _unitOfWork.SalesReturns.SearchAsync(new SalesReturnQuery
        {
            BranchId = branchId,
            WarehouseId = warehouseId,
            CustomerId = customerId,
            Reason = reason,
            FromDate = fromDate,
            ToDate = toDate,
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, ct);

        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(items);

        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<SalesReturnSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<SalesReturnSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesReturnResponseDto>> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default)
    {
        // التحقق من صحة بيانات النموذج المدخل
        if (DtoValidation.GetError(dto) is { } validationError)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        }

        // التحقق من وجود الفرع المحدد
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // متغير لتخزين الفاتورة الأصلية في حال كان المرتجع مستنداً إلى فاتورة
        SalesInvoice? originalInvoice = null;

        // معالجة الفاتورة الأصلية والتحقق من صلاحيات الإرجاع
        if (dto.OriginalInvoiceId.HasValue)
        {
            // استعلام الفاتورة الأصلية مع تفاصيل بنودها للتعديل
            originalInvoice = await _unitOfWork.SalesInvoices.GetDetailsForUpdateAsync(dto.OriginalInvoiceId.Value, ct);

            // التحقق من وجود الفاتورة
            if (originalInvoice is null)
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("الفاتورة الأصلية المحددة غير موجودة", ErrorCodes.SalesInvoiceNotFound);
            }

            // منع الإرجاع على فاتورة ملغاة أو مسودة
            if (originalInvoice.Status is not (InvoiceStatus.Pending or InvoiceStatus.Paid or InvoiceStatus.PartiallyPaid))
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("يجب أن تكون الفاتورة الأصلية مرحلة وغير ملغاة", ErrorCodes.ValidationError);
            }

            // قصر المرتجع حصراً على نفس فرع ومستودع الفاتورة الأصلية وعميلها
            dto.BranchId = originalInvoice.BranchId;
            dto.WarehouseId = originalInvoice.WarehouseId;
            if (originalInvoice.CustomerId.HasValue)
            {
                dto.CustomerId = originalInvoice.CustomerId;
            }
        }
        else if (!_permissionHelper.HasPermission(Permissions.SalesReturns.ReturnWithoutInvoice))
        {
            // التحقق من امتلاك الصلاحية الخاصة للإرجاع المباشر دون فاتورة
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب اختيار الفاتورة الأصلية للمرتجع — تسجيل مرتجع بدون فاتورة يتطلب صلاحية خاصة", ErrorCodes.Forbidden);
        }

        // استعلام المستودع أو الصالة المستلمة للبضاعة المرتجعة
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من صحة نوع المستودع وتبعيته لفرع المرتجع ونشاطه
        if (!Enum.IsDefined(warehouse.Type) || warehouse.BranchId != dto.BranchId ||
            (originalInvoice is null && !warehouse.IsActive))
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("اختر مستودعاً صالحاً تابعاً لفرع المرتجع", ErrorCodes.ValidationError);
        }

        // متغير لتخزين كائن العميل إن تم تحديده
        Customer? customer = null;
        if (dto.CustomerId.HasValue)
        {
            // جلب العميل للتعديل
            customer = await _unitOfWork.Customers.GetByIdForUpdateAsync(dto.CustomerId.Value, ct);
            if (customer is null)
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }
        }

        // التحقق من عدم تكرار رقم المرتجع
        bool numExists = await _unitOfWork.SalesReturns.NumberExistsAsync(dto.ReturnNumber, ct);
        if (numExists)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("رقم المرتجع مستخدم بالفعل", ErrorCodes.SalesReturnNumberExists);
        }

        // التحقق من احتواء المرتجع على بند واحد على الأقل
        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب إضافة بند واحد على الأقل للمرتجع", ErrorCodes.ValidationError);
        }

        // قاموس لتتبع الكميات المرتجعة سابقاً على نفس الفاتورة
        var previousReturnItems = new Dictionary<(Guid ProductId, Guid? ProductBarCodeId), int>();

        // التحقق من سقف الكميات ومطابقة البنود عند وجود فاتورة أصلية
        if (originalInvoice != null)
        {
            // تجميع بنود الفاتورة الأصلية حسب المنتج
            var invoiceItemsByProduct = originalInvoice.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.ToList());

            // جلب إجمالي الكميات المرتجعة سابقاً على هذه الفاتورة
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            previousReturnItems = (await _unitOfWork.SalesReturns.GetReturnedItemsByInvoiceAndProductsAsync(originalInvoice.Id, productIds, ct))
                .GroupBy(ri => (ri.ProductId, ri.ProductBarCodeId))
                .ToDictionary(g => g.Key, g => g.Sum(ri => ri.Quantity));

            // فحص كل بند مطلوب إرجاعه
            foreach (var item in dto.Items)
            {
                // التحقق من وجود الصنف في بنود الفاتورة الأصلية
                if (!invoiceItemsByProduct.TryGetValue(item.ProductId, out var matchingInvoiceItems) || matchingInvoiceItems.Count == 0)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("الصنف المحدد غير موجود ضمن بنود الفاتورة الأصلية", ErrorCodes.ValidationError);
                }

                // ربط البند بباركود بند الفاتورة المطابق في حال عدم تحديده
                if (!item.ProductBarCodeId.HasValue)
                {
                    var matchedInvoiceItem = matchingInvoiceItems.FirstOrDefault(i => i.ProductBarCodeId.HasValue) ?? matchingInvoiceItems.FirstOrDefault();
                    item.ProductBarCodeId = matchedInvoiceItem?.ProductBarCodeId;
                }
                else if (matchingInvoiceItems.All(i => i.ProductBarCodeId != item.ProductBarCodeId))
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("النكهة / الباركود المحدد غير موجود ضمن بنود الفاتورة الأصلية", ErrorCodes.ValidationError);
                }

                // حساب إجمالي الكمية المباعة للصنف والباركود في الفاتورة
                var soldQuantity = matchingInvoiceItems.Where(i => i.ProductBarCodeId == item.ProductBarCodeId).Sum(i => i.Quantity);
                previousReturnItems.TryGetValue((item.ProductId, item.ProductBarCodeId), out var previouslyReturned);

                // التحقق من أن كمية الإرجاع موجبة
                if (item.Quantity <= 0)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("كمية المرتجع يجب أن تكون أكبر من صفر", ErrorCodes.ValidationError);
                }

                // التحقق من عدم تجاوز سقف الكمية المباعة بعد احتساب المرتجع سابقاً
                if ((long)previouslyReturned + item.Quantity > soldQuantity)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure($"الكمية المرتجعة تتجاوز الكمية المباعة (المباعة: {soldQuantity} — المرتجع سابقاً: {previouslyReturned})", ErrorCodes.SalesReturnExceedsSold);
                }
            }

            // التحقق من مجموع الكميات عند تكرار نفس الباركود في أكثر من سطر بالمرتجع
            foreach (var requestedGroup in dto.Items.GroupBy(i => (i.ProductId, i.ProductBarCodeId)))
            {
                var soldQuantity = originalInvoice.Items.Where(i => i.ProductId == requestedGroup.Key.ProductId && i.ProductBarCodeId == requestedGroup.Key.ProductBarCodeId).Sum(i => i.Quantity);
                previousReturnItems.TryGetValue(requestedGroup.Key, out var previouslyReturned);
                if ((long)previouslyReturned + requestedGroup.Sum(i => (long)i.Quantity) > soldQuantity)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("إجمالي البنود المكررة في المرتجع يتجاوز الكمية المباعة لنفس الباركود", ErrorCodes.SalesReturnExceedsSold);
                }
            }
        }
        else
        {
            // في حالة الإرجاع بدون فاتورة: تعيين الباركودات الافتراضية للبنود الناقصة
            var productIdsNeedingDefaultBarcode = dto.Items.Where(i => !i.ProductBarCodeId.HasValue).Select(i => i.ProductId).Distinct().ToList();
            if (productIdsNeedingDefaultBarcode.Count > 0)
            {
                var defaultBarcodes = await _unitOfWork.Products.GetBarcodesByProductIdsAsync(productIdsNeedingDefaultBarcode, ct);
                var defaultBarcodeByProduct = defaultBarcodes.GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.First());
                foreach (var item in dto.Items)
                {
                    if (!item.ProductBarCodeId.HasValue && defaultBarcodeByProduct.TryGetValue(item.ProductId, out var defaultBarcode))
                    {
                        item.ProductBarCodeId = defaultBarcode.Id;
                    }
                }
            }
        }

        // التحقق من توافق نوع المستودع مع الفرع
        if (!Enum.IsDefined(warehouse.Type) || warehouse.BranchId != dto.BranchId)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("نوع مستودع المرتجع أو تبعيته للفرع غير صحيح", ErrorCodes.ValidationError);
        }

        // إلزام الباركود للبنود في حالة الإرجاع لمخزن تخزين
        if (warehouse.Type == WarehouseType.Storge && dto.Items.Any(i => !i.ProductBarCodeId.HasValue))
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("يلزم تحديد باركود لكل بند مرتجع إلى المخزن", ErrorCodes.ValidationError);
        }

        // جلب المنتجات للتحقق من وجودها واحتساب أسعار البيع
        var returnProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var productsById = (await _unitOfWork.Products.GetByIdsAsync(returnProductIds, ct)).ToDictionary(p => p.Id);
        if (productsById.Count != returnProductIds.Count)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
        }

        // التحقق من صحة الباركودات المحددة وتبعيتها للأصناف
        var returnBarcodeIds = dto.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
        var returnBarcodes = (await _unitOfWork.Products.GetBarcodesByIdsAsync(returnBarcodeIds, ct)).ToDictionary(b => b.Id);
        if (dto.Items.Any(i => warehouse.Type == WarehouseType.Storge && !i.ProductBarCodeId.HasValue ||
            (i.ProductBarCodeId.HasValue && (!returnBarcodes.TryGetValue(i.ProductBarCodeId.Value, out var barcode) || barcode.ProductId != i.ProductId))))
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("الباركود مطلوب للمخزن ويجب أن يتبع الصنف المحدد", ErrorCodes.ValidationError);
        }

        // حساب القيمة التناسبية للبنود المرتجعة بناءً على الفاتورة الأصلية بنظام التقريب التراكمي
        var originalAmounts = new Dictionary<(Guid ProductId, Guid? ProductBarCodeId), (decimal Quantity, decimal Amount)>();
        if (originalInvoice is not null)
        {
            var groups = originalInvoice.Items.GroupBy(i => (i.ProductId, i.ProductBarCodeId))
                .OrderBy(g => g.Key.ProductId).ThenBy(g => g.Key.ProductBarCodeId).ToList();
            var weights = groups.Select(g => g.Sum(i => i.LineTotal)).ToArray();
            if (weights.Sum() == 0)
            {
                weights = groups.Select(g => (decimal)g.Sum(i => i.Quantity)).ToArray();
            }
            var amounts = ReturnValuation.Allocate(weights, originalInvoice.TotalAmount);
            for (var index = 0; index < groups.Count; index++)
            {
                originalAmounts[groups[index].Key] = (groups[index].Sum(i => i.Quantity), amounts[index]);
            }
        }

        // تحويل DTO إلى كيان المرتجع
        var salesReturn = _mapper.Map<SalesReturn>(dto);
        salesReturn.Id = Guid.NewGuid();
        salesReturn.ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow : dto.ReturnDate;
        salesReturn.Items = new List<SalesReturnItem>();

        // إنشاء بنود المرتجع وحساب أسعار وتكاليف الرد بدقة
        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
            {
                return ServiceResult<SalesReturnResponseDto>.Failure("كمية المرتجع يجب أن تكون أكبر من صفر", ErrorCodes.ValidationError);
            }

            var key = (item.ProductId, item.ProductBarCodeId);
            decimal unitPrice, lineTotal;

            if (originalInvoice is not null)
            {
                var original = originalAmounts[key];
                previousReturnItems.TryGetValue(key, out var previous);
                lineTotal = ReturnValuation.ForQuantity(original.Amount, original.Quantity, previous, item.Quantity);
                unitPrice = decimal.Round(original.Amount / original.Quantity, 4);
                previousReturnItems[key] = checked(previous + item.Quantity);
            }
            else
            {
                unitPrice = productsById[item.ProductId].SalePrice;
                lineTotal = decimal.Round(item.Quantity * unitPrice, 4);
            }

            salesReturn.Items.Add(new SalesReturnItem
            {
                ProductId = item.ProductId,
                ProductBarCodeId = item.ProductBarCodeId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                LineTotal = lineTotal,
                Notes = item.Notes
            });
        }

        // حساب إجمالي قيمة المرتجع من مجموع بنوده
        salesReturn.TotalAmount = salesReturn.Items.Sum(i => i.LineTotal);

        // تحديد المبلغ المالي المعاد نقداً للعميل
        decimal refundedAmount;
        var hasPaymentMovements = originalInvoice is not null && _posPayments is not null && await _posPayments.HasPaymentsAsync(originalInvoice.Id, ct);
        var paymentMethod = dto.PaymentMethod ?? originalInvoice?.PaymentMethod ?? PaymentMethod.Cash;

        if (dto.RefundedAmount.HasValue)
        {
            refundedAmount = dto.RefundedAmount.Value;
        }
        else if (originalInvoice is not null)
        {
            var previousReturns = await _unitOfWork.SalesReturns.GetByInvoiceAsync(originalInvoice.Id, ct);
            var availableCash = Math.Max(0, originalInvoice.PaidAmount - previousReturns.Sum(r => r.RefundedAmount));
            refundedAmount = Math.Min(salesReturn.TotalAmount, availableCash);
            if (refundedAmount == 0) paymentMethod = PaymentMethod.Credit;
        }
        else
        {
            refundedAmount = customer is null ? salesReturn.TotalAmount : 0;
            if (refundedAmount == 0) paymentMethod = PaymentMethod.Credit;
        }

        // التحقق من عدم وجود رد نقدي في التسوية الآجلة
        if (paymentMethod == PaymentMethod.Credit && refundedAmount > 0)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("التسوية الآجلة لا يمكن أن تتضمن مبلغاً نقدياً مرتجعاً", ErrorCodes.ValidationError);
        }

        // التحقق من أن المبلغ المردود لا يتجاوز إجمالي المرتجع
        if (refundedAmount < 0 || refundedAmount > salesReturn.TotalAmount)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("المبلغ المعاد لا يمكن أن يتجاوز إجمالي المرتجع", ErrorCodes.ValidationError);
        }

        // التحقق من تحديد العميل في حال قيد جزء من المرتجع رصيداً دائناً بحسابه
        var accountCredit = salesReturn.TotalAmount - refundedAmount;
        if (accountCredit > 0 && customer is null)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب تحديد العميل لقيد جزء من المرتجع في حسابه", ErrorCodes.ValidationError);
        }

        // في حال وجود حركات مدفوعات POS يتم اعتماد وسيلة الدفع الأصلية
        if (hasPaymentMovements && refundedAmount > 0) paymentMethod = originalInvoice!.PaymentMethod;

        // مطابقة خزينة الصرف لرد النقدية
        var safeResult = await _financialPosting.ResolveSafeAsync(salesReturn.BranchId, dto.RefundSafeId, hasPaymentMovements ? 0 : refundedAmount, outbound: true, ct);
        if (!safeResult.IsSuccess)
        {
            return ServiceResult<SalesReturnResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
        }

        Safe? refundSafe = safeResult.Data;
        salesReturn.RefundedAmount = refundedAmount;
        salesReturn.PaymentMethod = paymentMethod;
        salesReturn.RefundSafeId = hasPaymentMovements ? originalInvoice!.PaymentSafeId : refundSafe?.Id;

        // بدء معاملة قاعدة بيانات لحفظ المرتجع وإعادة المخزون وترحيل القيود
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // عكس مدفوعات نقطة البيع إن وجدت
            if (hasPaymentMovements)
            {
                var reversal = await _posPayments!.ReverseAsync(originalInvoice!, FinancialDocumentType.SalesReturn, salesReturn.Id, refundedAmount, ct);
                if (!reversal.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    return ServiceResult<SalesReturnResponseDto>.Failure(reversal.ErrorMessage!, reversal.ErrorCode);
                }
            }

            // تحديث الفاتورة الأصلية لضمان قفل التزامن (Optimistic Concurrency)
            if (originalInvoice is not null)
            {
                _unitOfWork.SalesInvoices.Update(originalInvoice);
            }

            // إعادة البضائع المرتجعة إلى رصيد المخزون في الصالة أو المخزن
            if (warehouse.Type == WarehouseType.Show)
            {
                var productIds = salesReturn.Items.Select(i => i.ProductId).Distinct().ToList();
                var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productIds, ct)).ToDictionary(s => s.ProductId);

                foreach (var item in salesReturn.Items)
                {
                    if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                    {
                        stock.Quantity = checked(stock.Quantity + item.Quantity);
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
                var barcodeIds = salesReturn.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
                var stocksByBarcode = barcodeIds.Count > 0
                    ? (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId)
                    : new Dictionary<Guid, StorgeStock>();

                foreach (var item in salesReturn.Items)
                {
                    if (!item.ProductBarCodeId.HasValue) continue;

                    if (stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                    {
                        stock.Quantity = checked(stock.Quantity + item.Quantity);
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

            // ترحيل قيد تخفيض مديونية العميل وقيد استرداد النقدية
            if (customer is not null)
            {
                // خفض مديونية العميل بإجمالي المرتجع
                await _financialPosting.PostCustomerBalanceAsync(
                    customer,
                    -salesReturn.TotalAmount,
                    CustomerTransactionType.SalesReturn,
                    salesReturn.BranchId,
                    FinancialDocumentType.SalesReturn,
                    salesReturn.Id,
                    salesReturn.ReturnDate,
                    paymentMethod == PaymentMethod.Multiple ? null : paymentMethod,
                    $"مرتجع مبيعات رقم {salesReturn.ReturnNumber}",
                    ct);

                // إثبات صرف الدفعة النقدية من حسابه
                await _financialPosting.PostCustomerBalanceAsync(
                    customer,
                    refundedAmount,
                    CustomerTransactionType.Reversal,
                    salesReturn.BranchId,
                    FinancialDocumentType.SalesReturn,
                    salesReturn.Id,
                    salesReturn.ReturnDate,
                    paymentMethod == PaymentMethod.Multiple ? null : paymentMethod,
                    $"رد نقدي لمرتجع مبيعات رقم {salesReturn.ReturnNumber}",
                    ct);
            }

            // خصم المبلغ المردود من رصيد الخزينة عند الرد النقدي المباشر
            if (refundSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(
                    refundSafe,
                    -refundedAmount,
                    FinancialDocumentType.SalesReturn,
                    salesReturn.Id,
                    salesReturn.ReturnDate,
                    $"رد قيمة مرتجع مبيعات رقم {salesReturn.ReturnNumber}",
                    ct);
            }

            // حفظ التغييرات واعتماد المعاملة الذرية
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return ServiceResult<SalesReturnResponseDto>.Failure("تغيرت الفاتورة أو أرصدتها أثناء الترحيل؛ أعد تحميلها قبل إعادة المحاولة", ErrorCodes.ConcurrencyError);
        }
        catch (OverflowException)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return ServiceResult<SalesReturnResponseDto>.Failure("كمية المرتجع تتجاوز سعة المخزون المسموحة", ErrorCodes.ValidationError);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // جلب تفاصيل المرتجع وإرجاع النتيجة
        var created = await _unitOfWork.SalesReturns.GetDetailsAsync(salesReturn.Id, ct) ?? salesReturn;
        var responseDto = _mapper.Map<SalesReturnResponseDto>(created);
        return ServiceResult<SalesReturnResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام سجل المرتجع ككيان متتبع للحذف
        var salesReturn = await _unitOfWork.SalesReturns.GetDetailsForUpdateAsync(id, ct);
        if (salesReturn is null)
        {
            return ServiceResult.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        // جلب الفاتورة الأصلية إن وجدت
        var invoice = salesReturn.OriginalInvoiceId.HasValue
            ? await _unitOfWork.SalesInvoices.GetDetailsForUpdateAsync(salesReturn.OriginalInvoiceId.Value, ct)
            : null;

        // جلب العميل للتعديل
        var customer = salesReturn.CustomerId.HasValue
            ? await _unitOfWork.Customers.GetByIdForUpdateAsync(salesReturn.CustomerId.Value, ct)
            : null;

        // الخزينة المعتمدة لعكس الرد النقدي
        Safe? refundSafe = null;
        var hasPaymentMovements = invoice is not null && _posPayments is not null && await _posPayments.HasPaymentsAsync(invoice.Id, ct);

        if (salesReturn.RefundedAmount > 0 && !hasPaymentMovements)
        {
            if (!salesReturn.RefundSafeId.HasValue)
            {
                return ServiceResult.Failure("لا يمكن عكس مرتجع نقدي قديم دون خزينة مرتبطة", ErrorCodes.ValidationError);
            }

            var safeResult = await _financialPosting.ResolveSafeAsync(salesReturn.BranchId, salesReturn.RefundSafeId, salesReturn.RefundedAmount, outbound: false, ct);
            if (!safeResult.IsSuccess)
            {
                return ServiceResult.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            }
            refundSafe = safeResult.Data;
        }

        // جلب المستودع المرتبط بالمرتجع
        var warehouse = salesReturn.Warehouse ?? await _unitOfWork.Warehouses.GetByIdForUpdateAsync(salesReturn.WarehouseId, ct);
        if (warehouse is null || !Enum.IsDefined(warehouse.Type))
        {
            return ServiceResult.Failure("المستودع المرتبط بالمرتجع غير متاح للعكس", ErrorCodes.WarehouseNotFound);
        }

        if (warehouse.Type == WarehouseType.Storge && salesReturn.Items.Any(i => !i.ProductBarCodeId.HasValue))
        {
            return ServiceResult.Failure("لا يمكن عكس مرتجع تخزين دون تحديد باركود لكل بند", ErrorCodes.ValidationError);
        }

        // بدء معاملة قاعدة بيانات لحذف المرتجع وعكس آثاره
        await _unitOfWork.BeginTransactionAsync(ct);

        try
        {
            // عكس حركات مدفوعات نقطة البيع
            if (hasPaymentMovements)
            {
                var reversal = await _posPayments!.UndoReturnAsync(salesReturn.Id, ct);
                if (!reversal.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                    return reversal;
                }
            }

            // إلغاء تأثير المرتجع على المخزون (خصم الكميات التي كانت قد أضيفت سابقاً)
            if (salesReturn.Items != null && salesReturn.Items.Any() && warehouse != null)
            {
                if (warehouse.Type == WarehouseType.Show)
                {
                    var productIds = salesReturn.Items.Select(i => i.ProductId).Distinct().ToList();
                    var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productIds, ct)).ToDictionary(s => s.ProductId);
                    var totals = salesReturn.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));

                    // التحقق من عدم صرف الكميات المرتجعة مسبقاً
                    if (totals.Any(t => !stocksByProduct.TryGetValue(t.Key, out var stock) || stock.Quantity < t.Value))
                    {
                        await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                        return ServiceResult.Failure("لا يمكن حذف المرتجع بعد صرف كمياته من الصالة", ErrorCodes.InsufficientStock);
                    }

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
                    var barcodeIds = salesReturn.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
                    if (barcodeIds.Count > 0)
                    {
                        var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId);
                        var totals = salesReturn.Items.GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));

                        if (totals.Any(t => !stocksByBarcode.TryGetValue(t.Key, out var stock) || stock.Quantity < t.Value))
                        {
                            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
                            return ServiceResult.Failure("لا يمكن حذف المرتجع بعد صرف كمياته من المخزن", ErrorCodes.InsufficientStock);
                        }

                        // خصم الكميات من المستودع
                        foreach (var item in salesReturn.Items)
                        {
                            if (item.ProductBarCodeId.HasValue && stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                            {
                                stock.Quantity -= item.Quantity;
                            }
                        }
                    }
                }
            }

            // تحديث وقت تعديل الفاتورة الأصلية لحماية التزامن
            if (invoice is not null)
            {
                invoice.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.SalesInvoices.Update(invoice);
            }

            // تطبيق الحذف المنطقي لسجل المرتجع
            _unitOfWork.SalesReturns.SoftDelete(salesReturn);

            // عكس قيود حساب العميل
            if (customer is not null)
            {
                // عكس الرد النقدي
                await _financialPosting.PostCustomerBalanceAsync(
                    customer,
                    -salesReturn.RefundedAmount,
                    CustomerTransactionType.Reversal,
                    salesReturn.BranchId,
                    FinancialDocumentType.SalesReturn,
                    salesReturn.Id,
                    DateTime.UtcNow,
                    salesReturn.PaymentMethod == PaymentMethod.Multiple ? null : salesReturn.PaymentMethod,
                    $"عكس الرد النقدي لمرتجع محذوف رقم {salesReturn.ReturnNumber}",
                    ct);

                // إعادة مديونية المبيعات المرتجعة
                await _financialPosting.PostCustomerBalanceAsync(
                    customer,
                    salesReturn.TotalAmount,
                    CustomerTransactionType.Reversal,
                    salesReturn.BranchId,
                    FinancialDocumentType.SalesReturn,
                    salesReturn.Id,
                    DateTime.UtcNow,
                    salesReturn.PaymentMethod == PaymentMethod.Multiple ? null : salesReturn.PaymentMethod,
                    $"عكس مرتجع مبيعات محذوف رقم {salesReturn.ReturnNumber}",
                    ct);
            }

            // إعادة المبلغ المصروف إلى الخزينة
            if (refundSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(
                    refundSafe,
                    salesReturn.RefundedAmount,
                    FinancialDocumentType.SalesReturn,
                    salesReturn.Id,
                    DateTime.UtcNow,
                    $"عكس رد مرتجع مبيعات محذوف رقم {salesReturn.ReturnNumber}",
                    ct);
            }

            // حفظ التعديلات واعتماد المعاملة
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
