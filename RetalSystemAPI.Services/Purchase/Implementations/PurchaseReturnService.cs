using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.Models.Common;
using AutoMapper;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Purchase.Interfaces;

namespace RetalSystemAPI.Services.Purchase.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة مرتجعات المشتريات وخصم البضائع المرتجعة من المخازن بدقة على مستوى النكهات والباركودات.
/// </summary>
public class PurchaseReturnService : IPurchaseReturnService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول الكيانات لتحويل النماذج إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;
    private readonly IFinancialPostingService _financialPosting;
    /// <summary>
    /// تهيئة خدمة مرتجعات المشتريات مع حقن وحدة العمل ومحول الكيانات.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name = "mapper">خدمة تحويل النماذج</param>
    public PurchaseReturnService(IUnitOfWork unitOfWork, IMapper mapper, IFinancialPostingService financialPosting)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول البيانات
        _mapper = mapper;
        _financialPosting = financialPosting;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام مرتجع المشتريات بالمعرف مع تضمين الفرع والمستودع والمورد والفاتورة وتفاصيل الأصناف
        var purchaseReturn = await _unitOfWork.PurchaseReturns.GetDetailsAsync(id, ct);
        // التحقق من وجود سجل المرتجع
        if (purchaseReturn is null)
        {
            // إرجاع رسالة خطأ بعدم العثور على مرتجع المشتريات
            return ServiceResult<PurchaseReturnResponseDto>.Failure("مرتجع المشتريات غير موجود", ErrorCodes.PurchaseReturnNotFound);
        }

        // تحويل الكيان إلى كائن الاستجابة المنقول
        var dto = _mapper.Map<PurchaseReturnResponseDto>(purchaseReturn);
        // إرجاع النتيجة الناجحة محملة ببيانات المرتجع
        return ServiceResult<PurchaseReturnResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        // استعلام مرتجع المشتريات بواسطة رقم إشعار المرتجع مع التفاصيل الكاملة
        var purchaseReturn = await _unitOfWork.PurchaseReturns.GetByNumberAsync(returnNumber, ct);
        // التحقق من وجود المرتجع
        if (purchaseReturn is null)
        {
            // إرجاع رسالة خطأ بعدم العثور على المرتجع
            return ServiceResult<PurchaseReturnResponseDto>.Failure("مرتجع المشتريات غير موجود", ErrorCodes.PurchaseReturnNotFound);
        }

        // تحويل الكيان إلى كائن الاستجابة
        var dto = _mapper.Map<PurchaseReturnResponseDto>(purchaseReturn);
        // إرجاع النتيجة الناجحة
        return ServiceResult<PurchaseReturnResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<PurchaseReturnSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, Guid? supplierId = null, PurchaseReturnReason? reason = null, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام القوائم لمرتجعات المشتريات وفق محددات الفلترة
        // جلب قائمة المرتجعات المطابقة
        var returns = await _unitOfWork.PurchaseReturns.ListAsync(new PurchaseReturnQuery { BranchId = branchId, WarehouseId = warehouseId, SupplierId = supplierId, Reason = reason, PaymentMethod = paymentMethod, FromDate = fromDate, ToDate = toDate, Search = search }, ct);
        // تحويل الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<PurchaseReturnSummaryDto>>(returns);
        // إرجاع النتيجة الناجحة مع قائمة الملخصات
        return ServiceResult<IReadOnlyList<PurchaseReturnSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<PurchaseReturnSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, Guid? supplierId = null, PurchaseReturnReason? reason = null, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // بناء مواصفة الفلترة والترقيم لمرتجعات المشتريات
        // تنفيذ الاستعلام الصفحي لجلب سجلات الصفحة وإجمالي العدد
        var(items, totalCount) = await _unitOfWork.PurchaseReturns.SearchAsync(new PurchaseReturnQuery { BranchId = branchId, WarehouseId = warehouseId, SupplierId = supplierId, Reason = reason, PaymentMethod = paymentMethod, FromDate = fromDate, ToDate = toDate, Search = search, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل عناصر الصفحة الحالية إلى قائمة ملخصات
        var dtos = _mapper.Map<IReadOnlyList<PurchaseReturnSummaryDto>>(items);
        // إنشاء نتيجة الترقيم مع حساب عدد الصفحات
        var pagedResult = PagedResult<PurchaseReturnSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة الناجحة المصفحة
        return ServiceResult<PagedResult<PurchaseReturnSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseReturnResponseDto>> CreateAsync(CreatePurchaseReturnDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<PurchaseReturnResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // 1. التحقق من وجود المورد المحدد
        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(dto.SupplierId, ct);
        // في حال عدم وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم وجود المورد
            return ServiceResult<PurchaseReturnResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // 5. التحقق من عدم تكرار رقم إشعار المرتجع
        bool numExists = await _unitOfWork.PurchaseReturns.NumberExistsAsync(dto.ReturnNumber, ct);
        // في حال كان الرقم مسجلاً مسبقاً
        if (numExists)
        {
            // إرجاع خطأ تكرار رقم المرتجع
            return ServiceResult<PurchaseReturnResponseDto>.Failure("رقم إشعار المرتجع مستخدم بالفعل", ErrorCodes.PurchaseReturnNumberExists);
        }

        // 6. الربط بالفاتورة الأصلية: يرتبط حصراً بمورد ومستودع الفاتورة وبنودها مع سقف الكمية المرتجعة
        PurchaseInvoice? originalInvoice = null;
        // إذا حُدد معرف فاتورة أصلية
        if (dto.PurchaseInvoiceId.HasValue)
        {
            // استعلام الفاتورة الأصلية بكامل تفاصيلها
            originalInvoice = await _unitOfWork.PurchaseInvoices.GetDetailsAsync(dto.PurchaseInvoiceId.Value, ct);
            // في حال عدم العثور على الفاتورة
            if (originalInvoice is null)
            {
                // إرجاع خطأ عدم وجود الفاتورة
                return ServiceResult<PurchaseReturnResponseDto>.Failure("فاتورة الشراء الأصلية المحددة غير موجودة", ErrorCodes.PurchaseOrderNotFound);
            }

            // التحقق من تطابق المورد مع مورد الفاتورة الأصلية
            if (originalInvoice.SupplierId != dto.SupplierId)
            {
                // إرجاع خطأ عدم تطابق المورد
                return ServiceResult<PurchaseReturnResponseDto>.Failure("فاتورة الشراء الأصلية لا تنتمي للمورد المختار", ErrorCodes.ValidationError);
            }

            // منع إنشاء مرتجع على فاتورة ملغاة أو باطلة
            if (originalInvoice.Status is not (InvoiceStatus.Pending or InvoiceStatus.Paid or InvoiceStatus.PartiallyPaid))
            {
                // إرجاع خطأ عدم جواز الإرجاع على فاتورة ملغاة
                return ServiceResult<PurchaseReturnResponseDto>.Failure("يجب أن تكون الفاتورة الأصلية مرحلة وغير ملغاة", ErrorCodes.ValidationError);
            }

            // الفرع يعود للفاتورة، أما المخزن المصدر فيحدده الطلب بعد تحويل البضاعة إليه عند الحاجة.
            dto.BranchId = originalInvoice.BranchId;
        }

        // 2. التحقق من وجود الفرع المحدد
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        // في حال عدم وجود الفرع
        if (!branchExists)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<PurchaseReturnResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // 3. التحقق من وجود المستودع
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        // في حال عدم وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult<PurchaseReturnResponseDto>.Failure("المستودع المحدد غير موجود", ErrorCodes.WarehouseNotFound);
        }

        // 4. قاعدة العمل الصارمة: الإرجاع للموردين يتم حصرياً من المستودعات الرئيسية (المخازن) لضمان دقة النكهات
        if (warehouse.Type != WarehouseType.Storge)
        {
            // إرجاع خطأ اشتراط الإرجاع من المخزن الرئيسي حصراً
            return ServiceResult<PurchaseReturnResponseDto>.Failure("مرتجع المشتريات متاح فقط من المستودع الرئيسي (المخزن) لضمان دقة النكهات والباركودات. يرجى تحويل البضاعة من الصالة إلى المخزن أولاً عبر التحويلات المخزنية.", ErrorCodes.PurchaseReturnInvalidWarehouse);
        }

        if (warehouse.BranchId != dto.BranchId)
            return ServiceResult<PurchaseReturnResponseDto>.Failure("المخزن المصدر لا يتبع فرع مرتجع المشتريات", ErrorCodes.ValidationError);

        // 7. التحقق من وجود بنود للمرتجع
        if (dto.Items == null || !dto.Items.Any())
        {
            // إرجاع خطأ اشتراط وجود بند واحد على الأقل
            return ServiceResult<PurchaseReturnResponseDto>.Failure("يجب إضافة بند واحد على الأقل لمرتجع المشتريات", ErrorCodes.ValidationError);
        }

        // 8. التحقق من كفاية المخزون في المخزن لكل صنف ونكهة/باركود — دفعات موحدة
        var itemProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var productsById = (await _unitOfWork.Products.GetByIdsAsync(itemProductIds, ct)).ToDictionary(p => p.Id);
        var barcodes = (await _unitOfWork.Products.GetBarcodesByProductIdsAsync(itemProductIds, ct)).ToList();
        var barcodeById = barcodes.ToDictionary(b => b.Id);
        var defaults = barcodes.GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.OrderBy(b => b.Id).First().Id);
        var originalLots = originalInvoice is null ? null : await GetOriginalLotsAsync(originalInvoice, ct);
        var validatedItems = new List<(CreatePurchaseReturnItemDto dtoItem, Guid barcodeId)>();
        foreach (var item in dto.Items)
        {
            if (!productsById.ContainsKey(item.ProductId))
                return ServiceResult<PurchaseReturnResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
            var barcodeId = item.ProductBarCodeId ?? (defaults.TryGetValue(item.ProductId, out var fallback) ? fallback : (Guid?)null);
            if (!barcodeId.HasValue || !barcodeById.TryGetValue(barcodeId.Value, out var barcode) || barcode.ProductId != item.ProductId)
                return ServiceResult<PurchaseReturnResponseDto>.Failure("الباركود غير موجود أو لا يتبع الصنف المحدد", ErrorCodes.BarCodeNotFound);
            if (!StockQuantity.IsValid(item.Quantity))
                return ServiceResult<PurchaseReturnResponseDto>.Failure("كمية المرتجع يجب أن تكون عدداً صحيحاً موجباً", ErrorCodes.PurchaseReturnInvalidQuantity);
            if (originalLots is not null && !originalLots.ContainsKey((item.ProductId, barcodeId.Value)))
                return ServiceResult<PurchaseReturnResponseDto>.Failure("الباركود المحدد غير موجود في الفاتورة الأصلية أو تفاصيلها", ErrorCodes.ValidationError);
            validatedItems.Add((item, barcodeId.Value));
        }
        var previousByBarcode = new Dictionary<Guid, decimal>();
        if (originalInvoice is not null)
        {
            var ids = validatedItems.Select(v => v.barcodeId).Distinct().ToList();
            previousByBarcode = (await _unitOfWork.PurchaseReturns.GetReturnedItemsByInvoiceAndBarcodesAsync(originalInvoice.Id, ids, ct))
                .GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
            foreach (var group in validatedItems.GroupBy(v => v.barcodeId))
            {
                var lot = originalLots![(group.First().dtoItem.ProductId, group.Key)];
                if (previousByBarcode.GetValueOrDefault(group.Key) + group.Sum(v => v.dtoItem.Quantity) > lot.Quantity)
                    return ServiceResult<PurchaseReturnResponseDto>.Failure("إجمالي الكمية المرتجعة يتجاوز الكمية المشتراة للباركود", ErrorCodes.PurchaseReturnInvalidQuantity);
            }
        }
        // جلب أرصدة المخزن المعنية دفعة واحدة لتفادي استعلامات N+1
        var validatedBarcodeIds = validatedItems.Select(v => v.barcodeId).Distinct().ToList();
        // استعلام أرصدة المخزن المتتبعة
        var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, validatedBarcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId);
        // التحقق من كفاية الرصيد المخزني الفعلي لكل بند
        foreach (var group in validatedItems.GroupBy(v => v.barcodeId))
        {
            // محاولة جلب رصيد الباركود
            stocksByBarcode.TryGetValue(group.Key, out var stock);
            var returnQty = group.Sum(v => StockQuantity.ToInt32(v.dtoItem.Quantity));
            // فحص كفاية الرصيد في المخزن
            if (stock is null || stock.Quantity < returnQty)
            {
                // الكمية المتوفرة حالياً
                int available = stock?.Quantity ?? 0;
                // إرجاع خطأ عدم كفاية الرصيد المخزني للإرجاع
                return ServiceResult<PurchaseReturnResponseDto>.Failure($"رصيد المخزون المتوفر في المخزن للصنف '{productsById[group.First().dtoItem.ProductId].Name}' ({available}) غير كافٍ لإتمام الإرجاع ({returnQty}).", ErrorCodes.InsufficientStock);
            }
        }

        // 9. إنشاء كيان المرتجع
        var purchaseReturn = _mapper.Map<PurchaseReturn>(dto);
        // ضبط تاريخ المرتجع بالوقت الحالي إن لم يُحدد
        purchaseReturn.ReturnDate = dto.ReturnDate == default ? DateTime.UtcNow : dto.ReturnDate;
        // تفريغ خصائص التنقل
        purchaseReturn.Supplier = null!;
        purchaseReturn.Branch = null!;
        purchaseReturn.Warehouse = null!;
        purchaseReturn.PurchaseInvoice = null;
        // تجهيز بنود المرتجع واحتساب إجماليات السطور
        purchaseReturn.Items = new List<PurchaseReturnItem>();
        foreach (var (item, barcodeId) in validatedItems)
        {
            decimal unitPrice, lineTotal;
            if (originalLots is not null)
            {
                var lot = originalLots[(item.ProductId, barcodeId)];
                var previous = previousByBarcode.GetValueOrDefault(barcodeId);
                unitPrice = decimal.Round(lot.Amount / lot.Quantity, 4);
                lineTotal = ReturnValuation.ForQuantity(lot.Amount, lot.Quantity, previous, item.Quantity);
                previousByBarcode[barcodeId] = previous + item.Quantity;
            }
            else
            {
                var product = productsById[item.ProductId];
                unitPrice = product.AveragePrice > 0 ? product.AveragePrice : product.CostPrice;
                lineTotal = decimal.Round(item.Quantity * unitPrice, 4);
            }
            purchaseReturn.Items.Add(new PurchaseReturnItem
            {
                ProductId = item.ProductId, ProductBarCodeId = barcodeId, Quantity = item.Quantity,
                UnitPrice = unitPrice, LineTotal = lineTotal, Notes = item.Notes
            });
        }
        purchaseReturn.TotalAmount = purchaseReturn.Items.Sum(i => i.LineTotal);
        Safe? refundSafe = null;
        if (purchaseReturn.PaymentMethod != PaymentMethod.Credit)
        {
            var safeResult = await _financialPosting.ResolveSafeAsync(purchaseReturn.BranchId, dto.RefundSafeId, purchaseReturn.TotalAmount, outbound: false, ct);
            if (!safeResult.IsSuccess)
                return ServiceResult<PurchaseReturnResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            refundSafe = safeResult.Data;
            purchaseReturn.RefundSafeId = refundSafe?.Id;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // 10. خصم البضاعة المرتجعة من مخزون المستودع الرئيسي — على نفس الأرصدة المتتبعة أعلاه
            foreach (var item in purchaseReturn.Items)
            {
                // استخراج رصيد الباركود وتخفيضه بالكمية المرتجعة
                if (stocksByBarcode.TryGetValue(item.ProductBarCodeId!.Value, out var stock))
                {
                    // خصم الكمية المرتجعة من رصيد المخزن
                    stock.Quantity = checked(stock.Quantity - StockQuantity.ToInt32(item.Quantity));
                }
            }

            // إضافة سجل المرتجع إلى المستودع
            if (purchaseReturn.Id == Guid.Empty)
                purchaseReturn.Id = Guid.NewGuid();
            await _unitOfWork.PurchaseReturns.AddAsync(purchaseReturn, ct);
            await _financialPosting.PostSupplierBalanceAsync(supplier, -purchaseReturn.TotalAmount, SupplierTransactionType.PurchaseReturn, purchaseReturn.BranchId, FinancialDocumentType.PurchaseReturn, purchaseReturn.Id, purchaseReturn.ReturnDate, purchaseReturn.PaymentMethod, $"مرتجع مشتريات رقم {purchaseReturn.ReturnNumber}", ct);
            if (refundSafe is not null)
            {
                await _financialPosting.PostSupplierBalanceAsync(supplier, purchaseReturn.TotalAmount, SupplierTransactionType.Reversal, purchaseReturn.BranchId, FinancialDocumentType.PurchaseReturn, purchaseReturn.Id, purchaseReturn.ReturnDate, purchaseReturn.PaymentMethod, $"تحصيل نقدي لمرتجع مشتريات رقم {purchaseReturn.ReturnNumber}", ct);
                await _financialPosting.PostSafeBalanceAsync(refundSafe, purchaseReturn.TotalAmount, FinancialDocumentType.PurchaseReturn, purchaseReturn.Id, purchaseReturn.ReturnDate, $"تحصيل قيمة مرتجع مشتريات رقم {purchaseReturn.ReturnNumber}", ct);
            }

            // حفظ كافة التغييرات وحركات المخزون في قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // إعادة تحميل سجل المرتجع بكامل تفاصيله
        var createdReturn = await _unitOfWork.PurchaseReturns.GetDetailsAsync(purchaseReturn.Id, ct) ?? purchaseReturn;
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<PurchaseReturnResponseDto>(createdReturn);
        // إرجاع النتيجة الناجحة مع بيانات المرتجع
        return ServiceResult<PurchaseReturnResponseDto>.Success(responseDto);
    }

    private async Task<Dictionary<(Guid ProductId, Guid BarcodeId), (decimal Quantity, decimal Amount)>> GetOriginalLotsAsync(
        PurchaseInvoice invoice, CancellationToken ct)
    {
        var missingProducts = invoice.Items.Where(i => !i.ProductBarCodeId.HasValue && i.Breakdowns.Count == 0)
            .Select(i => i.ProductId).Distinct().ToList();
        var defaults = (await _unitOfWork.Products.GetBarcodesByProductIdsAsync(missingProducts, ct))
            .GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.OrderBy(b => b.Id).First().Id);
        var lots = new List<(Guid ProductId, Guid BarcodeId, decimal Quantity, decimal Weight)>();
        foreach (var item in invoice.Items.OrderBy(i => i.Id))
        {
            if (item.Breakdowns.Count > 0)
            {
                var breakdowns = item.Breakdowns.OrderBy(b => b.Id).ToList();
                var weights = breakdowns.Select(b => b.Quantity * (b.UnitPrice > 0 ? b.UnitPrice : item.UnitPrice)).ToArray();
                if (weights.Sum() == 0) weights = breakdowns.Select(b => b.Quantity).ToArray();
                var amounts = ReturnValuation.Allocate(weights, item.LineTotal);
                for (var index = 0; index < breakdowns.Count; index++)
                    lots.Add((item.ProductId, breakdowns[index].ProductBarCodeId, breakdowns[index].Quantity, amounts[index]));
            }
            else
            {
                var barcodeId = item.ProductBarCodeId ?? (defaults.TryGetValue(item.ProductId, out var fallback) ? fallback : (Guid?)null);
                if (barcodeId.HasValue) lots.Add((item.ProductId, barcodeId.Value, item.Quantity, item.LineTotal));
            }
        }
        var invoiceWeights = lots.Select(l => l.Weight).ToArray();
        if (invoiceWeights.Sum() == 0) invoiceWeights = lots.Select(l => l.Quantity).ToArray();
        var totals = ReturnValuation.Allocate(invoiceWeights, invoice.TotalAmount);
        return lots.Select((lot, index) => (lot.ProductId, lot.BarcodeId, lot.Quantity, Amount: totals[index]))
            .GroupBy(l => (l.ProductId, l.BarcodeId))
            .ToDictionary(g => g.Key, g => (g.Sum(l => l.Quantity), g.Sum(l => l.Amount)));
    }
    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع: التحميل بدون تتبع ينشئ نسخاً متعددة للكيان نفسه عند تكرار
        // الصنف في البنود، ثم يفشل Attach داخل SoftDelete بتضارب المفاتيح
        var purchaseReturn = await _unitOfWork.PurchaseReturns.GetDetailsForUpdateAsync(id, ct);
        // التحقق من وجود سجل المرتجع المطلوب حذفه
        if (purchaseReturn is null)
        {
            // إرجاع خطأ بعدم وجود سجل المرتجع
            return ServiceResult.Failure("مرتجع المشتريات غير موجود", ErrorCodes.PurchaseReturnNotFound);
        }

        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(purchaseReturn.SupplierId, ct);
        if (supplier is null)
            return ServiceResult.Failure("المورد المرتبط بالمرتجع غير موجود", ErrorCodes.SupplierNotFound);
        Safe? refundSafe = null;
        if (purchaseReturn.PaymentMethod != PaymentMethod.Credit)
        {
            if (!purchaseReturn.RefundSafeId.HasValue)
                return ServiceResult.Failure("لا يمكن عكس مرتجع نقدي قديم دون خزينة مرتبطة", ErrorCodes.ValidationError);
            var safeResult = await _financialPosting.ResolveSafeAsync(purchaseReturn.BranchId, purchaseReturn.RefundSafeId, purchaseReturn.TotalAmount, outbound: true, ct);
            if (!safeResult.IsSuccess)
                return ServiceResult.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            refundSafe = safeResult.Data;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // إعادة البضاعة المرتجعة إلى المخزن عند حذف أو إلغاء سجل المرتجع — دفعة أرصدة واحدة
            if (purchaseReturn.Items != null && purchaseReturn.Items.Any())
            {
                // استخراج معرفات الباركودات للبنود
                var barcodeIds = purchaseReturn.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
                // إذا وجدت باركودات يتم تحميل أرصدتها وإعادتها للمخزن
                if (barcodeIds.Count > 0)
                {
                    // استعلام أرصدة المخزن المتتبعة دفعة واحدة
                    var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(purchaseReturn.WarehouseId, barcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId);
                    // زيادة الأرصدة بالكميات المحذوفة من المرتجع
                    foreach (var item in purchaseReturn.Items)
                    {
                        // التحقق من وجود الباركود وتحديث الرصيد
                        if (item.ProductBarCodeId.HasValue && stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                        {
                            // إعادة الكمية إلى المخزن
                            stock.Quantity = checked(stock.Quantity + StockQuantity.ToInt32(item.Quantity));
                        }
                    }
                }
            }

            // تنفيذ الحذف المنطقي لسجل المرتجع
            _unitOfWork.PurchaseReturns.SoftDelete(purchaseReturn);
            if (refundSafe is not null)
            {
                await _financialPosting.PostSupplierBalanceAsync(supplier, -purchaseReturn.TotalAmount, SupplierTransactionType.Reversal, purchaseReturn.BranchId, FinancialDocumentType.PurchaseReturn, purchaseReturn.Id, DateTime.UtcNow, purchaseReturn.PaymentMethod, $"عكس التحصيل النقدي لمرتجع مشتريات محذوف رقم {purchaseReturn.ReturnNumber}", ct);
                await _financialPosting.PostSafeBalanceAsync(refundSafe, -purchaseReturn.TotalAmount, FinancialDocumentType.PurchaseReturn, purchaseReturn.Id, DateTime.UtcNow, $"عكس تحصيل مرتجع مشتريات محذوف رقم {purchaseReturn.ReturnNumber}", ct);
            }

            await _financialPosting.PostSupplierBalanceAsync(supplier, purchaseReturn.TotalAmount, SupplierTransactionType.Reversal, purchaseReturn.BranchId, FinancialDocumentType.PurchaseReturn, purchaseReturn.Id, DateTime.UtcNow, purchaseReturn.PaymentMethod, $"عكس مرتجع مشتريات محذوف رقم {purchaseReturn.ReturnNumber}", ct);
            // حفظ التغييرات في قاعدة البيانات
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
