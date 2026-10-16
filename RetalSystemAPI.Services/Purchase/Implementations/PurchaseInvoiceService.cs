using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.Models.Common;
using AutoMapper;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Purchase.Interfaces;

namespace RetalSystemAPI.Services.Purchase.Implementations;
/// <summary>
/// تنفيذ خدمة فواتير المشتريات وإدارة استلام المخزون حسب تفصيل النكهات واحتساب التكاليف والضرائب.
/// </summary>
public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;
    private readonly IFinancialPostingService _financialPosting;
    /// <summary>
    /// تهيئة خدمة فواتير المشتريات مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل للتعامل مع قاعدة البيانات</param>
    /// <param name = "mapper">خدمة تحويل البيانات</param>
    public PurchaseInvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IFinancialPostingService financialPosting)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
        _financialPosting = financialPosting;
    }

    /// <summary>
    /// تحويل نتيجة القراءة المخصصة إلى استجابة الخدمة وإضافة مسميات العرض.
    /// </summary>
    private static PurchaseInvoiceResponseDto? MapInvoiceResponse(PurchaseInvoiceReadResult? p) => p is null ? null : new PurchaseInvoiceResponseDto
    {
        Id = p.Id,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        InvoiceNumber = p.InvoiceNumber,
        InvoiceDate = p.InvoiceDate,
        SupplierId = p.SupplierId,
        SupplierName = p.SupplierName,
        BranchId = p.BranchId,
        BranchName = p.BranchName,
        WarehouseId = p.WarehouseId,
        WarehouseName = p.WarehouseName,
        Status = p.Status,
        StatusName = p.Status == InvoiceStatus.Draft ? "مسودة" : p.Status == InvoiceStatus.Pending ? "مفتوحة قيد الإدخال" : p.Status == InvoiceStatus.Paid ? "مغلقة ومرحلة" : p.Status == InvoiceStatus.PartiallyPaid ? "مسددة جزئياً" : p.Status == InvoiceStatus.Cancelled ? "ملغاة" : "باطلة",
        PaymentMethod = p.PaymentMethod,
        PaymentMethodName = p.PaymentMethod == PaymentMethod.Cash ? "نقدي" : p.PaymentMethod == PaymentMethod.BankTransfer ? "تحويل مصرفي" : p.PaymentMethod == PaymentMethod.CreditCard ? "بطاقة مصرفية" : p.PaymentMethod == PaymentMethod.Credit ? "آجل" : "صك",
        SubTotal = p.SubTotal,
        DiscountAmount = p.DiscountAmount,
        TaxAmount = p.TaxAmount,
        TotalAmount = p.TotalAmount,
        PaidAmount = p.PaidAmount,
        PaymentSafeId = p.PaymentSafeId,
        RemainingAmount = p.RemainingAmount,
        PurchaseOrderId = p.PurchaseOrderId,
        PurchaseOrderNumber = p.PurchaseOrderNumber,
        Notes = p.Notes,
        Items = p.Items.Select(MapInvoiceItem).ToList()
    };
    private static PurchaseInvoiceItemResponseDto MapInvoiceItem(PurchaseInvoiceItemReadResult p) => new PurchaseInvoiceItemResponseDto
    {
        Id = p.Id,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        ProductId = p.ProductId,
        ProductName = p.ProductName,
        ProductCode = p.ProductCode,
        ProductBarCodeId = p.ProductBarCodeId,
        BarCode = p.BarCode,
        Quantity = p.Quantity,
        UnitPrice = p.UnitPrice,
        DiscountAmount = p.DiscountAmount,
        LineTotal = p.LineTotal,
        Breakdowns = p.Breakdowns.Select(MapInvoiceBreakdown).ToList()
    };
    private static PurchaseInvoiceItemBreakdownResponseDto MapInvoiceBreakdown(PurchaseInvoiceBreakdownReadResult p) => new PurchaseInvoiceItemBreakdownResponseDto
    {
        Id = p.Id,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        ProductBarCodeId = p.ProductBarCodeId,
        BarCode = p.BarCode,
        Title = p.Title,
        PackageQuantity = p.PackageQuantity,
        UnitsPerPackage = p.UnitsPerPackage,
        Quantity = p.Quantity,
        UnitPrice = p.UnitPrice
    };
    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? supplierId = null, Guid? branchId = null, Guid? warehouseId = null, InvoiceStatus? status = null, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? searchTerm = null, CancellationToken ct = default)
    {
        // بناء مواصفة فلترة وترقيم فواتير المشتريات وفق المعايير المدخلة
        // تنفيذ الاستعلام الصفحي لجلب عناصر الصفحة والعدد الكلي للسجلات
        var(items, totalCount) = await _unitOfWork.PurchaseInvoices.SearchAsync(new PurchaseInvoiceQuery { SupplierId = supplierId, BranchId = branchId, WarehouseId = warehouseId, Status = status, PaymentMethod = paymentMethod, FromDate = fromDate, ToDate = toDate, SearchTerm = searchTerm, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل عناصر الصفحة إلى قائمة ملخصات فواتير المشتريات
        var dtos = _mapper.Map<IReadOnlyList<PurchaseInvoiceSummaryDto>>(items);
        // إنشاء كائن النتيجة المصفحة مع حساب أرقام الصفحات
        var pagedResult = PagedResult<PurchaseInvoiceSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة الناجحة المصفحة
        return ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // الاستعلام عن فاتورة المشتريات بالمعرف مع تفاصيل البنود وتفريعات النكهات بالإسقاط المباشر
        var dto = MapInvoiceResponse(await _unitOfWork.PurchaseInvoices.GetReadResultAsync(id, ct));
        // التحقق من وجود الفاتورة في قاعدة البيانات
        if (dto is null)
        {
            // إرجاع رسالة خطأ بعدم العثور على الفاتورة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // إرجاع النتيجة الناجحة مع بيانات الفاتورة
        return ServiceResult<PurchaseInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        if (dto.Status == InvoiceStatus.Draft)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("إنشاء الفاتورة هنا يرحّل المخزون والحسابات؛ لا يمكن إرسالها كمسودة", ErrorCodes.ValidationError);

        if (!Enum.IsDefined(dto.PaymentMethod))
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("طريقة الدفع غير صحيحة", ErrorCodes.ValidationError);
        if (dto.Items is { Count: > 0 })
        {
            var itemError = await ValidateItemsAsync(dto.Items, ct);
            if (itemError is not null)
                return ServiceResult<PurchaseInvoiceResponseDto>.Failure(itemError, ErrorCodes.ValidationError);
        }

        if (!HasMoneyPrecision(dto.DiscountAmount) || !HasMoneyPrecision(dto.TaxAmount) || !HasMoneyPrecision(dto.PaidAmount))
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المبالغ المالية تدعم أربع منازل عشرية كحد أقصى", ErrorCodes.ValidationError);
        // التحقق من وجود المورد المحدد في قاعدة البيانات
        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(dto.SupplierId, ct);
        // في حال عدم وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ بعدم وجود المورد
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
        }

        if (!supplier.IsActive)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المورد المحدد معطل حالياً", ErrorCodes.ValidationError);
        // التحقق من وجود الفرع المسجل
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        // في حال عدم وجود الفرع
        if (!branchExists)
        {
            // إرجاع خطأ بعدم وجود الفرع
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // الاستعلام عن المستودع أو الصالة المستلمة للبضاعة
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        // في حال عدم وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ بعدم وجود المستودع أو الصالة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        var barcodeError = await ResolveStorageBarcodesAsync(dto.Items, warehouse, ct);
        if (barcodeError is not null)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure(barcodeError, ErrorCodes.ValidationError);

        // التحقق من عدم تكرار رقم فاتورة المشتريات
        bool numExists = await _unitOfWork.PurchaseInvoices.NumberExistsAsync(dto.InvoiceNumber, ct);
        // في حال وجود رقم الفاتورة مسبقاً
        if (numExists)
        {
            // إرجاع خطأ تكرار رقم الفاتورة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("رقم فاتورة المشتريات مستخدم بالفعل", ErrorCodes.PurchaseOrderNumberExists);
        }

        // التحقق من احتواء الفاتورة على بند واحد على الأقل
        if (dto.Items == null || !dto.Items.Any())
        {
            // إرجاع خطأ وجوب إضافة بنود للفاتورة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("يجب إضافة بند واحد على الأقل لفاتورة المشتريات", ErrorCodes.ValidationError);
        }

        foreach (var item in dto.Items)
        {
            var gross = item.Quantity * item.UnitPrice;
            if (item.Quantity <= 0 || item.UnitPrice < 0 || item.DiscountAmount < 0 || item.DiscountAmount > gross)
                return ServiceResult<PurchaseInvoiceResponseDto>.Failure("بيانات كمية أو سعر أو خصم أحد البنود غير صحيحة", ErrorCodes.ValidationError);
        }

        if (dto.DiscountAmount < 0 || dto.TaxAmount < 0)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("الخصم والضريبة لا يقبلان قيماً سالبة", ErrorCodes.ValidationError);
        // تحويل بيانات الإدخال إلى كيان فاتورة المشتريات
        var invoice = _mapper.Map<PurchaseInvoice>(dto);
        // تفريغ كائنات التنقل لتجنب تتبع كيانات غير كاملة
        invoice.Supplier = null!;
        invoice.Branch = null!;
        invoice.Warehouse = null!;
        invoice.PurchaseOrder = null;
        // تجهيز بنود الفاتورة وتفصيلات النكهات واحتساب إجماليات السطور
        invoice.Items = dto.Items.Select(item =>
        {
            // إنشاء كيان بند فاتورة المشتريات
            var invItem = new PurchaseInvoiceItem
            {
                // تعيين معرف الصنف
                ProductId = item.ProductId,
                // تعيين معرف الباركود إن وجد
                ProductBarCodeId = item.ProductBarCodeId,
                // تعيين الكمية الإجمالية للبند
                Quantity = item.Quantity,
                // تعيين سعر الوحدة
                UnitPrice = item.UnitPrice,
                // تعيين مبلغ الخصم على البند
                DiscountAmount = item.DiscountAmount,
                // احتساب إجمالي البند بعد الخصم
                LineTotal = Math.Max(0, (item.Quantity * item.UnitPrice) - item.DiscountAmount)
            };
            // معالجة تفصيلات النكهات والطرود للبند إن وُجدت
            if (item.Breakdowns != null && item.Breakdowns.Any())
            {
                // تحويل تفصيلات النكهات إلى كيانات تفريعات البند
                invItem.Breakdowns = item.Breakdowns.Select(b => new PurchaseInvoiceItemBreakdown { // معرف باركود النكهة
 ProductBarCodeId = b.ProductBarCodeId, // عدد الكراتين أو الطرود
 PackageQuantity = b.PackageQuantity, // عدد الوحدات في كل طرد
 UnitsPerPackage = b.UnitsPerPackage, // احتساب الكمية الإجمالية بالوحدات
 Quantity = b.Quantity > 0 ? b.Quantity : (b.PackageQuantity * (b.UnitsPerPackage > 0 ? b.UnitsPerPackage : 1)), // سعر الوحدة للنكهة
 UnitPrice = b.UnitPrice > 0 ? b.UnitPrice : item.UnitPrice }).ToList();
            }

            // إرجاع بند الفاتورة المجهز
            return invItem;
        }).ToList();
        // احتساب الإجمالي الفرعي للفاتورة
        invoice.SubTotal = invoice.Items.Sum(i => i.LineTotal);
        // احتساب المبلغ الإجمالي بعد الخصومات والضرائب
        invoice.TotalAmount = Math.Max(0, invoice.SubTotal - invoice.DiscountAmount + invoice.TaxAmount);
        if (invoice.DiscountAmount > invoice.SubTotal + invoice.TaxAmount)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("خصم الفاتورة يتجاوز قيمة البنود والضريبة", ErrorCodes.ValidationError);
        if (dto.PaidAmount < 0 || dto.PaidAmount > invoice.TotalAmount)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المبلغ المدفوع لا يمكن أن يتجاوز إجمالي الفاتورة", ErrorCodes.ValidationError);
        invoice.PaidAmount = dto.PaidAmount;
        invoice.RemainingAmount = invoice.TotalAmount - invoice.PaidAmount;
        invoice.Status = invoice.PaidAmount == invoice.TotalAmount ? InvoiceStatus.Paid : invoice.PaidAmount > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Pending;
        var safeResult = await _financialPosting.ResolveSafeAsync(dto.BranchId, dto.PaymentSafeId, invoice.PaidAmount, outbound: true, ct);
        if (!safeResult.IsSuccess)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
        Safe? paymentSafe = safeResult.Data;
        invoice.PaymentSafeId = paymentSafe?.Id;
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // إضافة فاتورة المشتريات إلى المستودع
            if (invoice.Id == Guid.Empty)
                invoice.Id = Guid.NewGuid();
            await _unitOfWork.PurchaseInvoices.AddAsync(invoice, ct);
            // ── زيادة رصيد المخزون فوراً وتحديث أسعار التكلفة عند إنشاء الفاتورة وإضافة البند ──
            // زيادة أرصدة المخزون بالكميات المستلمة في المستودع
            await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: true, ct);
            // تحديث سعر التكلفة ومتوسط السعر للأصناف الواردة في الفاتورة
            await UpdateProductsCostAsync(invoice.Items, ct);
            await _financialPosting.PostSupplierBalanceAsync(supplier, invoice.TotalAmount, SupplierTransactionType.PurchaseInvoice, invoice.BranchId, FinancialDocumentType.PurchaseInvoice, invoice.Id, invoice.InvoiceDate, invoice.PaymentMethod, $"فاتورة مشتريات رقم {invoice.InvoiceNumber}", ct);
            await _financialPosting.PostSupplierBalanceAsync(supplier, -invoice.PaidAmount, SupplierTransactionType.Payment, invoice.BranchId, FinancialDocumentType.PurchaseInvoice, invoice.Id, invoice.InvoiceDate, invoice.PaymentMethod, $"دفعة فاتورة مشتريات رقم {invoice.InvoiceNumber}", ct);
            if (paymentSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(paymentSafe, -invoice.PaidAmount, FinancialDocumentType.PurchaseInvoice, invoice.Id, invoice.InvoiceDate, $"سداد فاتورة مشتريات رقم {invoice.InvoiceNumber}", ct);
            }

            // حفظ كافة التغييرات وحركات المخزون في معاملة واحدة
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // إعادة تحميل الفاتورة بالتفاصيل الكاملة عبر الإسقاط المباشر
        var responseDto = MapInvoiceResponse(await _unitOfWork.PurchaseInvoices.GetReadResultAsync(invoice.Id, ct)) ?? _mapper.Map<PurchaseInvoiceResponseDto>(invoice);
        // إرجاع النتيجة الناجحة مع بيانات الفاتورة المنشأة
        return ServiceResult<PurchaseInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        if (!Enum.IsDefined(dto.PaymentMethod))
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("طريقة الدفع غير صحيحة", ErrorCodes.ValidationError);
        if (dto.Items is not null)
        {
            if (dto.Items.Count == 0)
                return ServiceResult<PurchaseInvoiceResponseDto>.Failure("يجب إضافة بند واحد على الأقل لفاتورة المشتريات", ErrorCodes.ValidationError);
            var itemError = await ValidateItemsAsync(dto.Items, ct);
            if (itemError is not null)
                return ServiceResult<PurchaseInvoiceResponseDto>.Failure(itemError, ErrorCodes.ValidationError);
        }

        if (!HasMoneyPrecision(dto.DiscountAmount) || !HasMoneyPrecision(dto.TaxAmount) || !HasMoneyPrecision(dto.PaidAmount))
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المبالغ المالية تدعم أربع منازل عشرية كحد أقصى", ErrorCodes.ValidationError);
        // استعلام فاتورة المشتريات بالمعرف مع بنودها وتفصيلاتها
        var invoice = await _unitOfWork.PurchaseInvoices.GetDetailsAsync(id, ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ بعدم العثور على الفاتورة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        if (await _unitOfWork.PurchaseReturns.HasInvoiceReferencesAsync(id, ct))
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة لها مرتجعات مشتريات؛ اعكس المرتجعات أولاً", ErrorCodes.ValidationError);

        // لا يعدل بعد الإغلاق؛ الفاتورة المفتوحة Pending فقط تقبل الإضافة التراكمية.
        if (invoice.Status is not InvoiceStatus.Draft and not InvoiceStatus.Pending)
        {
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة مشتريات مرحلة — استخدم الإلغاء أو مرتجع المشتريات", ErrorCodes.ValidationError);
        }

        if (dto.SupplierId != invoice.SupplierId || dto.BranchId != invoice.BranchId || dto.WarehouseId != invoice.WarehouseId)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("لا يمكن تغيير المورد أو الفرع أو المستودع بعد بدء ترحيل بنود الفاتورة", ErrorCodes.ValidationError);
        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(invoice.SupplierId, ct);
        if (supplier is null)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المورد المرتبط بالفاتورة غير موجود", ErrorCodes.SupplierNotFound);
        if (dto.Items is not null)
        {
            foreach (var item in dto.Items)
            {
                var gross = item.Quantity * item.UnitPrice;
                if (item.Quantity <= 0 || item.UnitPrice < 0 || item.DiscountAmount < 0 || item.DiscountAmount > gross)
                    return ServiceResult<PurchaseInvoiceResponseDto>.Failure("بيانات كمية أو سعر أو خصم أحد البنود غير صحيحة", ErrorCodes.ValidationError);
            }
        }

        if (dto.DiscountAmount < 0 || dto.TaxAmount < 0)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("الخصم والضريبة لا يقبلان قيماً سالبة", ErrorCodes.ValidationError);
        var oldTotal = invoice.TotalAmount;
        var oldPaid = invoice.PaidAmount;
        var newSubTotal = dto.Items?.Sum(i => Math.Max(0, (i.Quantity * i.UnitPrice) - i.DiscountAmount)) ?? invoice.SubTotal;
        var newTotal = newSubTotal - dto.DiscountAmount + dto.TaxAmount;
        if (newTotal < 0)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("خصم الفاتورة يتجاوز قيمة البنود والضريبة", ErrorCodes.ValidationError);
        var newPaid = dto.Status == InvoiceStatus.Paid ? newTotal : dto.PaidAmount;
        if (newPaid < 0 || newPaid > newTotal)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المبلغ المدفوع لا يمكن أن يتجاوز إجمالي الفاتورة", ErrorCodes.ValidationError);
        var paidDelta = newPaid - oldPaid;
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        if (warehouse is null)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        if (dto.Items is not null && await ResolveStorageBarcodesAsync(dto.Items, warehouse, ct) is { } barcodeError)
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure(barcodeError, ErrorCodes.ValidationError);
        Safe? paymentSafe = null;
        if (paidDelta != 0)
        {
            if (invoice.PaymentSafeId.HasValue && dto.PaymentSafeId.HasValue && invoice.PaymentSafeId != dto.PaymentSafeId)
                return ServiceResult<PurchaseInvoiceResponseDto>.Failure("لا يمكن تغيير خزينة فاتورة تحتوي على دفعات", ErrorCodes.ValidationError);
            var safeResult = await _financialPosting.ResolveSafeAsync(invoice.BranchId, invoice.PaymentSafeId ?? dto.PaymentSafeId, Math.Abs(paidDelta), outbound: paidDelta > 0, ct);
            if (!safeResult.IsSuccess)
                return ServiceResult<PurchaseInvoiceResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            paymentSafe = safeResult.Data;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // مزامنة بنود الفاتورة وأرصدة المخزون ضمن المعاملة نفسها.
            if (dto.Items != null)
            {
                // Replace the old receipt atomically; editing a row updates stock as well as totals.
                var oldItems = invoice.Items?.ToList() ?? new List<PurchaseInvoiceItem>();
                await AdjustInvoiceItemsStockAsync(oldItems, invoice.WarehouseId, isIncrement: false, ct);
                var oldItemIds = oldItems.Select(i => i.Id).ToList();
                var oldBreakdowns = await _unitOfWork.PurchaseInvoices.GetBreakdownsByItemIdsForUpdateAsync(oldItemIds, ct);
                foreach (var breakdown in oldBreakdowns)
                    _unitOfWork.PurchaseInvoices.HardDeleteBreakdown(breakdown);
                var trackedItems = await _unitOfWork.PurchaseInvoices.GetItemsByIdsForUpdateAsync(oldItemIds, ct);
                foreach (var oldItem in trackedItems)
                    _unitOfWork.PurchaseInvoices.HardDeleteItem(oldItem);
                var replacementItems = dto.Items.Select(item => new PurchaseInvoiceItem { PurchaseInvoiceId = invoice.Id, ProductId = item.ProductId, ProductBarCodeId = item.ProductBarCodeId, Quantity = item.Quantity, UnitPrice = item.UnitPrice, DiscountAmount = item.DiscountAmount, LineTotal = item.Quantity * item.UnitPrice - item.DiscountAmount, Breakdowns = item.Breakdowns?.Select(b => new PurchaseInvoiceItemBreakdown { ProductBarCodeId = b.ProductBarCodeId, PackageQuantity = b.PackageQuantity, UnitsPerPackage = b.UnitsPerPackage, Quantity = b.Quantity, UnitPrice = b.UnitPrice > 0 ? b.UnitPrice : item.UnitPrice }).ToList() ?? new List<PurchaseInvoiceItemBreakdown>() }).ToList();
                foreach (var item in replacementItems)
                    await _unitOfWork.PurchaseInvoices.AddItemAsync(item, ct);
                await AdjustInvoiceItemsStockAsync(replacementItems, invoice.WarehouseId, isIncrement: true, ct);
                await UpdateProductsCostAsync(replacementItems, ct, oldItems);
                if (_unitOfWork.ChangeTrackerEntries().Any(e => e.Entity is StorgeStock storage && storage.Quantity < 0 || e.Entity is ShowroomStock showroom && showroom.Quantity < 0))
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return ServiceResult<PurchaseInvoiceResponseDto>.Failure("تعديل الكميات سيجعل رصيد المخزون سالباً؛ تحقق من الكميات المصروفة", ErrorCodes.ValidationError);
                }
            }

            invoice.InvoiceNumber = dto.InvoiceNumber;
            // تحديث تاريخ الفاتورة
            invoice.InvoiceDate = dto.InvoiceDate;
            // تحديث المورد
            invoice.SupplierId = dto.SupplierId;
            // تحديث الفرع
            invoice.BranchId = dto.BranchId;
            // تحديث المستودع
            invoice.WarehouseId = dto.WarehouseId;
            // تحديث طريقة الدفع
            invoice.PaymentMethod = dto.PaymentMethod;
            // تحديث مبلغ الخصم
            invoice.DiscountAmount = dto.DiscountAmount;
            // تحديث مبلغ الضريبة
            invoice.TaxAmount = dto.TaxAmount;
            // تحديث الملاحظات
            invoice.Notes = dto.Notes;
            // إعادة احتساب الإجمالي الفرعي
            invoice.SubTotal = newSubTotal;
            // احتساب الإجمالي النهائي
            invoice.TotalAmount = newTotal;
            // فحص ما إذا كان الطلب يتضمن إغلاق الفاتورة (التحويل إلى Paid)
            invoice.PaidAmount = newPaid;
            invoice.RemainingAmount = newTotal - newPaid;
            invoice.Status = newPaid == newTotal ? InvoiceStatus.Paid : newPaid > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Pending;
            invoice.PaymentSafeId = paymentSafe?.Id ?? invoice.PaymentSafeId;
            // تفريغ كائنات الربط المنفصلة لتجنب أي تعارض في تتبع الكيانات
            invoice.Items = null!;
            invoice.Supplier = null!;
            invoice.Branch = null!;
            invoice.Warehouse = null!;
            invoice.PurchaseOrder = null;
            // تحديث الفاتورة في المستودع
            _unitOfWork.PurchaseInvoices.Update(invoice);
            await _financialPosting.PostSupplierBalanceAsync(supplier, newTotal - oldTotal, SupplierTransactionType.PurchaseInvoice, invoice.BranchId, FinancialDocumentType.PurchaseInvoice, invoice.Id, invoice.InvoiceDate, invoice.PaymentMethod, $"تعديل فاتورة مشتريات رقم {invoice.InvoiceNumber}", ct);
            await _financialPosting.PostSupplierBalanceAsync(supplier, -paidDelta, SupplierTransactionType.Payment, invoice.BranchId, FinancialDocumentType.PurchaseInvoice, invoice.Id, invoice.InvoiceDate, invoice.PaymentMethod, $"تعديل دفعة فاتورة مشتريات رقم {invoice.InvoiceNumber}", ct);
            if (paymentSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(paymentSafe, -paidDelta, FinancialDocumentType.PurchaseInvoice, invoice.Id, invoice.InvoiceDate, $"تعديل سداد فاتورة مشتريات رقم {invoice.InvoiceNumber}", ct);
            }

            // حفظ التغييرات في قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // إعادة جلب الفاتورة المحدثة مع تفاصيلها عبر الإسقاط المباشر
        var responseDto = MapInvoiceResponse(await _unitOfWork.PurchaseInvoices.GetReadResultAsync(id, ct)) ?? _mapper.Map<PurchaseInvoiceResponseDto>(invoice);
        // إرجاع النتيجة الناجحة
        return ServiceResult<PurchaseInvoiceResponseDto>.Success(responseDto);
    }

    /// <summary>
    /// تطبيق أو عكس تأثير بنود الفاتورة على المخزون (في المستودع أو الصالة) مع دعم تفصيلات النكهات والباركودات.
    /// تُجلب أرصدة المستودع المعنية دفعة واحدة ثم تُعدَّل في الذاكرة (معالجة نمط N+1).
    /// </summary>
    /// <param name = "items">قائمة بنود الفاتورة المعنية بالتعديل</param>
    /// <param name = "warehouseId">معرف المستودع أو الصالة المستهدفة</param>
    /// <param name = "isIncrement">تحديد ما إذا كانت الحركة إضافة للمخزون (true) أو خصم (false)</param>
    /// <param name = "ct">رمز إلغاء العملية</param>
    private async Task AdjustInvoiceItemsStockAsync(IEnumerable<PurchaseInvoiceItem> items, Guid warehouseId, bool isIncrement, CancellationToken ct)
    {
        // استعلام المستودع للتعرف على نوعه
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(warehouseId, ct);
        // في حال عدم وجود المستودع يتم إنهاء الدالة
        if (warehouse == null)
            return;
        // تحويل البنود إلى قائمة محلية
        var itemsList = items.ToList();
        // تحديد إشارة الحركة المخزنية (+1 للإضافة، -1 للخصم)
        int sign = isIncrement ? 1 : -1;
        // معالجة المخزون في المخزن الرئيسي (مستودع تخزين Storge - بنية نكهات وباركودات)
        if (warehouse.Type == WarehouseType.Storge)
        {
            // جمع كل الأصناف التي تحتاج استنباط الباركود الافتراضي لها
            var fallbackProductIds = itemsList.Where(i => (i.Breakdowns == null || !i.Breakdowns.Any()) && (!i.ProductBarCodeId.HasValue || i.ProductBarCodeId.Value == Guid.Empty)).Select(i => i.ProductId).Distinct().ToList();
            // جلب الباركودات الافتراضية دفعة واحدة لتفادي استعلامات N+1
            var defaultBarcodeByProduct = fallbackProductIds.Count > 0 ? (await _unitOfWork.Products.GetBarcodesByProductIdsAsync(fallbackProductIds, ct)).GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.First()) : new Dictionary<Guid, ProductBarCode>();
            // تجميع كافة معرفات الباركود المشاركة في هذه الدفعة
            var allBarcodeIds = new HashSet<Guid>();
            foreach (var item in itemsList)
            {
                // إذا كان للبند تفصيلات نكهات متعددة
                if (item.Breakdowns != null && item.Breakdowns.Any())
                {
                    // إضافة باركود كل نكهة
                    foreach (var bd in item.Breakdowns)
                    {
                        allBarcodeIds.Add(bd.ProductBarCodeId);
                    }
                }
                // إذا كان البند يرتبط مباشرة بباركود محدد
                else if (item.ProductBarCodeId.HasValue && item.ProductBarCodeId.Value != Guid.Empty)
                {
                    allBarcodeIds.Add(item.ProductBarCodeId.Value);
                }
                // إذا تم استنباط الباركود الافتراضي للصنف
                else if (defaultBarcodeByProduct.TryGetValue(item.ProductId, out var bc))
                {
                    allBarcodeIds.Add(bc.Id);
                }
            }

            // تحويل المعرفات إلى قائمة
            var barcodeIdList = allBarcodeIds.ToList();
            // استعلام أرصدة المخزن المتتبعة للباركودات المعنية دفعة واحدة
            var stocksByBarcode = barcodeIdList.Count > 0 ? (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeIdList, ct)).ToDictionary(s => s.ProductBarcodeId) : new Dictionary<Guid, StorgeStock>();
            // المرور على بنود الفاتورة وتحديث أرصدة المخزن
            foreach (var item in itemsList)
            {
                // في حال وجود تفصيلات للنكهات
                if (item.Breakdowns != null && item.Breakdowns.Any())
                {
                    // معالجة كل نكهة على حدة
                    foreach (var bd in item.Breakdowns)
                    {
                        // فحص وجود رصيد مسبق للنكهة في المخزن
                        if (stocksByBarcode.TryGetValue(bd.ProductBarCodeId, out var stock))
                        {
                            // تعديل الرصيد المخزني بالزيادة أو النقصان
                            stock.Quantity = checked(stock.Quantity + sign * StockQuantity.ToInt32(bd.Quantity));
                        }
                        // في حال كانت العملية إضافة رصيد ولم يكن الرصيد موجوداً مسبقاً
                        else if (isIncrement)
                        {
                            // إنشاء سجل رصيد جديد للنكهة
                            var newStock = new StorgeStock
                            {
                                WarehouseId = warehouse.Id,
                                ProductBarcodeId = bd.ProductBarCodeId,
                                Quantity = StockQuantity.ToInt32(bd.Quantity),
                                MinStockLevel = 0
                            };
                            // إضافة السجل للمستودع
                            await _unitOfWork.Stock.AddStorgeStockAsync(newStock, ct);
                            // حفظ السجل في القاموس المحلي
                            stocksByBarcode[bd.ProductBarCodeId] = newStock;
                        }
                    }
                }
                // في حال كان البند بدون تفصيلات نكهات
                else
                {
                    // استخراج معرف الباركود للبند
                    var barcodeId = item.ProductBarCodeId;
                    // إذا لم يكن محدداً يتم محاولة استرجاع الباركود الافتراضي
                    if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty)
                    {
                        if (defaultBarcodeByProduct.TryGetValue(item.ProductId, out var bc))
                        {
                            barcodeId = bc.Id;
                        }
                    }

                    // تخطي البند إن تعذر تحديد باركود له
                    if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty)
                        continue;
                    // فحص وجود رصيد مسجل مسبقاً لهذا الباركود
                    if (stocksByBarcode.TryGetValue(barcodeId.Value, out var stock))
                    {
                        // تعديل الرصيد
                        stock.Quantity = checked(stock.Quantity + sign * StockQuantity.ToInt32(item.Quantity));
                    }
                    // إذا كانت حركة إضافة ورصيد جديد
                    else if (isIncrement)
                    {
                        // إنشاء سجل رصيد مخزني جديد
                        var newStock = new StorgeStock
                        {
                            WarehouseId = warehouse.Id,
                            ProductBarcodeId = barcodeId.Value,
                            Quantity = StockQuantity.ToInt32(item.Quantity),
                            MinStockLevel = 0
                        };
                        // إضافة السجل الجديد
                        await _unitOfWork.Stock.AddStorgeStockAsync(newStock, ct);
                        // تخزينه في القاموس
                        stocksByBarcode[barcodeId.Value] = newStock;
                    }
                }
            }
        }
        // معالجة المخزون في صالة العرض (مستودع Showroom - على مستوى الأصناف Products)
        else if (warehouse.Type == WarehouseType.Show)
        {
            // استخراج معرفات الأصناف الفريدة
            var productIds = itemsList.Select(i => i.ProductId).Distinct().ToList();
            // استعلام أرصدة صالة العرض المتتبعة دفعة واحدة للأصناف المشاركة
            var stocksByProduct = productIds.Count > 0 ? (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productIds, ct)).ToDictionary(s => s.ProductId) : new Dictionary<Guid, ShowroomStock>();
            // المرور على بنود الفاتورة وتحديث أرصدة صالة العرض
            foreach (var item in itemsList)
            {
                // إذا كان للصنف رصيد مسجل مسبقاً
                if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                {
                    // تعديل كمية الرصيد
                    stock.Quantity = checked(stock.Quantity + sign * StockQuantity.ToInt32(item.Quantity));
                }
                // إذا كانت حركة إضافة ورصيد جديد في الصالة
                else if (isIncrement)
                {
                    // إنشاء سجل رصيد جديد في صالة العرض
                    var newStock = new ShowroomStock
                    {
                        WarehouseId = warehouse.Id,
                        ProductId = item.ProductId,
                        Quantity = StockQuantity.ToInt32(item.Quantity),
                        MinStockLevel = 0
                    };
                    // إضافة سجل رصيد الصالة
                    await _unitOfWork.Stock.AddShowroomStockAsync(newStock, ct);
                    // حفظ السجل في القاموس
                    stocksByProduct[item.ProductId] = newStock;
                }
            }
        }
    }

    /// <summary>
    /// تحديث آخر سعر شراء (CostPrice) ومتوسط التكلفة (AveragePrice) للأصناف — جلب المنتجات المعنية دفعة واحدة.
    /// </summary>
    /// <param name = "items">بنود الفاتورة التي تحتوي على أسعار الشراء الجديدة</param>
    /// <param name = "ct">رمز إلغاء العملية</param>
    private async Task UpdateProductsCostAsync(IEnumerable<PurchaseInvoiceItem> items, CancellationToken ct,
        IReadOnlyList<PurchaseInvoiceItem>? replacedItems = null)
    {
        var incoming = items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => (Quantity: g.Sum(i => i.Quantity), Value: g.Sum(i => i.LineTotal)));
        var removed = (replacedItems ?? []).GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => (Quantity: g.Sum(i => i.Quantity), Value: g.Sum(i => i.LineTotal)));
        var productIds = incoming.Keys.Union(removed.Keys).ToList();
        var products = (await _unitOfWork.Products.GetByIdsForUpdateAsync(productIds, ct)).ToDictionary(p => p.Id);
        // الكميات المحفوظة تسبق تغييرات المخزون المرحلية في هذه المعاملة.
        var quantities = await _unitOfWork.Stock.GetQuantitiesByProductAsync(productIds, ct);
        foreach (var productId in productIds)
        {
            var product = products[productId];
            var receipt = incoming.GetValueOrDefault(productId);
            var reversal = removed.GetValueOrDefault(productId);
            var currentQuantity = Math.Max(0, quantities.GetValueOrDefault(productId));
            var average = product.AveragePrice > 0 ? product.AveragePrice : product.CostPrice;
            var remainingQuantity = Math.Max(0, currentQuantity - reversal.Quantity);
            var remainingValue = remainingQuantity > 0 ? Math.Max(0, currentQuantity * average - reversal.Value) : 0;
            var newQuantity = remainingQuantity + receipt.Quantity;
            product.AveragePrice = newQuantity > 0 ? decimal.Round((remainingValue + receipt.Value) / newQuantity, 4) : 0;
            product.CostPrice = receipt.Quantity > 0 ? decimal.Round(receipt.Value / receipt.Quantity, 4) : product.AveragePrice;
        }
    }
    /// <inheritdoc/>
    public async Task<ServiceResult> CancelAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام فاتورة المشتريات بالمعرف مع تفاصيل البنود
        var invoice = await _unitOfWork.PurchaseInvoices.GetDetailsForUpdateAsync(id, ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ بعدم وجود الفاتورة
            return ServiceResult.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // منع إلغاء فاتورة ملغاة مسبقاً
        if (invoice.Status is InvoiceStatus.Cancelled or InvoiceStatus.Voided)
        {
            // إرجاع خطأ بكون الفاتورة ملغاة بالفعل
            return ServiceResult.Failure("الفاتورة ملغاة بالفعل", ErrorCodes.PurchaseOrderInvalidStatus);
        }

        if (await _unitOfWork.PurchaseReturns.HasInvoiceReferencesAsync(id, ct))
            return ServiceResult.Failure("لا يمكن إلغاء فاتورة لها مرتجعات مشتريات؛ اعكس المرتجعات أولاً", ErrorCodes.ValidationError);

        var supplier = await _unitOfWork.Suppliers.GetByIdForUpdateAsync(invoice.SupplierId, ct);
        if (supplier is null)
            return ServiceResult.Failure("المورد المرتبط بالفاتورة غير موجود", ErrorCodes.SupplierNotFound);
        var wasPosted = invoice.Status != InvoiceStatus.Draft;
        Safe? paymentSafe = null;
        if (wasPosted && invoice.PaidAmount > 0)
        {
            if (!invoice.PaymentSafeId.HasValue)
                return ServiceResult.Failure("لا يمكن إلغاء فاتورة قديمة مدفوعة قبل تحديد خزينة التسوية لها", ErrorCodes.ValidationError);
            var safeResult = await _financialPosting.ResolveSafeAsync(invoice.BranchId, invoice.PaymentSafeId, invoice.PaidAmount, outbound: false, ct);
            if (!safeResult.IsSuccess)
                return ServiceResult.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            paymentSafe = safeResult.Data;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // عكس تأثير المخزون بخصم الكميات التي كانت قد أضيفت بموجب الفاتورة
            if (wasPosted && invoice.Items != null && invoice.Items.Any())
            {
                // خصم بنود الفاتورة من المخزن المعني
                await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: false, ct);
                if (_unitOfWork.ChangeTrackerEntries().Any(e => e.Entity is StorgeStock storage && storage.Quantity < 0 || e.Entity is ShowroomStock showroom && showroom.Quantity < 0))
                {
                    await _unitOfWork.RollbackTransactionAsync(ct);
                    return ServiceResult.Failure("لا يمكن إلغاء الفاتورة بعد صرف كمياتها من المخزون", ErrorCodes.InsufficientStock);
                }
            }

            if (wasPosted)
                await UpdateProductsCostAsync([], ct, invoice.Items?.ToList() ?? []);

            // تحديث حالة الفاتورة إلى ملغاة
            invoice.Status = InvoiceStatus.Cancelled;
            // تعليم الفاتورة كمحدثة
            _unitOfWork.PurchaseInvoices.Update(invoice);
            if (wasPosted)
            {
                await _financialPosting.PostSupplierBalanceAsync(supplier, invoice.PaidAmount, SupplierTransactionType.Reversal, invoice.BranchId, FinancialDocumentType.PurchaseInvoice, invoice.Id, DateTime.UtcNow, invoice.PaymentMethod, $"عكس دفعة فاتورة مشتريات ملغاة رقم {invoice.InvoiceNumber}", ct);
                await _financialPosting.PostSupplierBalanceAsync(supplier, -invoice.TotalAmount, SupplierTransactionType.Reversal, invoice.BranchId, FinancialDocumentType.PurchaseInvoice, invoice.Id, DateTime.UtcNow, invoice.PaymentMethod, $"عكس فاتورة مشتريات ملغاة رقم {invoice.InvoiceNumber}", ct);
            }

            if (paymentSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(paymentSafe, invoice.PaidAmount, FinancialDocumentType.PurchaseInvoice, invoice.Id, DateTime.UtcNow, $"استرداد سداد فاتورة مشتريات ملغاة رقم {invoice.InvoiceNumber}", ct);
            }

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

    /// <inheritdoc/>
    private async Task<string?> ResolveStorageBarcodesAsync(IReadOnlyCollection<CreatePurchaseInvoiceItemDto> items, Warehouse warehouse, CancellationToken ct)
    {
        if (!Enum.IsDefined(warehouse.Type)) return "نوع المستودع غير صحيح";
        if (warehouse.Type != WarehouseType.Storge) return null;
        var missing = items.Where(i => !i.ProductBarCodeId.HasValue && i.Breakdowns is not { Count: > 0 }).ToList();
        var ids = missing.Select(i => i.ProductId).Distinct().ToList();
        var defaults = (await _unitOfWork.Products.GetBarcodesByProductIdsAsync(ids, ct))
            .GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.OrderBy(b => b.Id).First().Id);
        foreach (var item in missing)
        {
            if (!defaults.TryGetValue(item.ProductId, out var barcodeId))
                return "يجب تسجيل باركود للصنف قبل استلام كمياته في مخزن التخزين";
            item.ProductBarCodeId = barcodeId;
        }
        return null;
    }

    private static bool HasMoneyPrecision(decimal value) => decimal.Round(value, 4) == value;
    private async Task<string?> ValidateItemsAsync(IReadOnlyCollection<CreatePurchaseInvoiceItemDto> items, CancellationToken ct)
    {
        foreach (var item in items)
        {
            if (!StockQuantity.IsValid(item.Quantity))
                return "الكمية يجب أن تكون عدداً صحيحاً موجباً من أصغر وحدة (غرام أو قطعة)، ضمن الحد المسموح";
            if (!HasMoneyPrecision(item.UnitPrice) || !HasMoneyPrecision(item.DiscountAmount))
                return "الأسعار والخصومات تدعم أربع منازل عشرية كحد أقصى";
            if (!await _unitOfWork.Products.ExistsByIdAsync(item.ProductId, ct))
                return "الصنف المحدد غير موجود";
            if (item.ProductBarCodeId.HasValue && !await _unitOfWork.Products.BarcodeBelongsToProductAsync(item.ProductBarCodeId.Value, item.ProductId, ct))
                return "الباركود لا ينتمي إلى الصنف المحدد";
            if (item.Breakdowns is not { Count: > 0 })
                continue;
            decimal total = 0;
            foreach (var breakdown in item.Breakdowns)
            {
                if (breakdown.PackageQuantity <= 0 || breakdown.PackageQuantity > int.MaxValue || decimal.Round(breakdown.PackageQuantity, 4) != breakdown.PackageQuantity || breakdown.UnitsPerPackage <= 0)
                    return "عدد العبوات ومعامل التحويل إلى أصغر وحدة غير صحيحين";
                var baseQuantity = breakdown.PackageQuantity * breakdown.UnitsPerPackage;
                if (!StockQuantity.IsValid(baseQuantity) || (breakdown.Quantity != 0 && breakdown.Quantity != baseQuantity))
                    return "كمية التفصيل يجب أن تساوي عدد العبوات × معامل التحويل، وأن تمثل وحدات صغرى كاملة";
                if (breakdown.UnitPrice < 0 || !HasMoneyPrecision(breakdown.UnitPrice))
                    return "سعر أصغر وحدة غير صحيح أو يتجاوز أربع منازل عشرية";
                if (!await _unitOfWork.Products.BarcodeBelongsToProductAsync(breakdown.ProductBarCodeId, item.ProductId, ct))
                    return "باركود التفصيل لا ينتمي إلى الصنف المحدد";
                breakdown.Quantity = baseQuantity;
                total += baseQuantity;
            }

            if (total != item.Quantity)
                return "مجموع كميات التفصيل بأصغر وحدة يجب أن يساوي كمية البند";
        }

        return null;
    }

    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع لتفادي تضارب النسخ المكررة عند تكرار الصنف في البنود أثناء SoftDelete
        var invoice = await _unitOfWork.PurchaseInvoices.GetDetailsForUpdateAsync(id, ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ بعدم العثور على الفاتورة
            return ServiceResult.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // منع حذف فاتورة لها مرتجعات مرتبطة — يلزم حذف المرتجعات أولاً
        bool hasReturns = await _unitOfWork.PurchaseReturns.HasInvoiceReferencesAsync(id, ct);
        // في حال وجود مرتجعات مرتبطة
        if (hasReturns)
        {
            // إرجاع خطأ منع الحذف
            return ServiceResult.Failure("لا يمكن حذف فاتورة مشتريات لها مرتجعات مرتبطة — احذف المرتجعات أولاً", ErrorCodes.ValidationError);
        }

        if (invoice.Status != InvoiceStatus.Draft)
            return ServiceResult.Failure("لا يجوز حذف فاتورة مرحلة حفاظاً على سجل التدقيق؛ استخدم الإلغاء بدلاً من الحذف", ErrorCodes.ValidationError);
        // عكس المخزون في حال لم تكن الفاتورة ملغاة مسبقاً
        _unitOfWork.PurchaseInvoices.SoftDelete(invoice);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
