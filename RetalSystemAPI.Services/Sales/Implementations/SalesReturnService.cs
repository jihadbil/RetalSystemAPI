using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
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
    /// <param name = "unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name = "mapper">محول الكيانات</param>
    /// <param name = "permissionHelper">مدقق صلاحيات المستخدم الحالي</param>
    public SalesReturnService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentPermissionService permissionHelper, IFinancialPostingService financialPosting)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع المحول
        _mapper = mapper;
        // تعيين مرجع مدقق الصلاحيات
        _permissionHelper = permissionHelper;
        _financialPosting = financialPosting;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام مرتجع المبيعات بالمعرف مع تفاصيل البنود والعميل والفرع والفاتورة
        var salesReturn = await _unitOfWork.SalesReturns.GetDetailsAsync(id, ct);
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
    public async Task<ServiceResult<SalesReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        // استعلام مرتجع المبيعات بواسطة رقم المرتجع
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
    public async Task<ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, Guid? customerId = null, SalesReturnReason? reason = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام القوائم الخفيفة لمرتجعات المبيعات
        // جلب قائمة المرتجعات
        var returns = await _unitOfWork.SalesReturns.ListAsync(new SalesReturnQuery { BranchId = branchId, WarehouseId = warehouseId, CustomerId = customerId, Reason = reason, FromDate = fromDate, ToDate = toDate, Search = search }, ct);
        // تحويل الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<SalesReturnSummaryDto>>(returns);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<SalesReturnSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, Guid? customerId = null, SalesReturnReason? reason = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة القوائم الخفيفة لمرتجعات المبيعات
        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var(items, totalCount) = await _unitOfWork.SalesReturns.SearchAsync(new SalesReturnQuery { BranchId = branchId, WarehouseId = warehouseId, CustomerId = customerId, Reason = reason, FromDate = fromDate, ToDate = toDate, Search = search, PageNumber = pageNumber, PageSize = pageSize }, ct);
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
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SalesReturnResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من وجود الفرع المحدد
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
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
            originalInvoice = await _unitOfWork.SalesInvoices.GetDetailsAsync(dto.OriginalInvoiceId.Value, ct);
            // التحقق من وجود الفاتورة
            if (originalInvoice is null)
            {
                // إرجاع خطأ عدم وجود الفاتورة
                return ServiceResult<SalesReturnResponseDto>.Failure("الفاتورة الأصلية المحددة غير موجودة", ErrorCodes.SalesInvoiceNotFound);
            }

            // منع الإرجاع على فاتورة ملغاة
            if (originalInvoice.Status is not (InvoiceStatus.Pending or InvoiceStatus.Paid or InvoiceStatus.PartiallyPaid))
            {
                // إرجاع خطأ الفاتورة الملغاة
                return ServiceResult<SalesReturnResponseDto>.Failure("يجب أن تكون الفاتورة الأصلية مرحلة وغير ملغاة", ErrorCodes.ValidationError);
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
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult<SalesReturnResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        Customer? customer = null;
        // التحقق من وجود العميل إن تم تحديده
        if (dto.CustomerId.HasValue)
        {
            customer = await _unitOfWork.Customers.GetByIdForUpdateAsync(dto.CustomerId.Value, ct);
            if (customer is null)
            {
                // إرجاع خطأ عدم وجود العميل
                return ServiceResult<SalesReturnResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }
        }

        // التحقق من عدم تكرار رقم المرتجع
        bool numExists = await _unitOfWork.SalesReturns.NumberExistsAsync(dto.ReturnNumber, ct);
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
        var previousReturnItems = new Dictionary<(Guid ProductId, Guid? ProductBarCodeId), int>();
        if (originalInvoice != null)
        {
            // تجميع بنود الفاتورة الأصلية حسب المنتج
            var invoiceItemsByProduct = originalInvoice.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.ToList());
            // حساب الكميات المرتجعة سابقاً على نفس الفاتورة لكل صنف
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            previousReturnItems = (await _unitOfWork.SalesReturns.GetReturnedItemsByInvoiceAndProductsAsync(originalInvoice.Id, productIds, ct)).GroupBy(ri => (ri.ProductId, ri.ProductBarCodeId)).ToDictionary(g => g.Key, g => g.Sum(ri => ri.Quantity));
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
                    var matchedInvoiceItem = matchingInvoiceItems.FirstOrDefault(i => i.ProductBarCodeId.HasValue) ?? matchingInvoiceItems.FirstOrDefault();
                    item.ProductBarCodeId = matchedInvoiceItem?.ProductBarCodeId;
                }
                else if (matchingInvoiceItems.All(i => i.ProductBarCodeId != item.ProductBarCodeId))
                {
                    // التحقق من مطابقة الباركود المرتجع مع الفاتورة
                    return ServiceResult<SalesReturnResponseDto>.Failure("النكهة / الباركود المحدد غير موجود ضمن بنود الفاتورة الأصلية", ErrorCodes.ValidationError);
                }

                // حساب إجمالي الكمية المباعة للصنف
                var soldQuantity = matchingInvoiceItems.Where(i => i.ProductBarCodeId == item.ProductBarCodeId).Sum(i => i.Quantity);
                previousReturnItems.TryGetValue((item.ProductId, item.ProductBarCodeId), out var previouslyReturned);
                // التحقق من أن كمية الإرجاع موجبة
                if (item.Quantity <= 0)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("كمية المرتجع يجب أن تكون أكبر من صفر", ErrorCodes.ValidationError);
                }

                // التحقق من عدم تجاوز سقف الكمية المباعة
                if (previouslyReturned + item.Quantity > soldQuantity)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure($"الكمية المرتجعة تتجاوز الكمية المباعة (المباعة: {soldQuantity} — المرتجع سابقاً: {previouslyReturned})", ErrorCodes.SalesReturnExceedsSold);
                }
            }

            foreach (var requestedGroup in dto.Items.GroupBy(i => (i.ProductId, i.ProductBarCodeId)))
            {
                var soldQuantity = originalInvoice.Items.Where(i => i.ProductId == requestedGroup.Key.ProductId && i.ProductBarCodeId == requestedGroup.Key.ProductBarCodeId).Sum(i => i.Quantity);
                previousReturnItems.TryGetValue(requestedGroup.Key, out var previouslyReturned);
                if (previouslyReturned + requestedGroup.Sum(i => i.Quantity) > soldQuantity)
                {
                    return ServiceResult<SalesReturnResponseDto>.Failure("إجمالي البنود المكررة في المرتجع يتجاوز الكمية المباعة لنفس الباركود", ErrorCodes.SalesReturnExceedsSold);
                }
            }
        }
        else
        {
            // مرتجع بدون فاتورة: حل الباركودات الافتراضية
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

        var returnProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var productsById = (await _unitOfWork.Products.GetByIdsAsync(returnProductIds, ct)).ToDictionary(p => p.Id);
        if (productsById.Count != returnProductIds.Count)
            return ServiceResult<SalesReturnResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
        var originalAmounts = new Dictionary<(Guid ProductId, Guid? ProductBarCodeId), (decimal Quantity, decimal Amount)>();
        if (originalInvoice is not null)
        {
            var groups = originalInvoice.Items.GroupBy(i => (i.ProductId, i.ProductBarCodeId))
                .OrderBy(g => g.Key.ProductId).ThenBy(g => g.Key.ProductBarCodeId).ToList();
            var weights = groups.Select(g => g.Sum(i => i.LineTotal)).ToArray();
            if (weights.Sum() == 0)
                weights = groups.Select(g => (decimal)g.Sum(i => i.Quantity)).ToArray();
            var amounts = ReturnValuation.Allocate(weights, originalInvoice.TotalAmount);
            for (var index = 0; index < groups.Count; index++)
                originalAmounts[groups[index].Key] = (groups[index].Sum(i => i.Quantity), amounts[index]);
        }

        var salesReturn = _mapper.Map<SalesReturn>(dto);
        salesReturn.ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow : dto.ReturnDate;
        salesReturn.Items = new List<SalesReturnItem>();
        foreach (var item in dto.Items)
        {
            if (item.Quantity <= 0)
                return ServiceResult<SalesReturnResponseDto>.Failure("كمية المرتجع يجب أن تكون أكبر من صفر", ErrorCodes.ValidationError);
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
                ProductId = item.ProductId, ProductBarCodeId = item.ProductBarCodeId, Quantity = item.Quantity,
                UnitPrice = unitPrice, LineTotal = lineTotal, Notes = item.Notes
            });
        }
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
            var previousReturns = await _unitOfWork.SalesReturns.GetByInvoiceAsync(originalInvoice.Id, ct);
            var availableCash = Math.Max(0, originalInvoice.PaidAmount - previousReturns.Sum(r => r.RefundedAmount));
            refundedAmount = Math.Min(salesReturn.TotalAmount, availableCash);
            if (refundedAmount == 0)
                paymentMethod = PaymentMethod.Credit;
        }
        else
        {
            refundedAmount = customer is null ? salesReturn.TotalAmount : 0;
            if (refundedAmount == 0)
                paymentMethod = PaymentMethod.Credit;
        }

        if (paymentMethod == PaymentMethod.Credit && refundedAmount > 0)
            return ServiceResult<SalesReturnResponseDto>.Failure("التسوية الآجلة لا يمكن أن تتضمن مبلغاً نقدياً مرتجعاً", ErrorCodes.ValidationError);
        if (refundedAmount < 0 || refundedAmount > salesReturn.TotalAmount)
            return ServiceResult<SalesReturnResponseDto>.Failure("المبلغ المعاد لا يمكن أن يتجاوز إجمالي المرتجع", ErrorCodes.ValidationError);
        var accountCredit = salesReturn.TotalAmount - refundedAmount;
        if (accountCredit > 0 && customer is null)
            return ServiceResult<SalesReturnResponseDto>.Failure("يجب تحديد العميل لقيد جزء من المرتجع في حسابه", ErrorCodes.ValidationError);
        var safeResult = await _financialPosting.ResolveSafeAsync(salesReturn.BranchId, dto.RefundSafeId, refundedAmount, outbound: true, ct);
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
                var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productIds, ct)).ToDictionary(s => s.ProductId);
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
                var barcodeIds = salesReturn.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
                var stocksByBarcode = barcodeIds.Count > 0 ? (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId) : new Dictionary<Guid, StorgeStock>();
                // زيادة كمية كل باركود في رصيد المستودع
                foreach (var item in salesReturn.Items)
                {
                    if (!item.ProductBarCodeId.HasValue)
                        continue;
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
            if (salesReturn.Id == Guid.Empty)
                salesReturn.Id = Guid.NewGuid();
            await _unitOfWork.SalesReturns.AddAsync(salesReturn, ct);
            if (customer is not null)
            {
                await _financialPosting.PostCustomerBalanceAsync(customer, -salesReturn.TotalAmount, CustomerTransactionType.SalesReturn, salesReturn.BranchId, FinancialDocumentType.SalesReturn, salesReturn.Id, salesReturn.ReturnDate, paymentMethod, $"مرتجع مبيعات رقم {salesReturn.ReturnNumber}", ct);
                await _financialPosting.PostCustomerBalanceAsync(customer, refundedAmount, CustomerTransactionType.Reversal, salesReturn.BranchId, FinancialDocumentType.SalesReturn, salesReturn.Id, salesReturn.ReturnDate, paymentMethod, $"رد نقدي لمرتجع مبيعات رقم {salesReturn.ReturnNumber}", ct);
            }

            if (refundSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(refundSafe, -refundedAmount, FinancialDocumentType.SalesReturn, salesReturn.Id, salesReturn.ReturnDate, $"رد قيمة مرتجع مبيعات رقم {salesReturn.ReturnNumber}", ct);
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
        var created = await _unitOfWork.SalesReturns.GetDetailsAsync(salesReturn.Id, ct) ?? salesReturn;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SalesReturnResponseDto>(created);
        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesReturnResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام سجل المرتجع ككيان متتبع
        var salesReturn = await _unitOfWork.SalesReturns.GetDetailsForUpdateAsync(id, ct);
        // التحقق من وجود المرتجع
        if (salesReturn is null)
        {
            // إرجاع خطأ عدم وجود المرتجع
            return ServiceResult.Failure("مرتجع المبيعات غير موجود", ErrorCodes.SalesReturnNotFound);
        }

        var customer = salesReturn.CustomerId.HasValue ? await _unitOfWork.Customers.GetByIdForUpdateAsync(salesReturn.CustomerId.Value, ct) : null;
        Safe? refundSafe = null;
        if (salesReturn.RefundedAmount > 0)
        {
            if (!salesReturn.RefundSafeId.HasValue)
                return ServiceResult.Failure("لا يمكن عكس مرتجع نقدي قديم دون خزينة مرتبطة", ErrorCodes.ValidationError);
            var safeResult = await _financialPosting.ResolveSafeAsync(salesReturn.BranchId, salesReturn.RefundSafeId, salesReturn.RefundedAmount, outbound: false, ct);
            if (!safeResult.IsSuccess)
                return ServiceResult.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            refundSafe = safeResult.Data;
        }

        var warehouse = salesReturn.Warehouse ?? await _unitOfWork.Warehouses.GetByIdForUpdateAsync(salesReturn.WarehouseId, ct);
        if (warehouse is null || !Enum.IsDefined(warehouse.Type))
            return ServiceResult.Failure("المستودع المرتبط بالمرتجع غير متاح للعكس", ErrorCodes.WarehouseNotFound);
        if (warehouse.Type == WarehouseType.Storge && salesReturn.Items.Any(i => !i.ProductBarCodeId.HasValue))
            return ServiceResult.Failure("لا يمكن عكس مرتجع تخزين دون تحديد باركود لكل بند", ErrorCodes.ValidationError);
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // إلغاء تأثير المرتجع على المخزون (خصم الكميات التي كانت قد أضيفت سابقاً)
            if (salesReturn.Items != null && salesReturn.Items.Any())
            {

                if (warehouse != null)
                {
                    if (warehouse.Type == WarehouseType.Show)
                    {
                        var productIds = salesReturn.Items.Select(i => i.ProductId).Distinct().ToList();
                        var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productIds, ct)).ToDictionary(s => s.ProductId);
                        var totals = salesReturn.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
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
            }

            // تطبيق الحذف المنطقي لسجل المرتجع
            _unitOfWork.SalesReturns.SoftDelete(salesReturn);
            if (customer is not null)
            {
                await _financialPosting.PostCustomerBalanceAsync(customer, -salesReturn.RefundedAmount, CustomerTransactionType.Reversal, salesReturn.BranchId, FinancialDocumentType.SalesReturn, salesReturn.Id, DateTime.UtcNow, salesReturn.PaymentMethod, $"عكس الرد النقدي لمرتجع محذوف رقم {salesReturn.ReturnNumber}", ct);
                await _financialPosting.PostCustomerBalanceAsync(customer, salesReturn.TotalAmount, CustomerTransactionType.Reversal, salesReturn.BranchId, FinancialDocumentType.SalesReturn, salesReturn.Id, DateTime.UtcNow, salesReturn.PaymentMethod, $"عكس مرتجع مبيعات محذوف رقم {salesReturn.ReturnNumber}", ct);
            }

            if (refundSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(refundSafe, salesReturn.RefundedAmount, FinancialDocumentType.SalesReturn, salesReturn.Id, DateTime.UtcNow, $"عكس رد مرتجع مبيعات محذوف رقم {salesReturn.ReturnNumber}", ct);
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
