using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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
/// تنفيذ خدمة إدارة فواتير المبيعات وحركات البيع وخصم المخزون والتحقق من كفاية الأرصدة.
/// </summary>
public class SalesInvoiceService : ISalesInvoiceService
{
    // وحدة العمل للتعامل مع مستودعات الفواتير والعملاء والفروع والمخزون
    private readonly IUnitOfWork _unitOfWork;
    // محول النماذج للتحويل بين الكيانات وDTOs
    private readonly IMapper _mapper;
    private readonly IFinancialPostingService _financialPosting;
    private readonly IPosCashService _posCashService;
    private readonly ICurrentPermissionService? _permissions;
    /// <summary>
    /// تهيئة خدمة فواتير المبيعات مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name = "mapper">محول الكيانات</param>
    public SalesInvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IFinancialPostingService financialPosting, IPosCashService posCashService, ICurrentPermissionService? permissions = null)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع المحول
        _mapper = mapper;
        _financialPosting = financialPosting;
        _posCashService = posCashService;
        _permissions = permissions;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام فاتورة المبيعات بالمعرف مع تفاصيل البنود والعميل والفرع
        var invoice = await _unitOfWork.SalesInvoices.GetDetailsAsync(id, ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ عدم وجود الفاتورة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // تحويل الكيان إلى كائن استجابة DTO
        var dto = await MapWithIssuerAsync(invoice, ct);
        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        // استعلام الفاتورة بواسطة رقم الفاتورة
        var invoice = await _unitOfWork.SalesInvoices.GetByNumberAsync(invoiceNumber, ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ عدم وجود الفاتورة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = await MapWithIssuerAsync(invoice, ct);
        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, Guid? customerId = null, InvoiceStatus? status = null, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام القوائم الخفيفة لفواتير المبيعات
        // استرجاع قائمة الفواتير المطابقة
        var invoices = await _unitOfWork.SalesInvoices.ListAsync(new SalesInvoiceQuery { BranchId = branchId, WarehouseId = warehouseId, CustomerId = customerId, Status = status, PaymentMethod = paymentMethod, FromDate = fromDate, ToDate = toDate, Search = search }, ct);
        // تحويل الكيانات إلى ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<SalesInvoiceSummaryDto>>(invoices);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<SalesInvoiceSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, Guid? customerId = null, InvoiceStatus? status = null, PaymentMethod? paymentMethod = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة القوائم الخفيفة
        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var(items, totalCount) = await _unitOfWork.SalesInvoices.SearchAsync(new SalesInvoiceQuery { BranchId = branchId, WarehouseId = warehouseId, CustomerId = customerId, Status = status, PaymentMethod = paymentMethod, FromDate = fromDate, ToDate = toDate, Search = search, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<SalesInvoiceSummaryDto>>(items);
        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<SalesInvoiceSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<SalesInvoiceSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> CreatePosAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        if (_permissions?.HasPermission(Permissions.Pos.CreateSale) != true)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا تملك صلاحية إتمام بيع نقطة البيع", ErrorCodes.Forbidden);
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SalesInvoiceResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        if ((dto.DiscountAmount > 0 || dto.Items.Any(i => i.DiscountAmount > 0)) &&
            !_permissions.HasPermission(Permissions.Pos.ApplyDiscount))
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا تملك صلاحية منح خصم في نقطة البيع", ErrorCodes.Forbidden);

        var invoiceNumber = PosSaleRequest.InvoiceNumber(dto.InvoiceNumber);
        var requestHash = PosSaleRequest.Fingerprint(dto);
        var existing = await _unitOfWork.SalesInvoices.GetByNumberAsync(invoiceNumber, ct);
        if (existing is not null)
            return PosReplay(existing, requestHash);

        var products = (await _unitOfWork.Products.GetByIdsAsync(dto.Items.Select(i => i.ProductId).Distinct().ToList(), ct)).ToDictionary(p => p.Id);
        if (dto.Items.Any(i => !products.TryGetValue(i.ProductId, out var product) || i.UnitPrice != product.SalePrice))
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تغيرت أسعار الأصناف أو حالتها؛ أعد تحميلها قبل إتمام البيع", ErrorCodes.ValidationError);
        if (dto.PaymentMethod != PaymentMethod.Cash && dto.PaidAmount > 0)
        {
            var account = dto.PaymentSafeId.HasValue ? await _unitOfWork.Accounting.GetSafeByIdAsync(dto.PaymentSafeId.Value, ct) : null;
            if (account is null || account.Type != SafeType.Bank || !account.IsActive || account.BranchId != dto.BranchId)
                return ServiceResult<SalesInvoiceResponseDto>.Failure("اختر حساب تحصيل مصرفي نشطاً تابعاً لفرع نقطة البيع", ErrorCodes.ValidationError);
        }
        try
        {
            var result = await CreateCoreAsync(PosSaleRequest.ForPosting(dto, invoiceNumber), requestHash, ct);
            if (!result.IsSuccess)
            {
                existing = await _unitOfWork.SalesInvoices.GetByNumberAsync(invoiceNumber, ct);
                if (existing is not null) return PosReplay(existing, requestHash);
            }
            return result;
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // A concurrent identical request may have won the unique-index race.
            // CreateCoreAsync has rolled back and cleared its failed tracked writes.
            existing = await _unitOfWork.SalesInvoices.GetByNumberAsync(invoiceNumber, ct);
            if (existing is not null) return PosReplay(existing, requestHash);
            throw;
        }
    }

    private ServiceResult<SalesInvoiceResponseDto> PosReplay(SalesInvoice existing, string requestHash) =>
        existing.PosRequestHash == requestHash
            ? ServiceResult<SalesInvoiceResponseDto>.Success(_mapper.Map<SalesInvoiceResponseDto>(existing))
            : ServiceResult<SalesInvoiceResponseDto>.Failure("رقم عملية نقطة البيع مستخدم لطلب مختلف؛ لا تعِد استخدامه بعد تغيير البيانات", ErrorCodes.SalesInvoiceNumberExists);

    public Task<ServiceResult<SalesInvoiceResponseDto>> CreateAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default) =>
        CreateCoreAsync(dto, null, ct);

    private async Task<ServiceResult<SalesInvoiceResponseDto>> CreateCoreAsync(CreateSalesInvoiceDto dto, string? posRequestHash, CancellationToken ct)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SalesInvoiceResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        if (dto.Status == InvoiceStatus.Draft)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("إنشاء الفاتورة هنا يرحّل المخزون والحسابات؛ لا يمكن إرسالها كمسودة", ErrorCodes.ValidationError);

        // التحقق من وجود الفرع المحدد
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        if (!branchExists)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<SalesInvoiceResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // التحقق من وجود المستودع أو صالة العرض المحددة
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        if (!warehouse.IsActive || warehouse.BranchId != dto.BranchId || !Enum.IsDefined(warehouse.Type))
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب اختيار مستودع نشط تابع لفرع الفاتورة", ErrorCodes.ValidationError);

        Customer? customer = null;
        // التحقق من وجود العميل إن تم تحديده
        if (dto.CustomerId.HasValue)
        {
            customer = await _unitOfWork.Customers.GetByIdForUpdateAsync(dto.CustomerId.Value, ct);
            if (customer is null)
            {
                // إرجاع خطأ عدم وجود العميل
                return ServiceResult<SalesInvoiceResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }

            if (!customer.IsActive)
                return ServiceResult<SalesInvoiceResponseDto>.Failure("العميل المحدد معطل حالياً", ErrorCodes.ValidationError);
        }

        // التحقق من عدم تكرار رقم فاتورة المبيعات
        bool numExists = await _unitOfWork.SalesInvoices.NumberExistsAsync(dto.InvoiceNumber, ct);
        if (numExists)
        {
            // إرجاع خطأ تكرار رقم الفاتورة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("رقم الفاتورة مستخدم بالفعل", ErrorCodes.SalesInvoiceNumberExists);
        }

        // التحقق من احتواء الفاتورة على بند واحد على الأقل
        if (dto.Items == null || !dto.Items.Any())
        {
            // إرجاع خطأ تحقق لغياب البنود
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب إضافة بند واحد على الأقل للفاتورة", ErrorCodes.ValidationError);
        }

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var productsById = (await _unitOfWork.Products.GetByIdsAsync(productIds, ct)).ToDictionary(p => p.Id);
        if (productsById.Count != productIds.Count)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
        foreach (var item in dto.Items)
        {
            var gross = item.Quantity * item.UnitPrice;
            if (item.Quantity <= 0 || item.UnitPrice < 0 || item.DiscountAmount < 0 || item.DiscountAmount > gross)
                return ServiceResult<SalesInvoiceResponseDto>.Failure("بيانات كمية أو سعر أو خصم أحد البنود غير صحيحة", ErrorCodes.ValidationError);
        }

        var grossSubTotal = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
        var lineDiscounts = dto.Items.Sum(i => i.DiscountAmount);
        var netBeforeInvoiceDiscount = grossSubTotal - lineDiscounts;
        if (dto.DiscountAmount < 0 || dto.DiscountAmount > netBeforeInvoiceDiscount || dto.TaxAmount < 0)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("خصم الفاتورة يتجاوز قيمة البنود", ErrorCodes.ValidationError);
        var calculatedTotal = netBeforeInvoiceDiscount - dto.DiscountAmount + dto.TaxAmount;
        if (dto.PaidAmount < 0 || dto.PaidAmount > calculatedTotal)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المبلغ المدفوع لا يمكن أن يتجاوز إجمالي الفاتورة", ErrorCodes.ValidationError);
        var remainingAmount = calculatedTotal - dto.PaidAmount;
        if (remainingAmount > 0 && customer is null)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب تحديد العميل عند وجود مبلغ آجل", ErrorCodes.ValidationError);
        if (customer is not null && customer.CreditLimit > 0 && customer.CurrentBalance + remainingAmount > customer.CreditLimit)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("قيمة الفاتورة تتجاوز سقف ائتمان العميل", ErrorCodes.ValidationError);
        CashSession? cashSession = null;
        Guid? effectiveSafeId = dto.PaymentSafeId;
        if (dto.CashSessionId.HasValue)
        {
            if (dto.PaymentMethod != PaymentMethod.Cash)
                return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يجوز ربط وردية درج نقود بفاتورة غير نقدية", ErrorCodes.ValidationError);
            var sessionResult = await _posCashService.ResolveOpenSessionForPostingAsync(dto.CashSessionId.Value, dto.BranchId, ct);
            if (!sessionResult.IsSuccess)
                return ServiceResult<SalesInvoiceResponseDto>.Failure(sessionResult.ErrorMessage!, sessionResult.ErrorCode);
            cashSession = sessionResult.Data;
            effectiveSafeId = cashSession!.CashDrawerId;
        }
        else if (dto.PaymentMethod == PaymentMethod.Cash && dto.PaidAmount > 0 && dto.InvoiceNumber.StartsWith("POS-", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب فتح وردية درج نقود وربطها قبل تنفيذ بيع نقدي من نقطة البيع", ErrorCodes.ValidationError);
        }

        var safeResult = await _financialPosting.ResolveSafeAsync(dto.BranchId, effectiveSafeId, dto.PaidAmount, outbound: false, ct);
        if (!safeResult.IsSuccess)
            return ServiceResult<SalesInvoiceResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
        Safe? paymentSafe = safeResult.Data;
        // حل الباركود الافتراضي للأصناف إن لم يتم تحديده دفعة واحدة
        var productIdsNeedingDefaultBarcode = dto.Items.Where(i => !i.ProductBarCodeId.HasValue).Select(i => i.ProductId).Distinct().ToList();
        // استخراج الباركودات الافتراضية
        if (productIdsNeedingDefaultBarcode.Count > 0)
        {
            // جلب باركودات المنتجات
            var defaultBarcodes = await _unitOfWork.Products.GetBarcodesByProductIdsAsync(productIdsNeedingDefaultBarcode, ct);
            // تجميع الباركود الأول لكل منتج
            var defaultBarcodeByProduct = defaultBarcodes.GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.First());
            // تعيين الباركود لكل بند ناقص
            foreach (var item in dto.Items)
            {
                // إذا لم يتم تحديد باركود ووجد باركود افتراضي
                if (!item.ProductBarCodeId.HasValue && defaultBarcodeByProduct.TryGetValue(item.ProductId, out var defaultBarcode))
                {
                    // إسناد معرف الباركود
                    item.ProductBarCodeId = defaultBarcode.Id;
                }
            }
        }

        // جلب أرصدة المستودع وفحص كفايتها دفعة واحدة بحسب نوع المستودع
        if (warehouse.Type == WarehouseType.Show)
        {
            // تجميع إجمالي الكمية المطلوبة لكل منتج
            var productTotals = dto.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
            // قائمة معرفات المنتجات
            var warehouseProductIds = productTotals.Keys.ToList();
            // جلب الأرصدة المتتبعة في الصالة
            var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, warehouseProductIds, ct)).ToDictionary(s => s.ProductId);
            // التحقق من توفر الرصيد الكافي لكل منتج
            foreach (var(productId, totalQty)in productTotals)
            {
                // فحص كفاية الرصيد
                if (!stocksByProduct.TryGetValue(productId, out var stock) || stock.Quantity < totalQty)
                {
                    // إرجاع خطأ عدم كفاية رصيد الصالة
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("الكمية المطلوبة غير متوفرة في صالة العرض للصنف المحدد", ErrorCodes.InsufficientShowroomStock);
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Storge)
        {
            // التحقق من تعيين باركود لكل بند في حالة البيع من المستودع
            foreach (var item in dto.Items)
            {
                if (!item.ProductBarCodeId.HasValue)
                {
                    // إرجاع خطأ غياب الباركود
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("لم يتم العثور على باركود للصنف المحدد في المخزن", ErrorCodes.InsufficientStock);
                }
            }

            // تجميع الكميات المطلوبة لكل باركود
            var barcodeTotals = dto.Items.GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
            // قائمة الباركودات
            var warehouseBarcodeIds = barcodeTotals.Keys.ToList();
            // جلب الأرصدة المتتبعة بالمستودع
            var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, warehouseBarcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId);
            // التحقق من كفاية رصيد كل باركود
            foreach (var(barcodeId, totalQty)in barcodeTotals)
            {
                // فحص الرصيد المتوفر
                if (!stocksByBarcode.TryGetValue(barcodeId, out var stock) || stock.Quantity < totalQty)
                {
                    // إرجاع خطأ عدم كفاية رصيد المستودع
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("الكمية المطلوبة غير متوفرة في المخزن للصنف المحدد", ErrorCodes.InsufficientStock);
                }
            }
        }

        // تحويل كائن DTO إلى كيان فاتورة المبيعات
        var invoice = _mapper.Map<SalesInvoice>(dto);
        invoice.PosRequestHash = posRequestHash;
        // ضبط تاريخ الفاتورة
        invoice.InvoiceDate = dto.InvoiceDate == default ? DateTime.UtcNow : dto.InvoiceDate;
        // لا تثق في الإجماليات أو التكلفة القادمة من العميل.
        invoice.SubTotal = grossSubTotal;
        invoice.TaxAmount = dto.TaxAmount;
        invoice.TotalAmount = calculatedTotal;
        invoice.PaidAmount = dto.PaidAmount;
        invoice.RemainingAmount = remainingAmount;
        invoice.PaymentSafeId = paymentSafe?.Id;
        invoice.CashSessionId = cashSession?.Id;
        invoice.PosTerminalId = cashSession?.PosTerminalId;
        invoice.Status = calculatedTotal == dto.PaidAmount ? InvoiceStatus.Paid : dto.PaidAmount > 0 ? InvoiceStatus.PartiallyPaid : InvoiceStatus.Pending;
        // بناء قائمة بنود الفاتورة وحساب إجماليات السطور
        invoice.Items = dto.Items.Select(item => new SalesInvoiceItem { // معرف المنتج
 ProductId = item.ProductId, // معرف الباركود
 ProductBarCodeId = item.ProductBarCodeId, // الكمية المباعة
 Quantity = item.Quantity, // سعر بيع الوحدة
 UnitPrice = item.UnitPrice, // تكلفة الوحدة لحساب الربحية
 UnitCost = productsById[item.ProductId].AveragePrice > 0 ? productsById[item.ProductId].AveragePrice : productsById[item.ProductId].CostPrice, // قيمة الخصم على السطر
 DiscountAmount = item.DiscountAmount, // صافي إجمالي السطر
 LineTotal = (item.Quantity * item.UnitPrice) - item.DiscountAmount }).ToList();
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // خصم الكميات من أرصدة المخزون المتتبعة مباشرة دون استعلامات إضافية
            if (warehouse.Type == WarehouseType.Show)
            {
                // تجميع الكميات المطلوبة لكل صنف
                var productTotals = invoice.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
                // قائمة المعرفات
                var warehouseProductIds = productTotals.Keys.ToList();
                // جلب الأرصدة المتتبعة
                var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, warehouseProductIds, ct)).ToDictionary(s => s.ProductId);
                // خصم الكميات من الصالة
                foreach (var(productId, totalQty)in productTotals)
                {
                    // فحص وجود الرصيد وخصم الكمية
                    if (stocksByProduct.TryGetValue(productId, out var stock))
                    {
                        // طرح الكمية المباعة
                        stock.Quantity = checked(stock.Quantity - (int)totalQty);
                    }
                }
            }
            else if (warehouse.Type == WarehouseType.Storge)
            {
                // تجميع الكميات لكل باركود
                var barcodeTotals = invoice.Items.Where(i => i.ProductBarCodeId.HasValue).GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => (long)i.Quantity));
                // قائمة الباركودات
                var warehouseBarcodeIds = barcodeTotals.Keys.ToList();
                // جلب الأرصدة المتتبعة
                var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, warehouseBarcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId);
                // خصم الكميات من المستودع
                foreach (var(barcodeId, totalQty)in barcodeTotals)
                {
                    // فحص وجود الرصيد وخصم الكمية
                    if (stocksByBarcode.TryGetValue(barcodeId, out var stock))
                    {
                        // طرح الكمية المباعة
                        stock.Quantity = checked(stock.Quantity - (int)totalQty);
                    }
                }
            }

            // إضافة فاتورة المبيعات إلى المستودع
            if (invoice.Id == Guid.Empty)
                invoice.Id = Guid.NewGuid();
            await _unitOfWork.SalesInvoices.AddAsync(invoice, ct);
            if (customer is not null)
            {
                await _financialPosting.PostCustomerBalanceAsync(customer, invoice.TotalAmount, CustomerTransactionType.SalesInvoice, invoice.BranchId, FinancialDocumentType.SalesInvoice, invoice.Id, invoice.InvoiceDate, invoice.PaymentMethod, $"فاتورة مبيعات رقم {invoice.InvoiceNumber}", ct);
                await _financialPosting.PostCustomerBalanceAsync(customer, -invoice.PaidAmount, CustomerTransactionType.Payment, invoice.BranchId, FinancialDocumentType.SalesInvoice, invoice.Id, invoice.InvoiceDate, invoice.PaymentMethod, $"دفعة فاتورة مبيعات رقم {invoice.InvoiceNumber}", ct);
            }

            if (paymentSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(paymentSafe, invoice.PaidAmount, FinancialDocumentType.SalesInvoice, invoice.Id, invoice.InvoiceDate, $"تحصيل فاتورة مبيعات رقم {invoice.InvoiceNumber}", ct, invoice.CashSessionId, $"sales-invoice:{invoice.Id}:payment");
            }

            // حفظ التغييرات وحركات المخزون في قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تغير المخزون أو رصيد الدرج أثناء الترحيل؛ أعد تحميل البيانات وحاول مجدداً", ErrorCodes.ConcurrencyError);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // إعادة جلب الفاتورة المحفوظة مع التفاصيل
        var created = await _unitOfWork.SalesInvoices.GetDetailsAsync(invoice.Id, ct) ?? invoice;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(created);
        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<SalesInvoiceResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        if (dto.Status is not null and not InvoiceStatus.Draft || dto.PaidAmount.GetValueOrDefault() != 0)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تعديل المسودة لا يسمح بالترحيل أو تسجيل دفعات", ErrorCodes.ValidationError);

        var invoice = await _unitOfWork.SalesInvoices.GetDetailsForUpdateAsync(id, ct);
        if (invoice is null)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        if (invoice.Status != InvoiceStatus.Draft || invoice.PaidAmount != 0 || invoice.CashSessionId.HasValue ||
            invoice.InvoiceNumber.StartsWith("POS-", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة مرحلة — استخدم الإلغاء أو تسجيل مرتجع", ErrorCodes.ValidationError);

        var warehouse = invoice.Warehouse ?? await _unitOfWork.Warehouses.GetByIdForUpdateAsync(invoice.WarehouseId, ct);
        if (warehouse is null || !Enum.IsDefined(warehouse.Type))
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);

        List<SalesInvoiceItem>? replacement = null;
        if (dto.Items is not null)
        {
            if (dto.Items.Count == 0)
                return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب أن تحتوي الفاتورة على بند واحد على الأقل", ErrorCodes.ValidationError);
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = (await _unitOfWork.Products.GetByIdsAsync(productIds, ct)).ToDictionary(p => p.Id);
            if (products.Count != productIds.Count)
                return ServiceResult<SalesInvoiceResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
            var barcodes = await _unitOfWork.Products.GetBarcodesByProductIdsAsync(productIds, ct);
            var barcodeById = barcodes.ToDictionary(b => b.Id);
            var defaults = barcodes.GroupBy(b => b.ProductId).ToDictionary(g => g.Key, g => g.OrderBy(b => b.Id).First().Id);
            replacement = new List<SalesInvoiceItem>();
            foreach (var item in dto.Items)
            {
                var barcodeId = item.ProductBarCodeId ?? (defaults.TryGetValue(item.ProductId, out var fallback) ? fallback : (Guid?)null);
                if (barcodeId.HasValue && (!barcodeById.TryGetValue(barcodeId.Value, out var barcode) || barcode.ProductId != item.ProductId) ||
                    warehouse.Type == WarehouseType.Storge && !barcodeId.HasValue)
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("الباركود لا يتبع الصنف المحدد أو غير متوفر", ErrorCodes.ValidationError);
                var gross = item.Quantity * item.UnitPrice;
                if (item.DiscountAmount > gross)
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("خصم البند يتجاوز قيمته", ErrorCodes.ValidationError);
                var product = products[item.ProductId];
                replacement.Add(new SalesInvoiceItem
                {
                    TenantId = invoice.TenantId, SalesInvoiceId = invoice.Id, ProductId = item.ProductId,
                    ProductBarCodeId = barcodeId, Quantity = item.Quantity, UnitPrice = item.UnitPrice,
                    UnitCost = product.AveragePrice > 0 ? product.AveragePrice : product.CostPrice,
                    DiscountAmount = item.DiscountAmount, LineTotal = gross - item.DiscountAmount
                });
            }
        }

        var lines = replacement ?? invoice.Items.ToList();
        var subTotal = lines.Sum(i => i.Quantity * i.UnitPrice);
        var net = lines.Sum(i => i.LineTotal);
        var tax = dto.TaxAmount ?? invoice.TaxAmount;
        var total = net - invoice.DiscountAmount + tax;
        if (invoice.DiscountAmount < 0 || invoice.DiscountAmount > net || total < 0 || total > MoneyAmountAttribute.Maximum ||
            subTotal > MoneyAmountAttribute.Maximum)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("إجماليات المسودة أو الخصم تتجاوز الحدود المسموحة", ErrorCodes.ValidationError);

        // A draft has no inventory or ledger effect. Mutate only after every check succeeds.
        if (replacement is not null)
        {
            foreach (var old in invoice.Items.ToList())
                _unitOfWork.SalesInvoices.HardDeleteItem(old);
            invoice.Items = replacement;
        }
        invoice.SubTotal = subTotal;
        invoice.TaxAmount = tax;
        invoice.TotalAmount = total;
        invoice.RemainingAmount = total;
        if (dto.PaymentMethod.HasValue) invoice.PaymentMethod = dto.PaymentMethod.Value;
        invoice.Notes = dto.Notes;
        _unitOfWork.SalesInvoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);
        var updated = await _unitOfWork.SalesInvoices.GetDetailsAsync(id, ct) ?? invoice;
        return ServiceResult<SalesInvoiceResponseDto>.Success(_mapper.Map<SalesInvoiceResponseDto>(updated));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<SalesInvoiceResponseDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default)
    {
        // استعلام الفاتورة ككيان متتبع
        var invoice = await _unitOfWork.SalesInvoices.GetDetailsForUpdateAsync(id, ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ عدم وجود الفاتورة
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        if (status is not InvoiceStatus.Cancelled and not InvoiceStatus.Voided)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تغيير حالة الفاتورة المرحلة غير مسموح مباشرة؛ المتاح هنا هو الإلغاء فقط", ErrorCodes.ValidationError);
        // التحقق مما إذا كان الإجراء إلغاء لفاتورة نشطة
        bool isCancelling = (status == InvoiceStatus.Cancelled || status == InvoiceStatus.Voided) && (invoice.Status != InvoiceStatus.Cancelled && invoice.Status != InvoiceStatus.Voided);
        if (!isCancelling)
            return ServiceResult<SalesInvoiceResponseDto>.Failure("الفاتورة ملغاة بالفعل", ErrorCodes.ValidationError);
        var wasPosted = invoice.Status != InvoiceStatus.Draft;
        if (wasPosted && await _unitOfWork.SalesReturns.HasInvoiceReferencesAsync(id, ct))
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يمكن إلغاء فاتورة لها مرتجعات مرتبطة؛ اعكس المرتجعات أولاً", ErrorCodes.ValidationError);

        Customer? customer = invoice.CustomerId.HasValue ? await _unitOfWork.Customers.GetByIdForUpdateAsync(invoice.CustomerId.Value, ct) : null;
        Safe? paymentSafe = null;
        if (wasPosted && invoice.PaidAmount > 0)
        {
            if (!invoice.PaymentSafeId.HasValue)
                return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يمكن إلغاء فاتورة قديمة مدفوعة قبل تحديد خزينة التسوية لها", ErrorCodes.ValidationError);
            var safeResult = await _financialPosting.ResolveSafeAsync(invoice.BranchId, invoice.PaymentSafeId, invoice.PaidAmount, outbound: true, ct);
            if (!safeResult.IsSuccess)
                return ServiceResult<SalesInvoiceResponseDto>.Failure(safeResult.ErrorMessage!, safeResult.ErrorCode);
            paymentSafe = safeResult.Data;
        }

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            // تعيين الحالة الجديدة
            invoice.Status = status;
            // استعادة المخزون في حال إلغاء الفاتورة
            if (wasPosted && isCancelling && invoice.Items != null && invoice.Items.Any())
            {
                // جلب بيانات المستودع
                var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(invoice.WarehouseId, ct);
                if (warehouse != null)
                {
                    if (warehouse.Type == WarehouseType.Show)
                    {
                        // معرفات المنتجات
                        var productIds = invoice.Items.Select(i => i.ProductId).Distinct().ToList();
                        // جلب أرصدة الصالة
                        var stocksByProduct = (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productIds, ct)).ToDictionary(s => s.ProductId);
                        // استعادة كمية كل صنف إلى رصيد الصالة
                        foreach (var item in invoice.Items)
                        {
                            if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                            {
                                stock.Quantity = checked(stock.Quantity + item.Quantity);
                            }
                        }
                    }
                    else if (warehouse.Type == WarehouseType.Storge)
                    {
                        // معرفات الباركودات
                        var barcodeIds = invoice.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
                        // استعادة كمية كل باركود إلى رصيد المستودع
                        if (barcodeIds.Count > 0)
                        {
                            var stocksByBarcode = (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId);
                            foreach (var item in invoice.Items)
                            {
                                if (item.ProductBarCodeId.HasValue && stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                                {
                                    stock.Quantity = checked(stock.Quantity + item.Quantity);
                                }
                            }
                        }
                    }
                }
            }

            if (wasPosted && customer is not null)
            {
                await _financialPosting.PostCustomerBalanceAsync(customer, invoice.PaidAmount, CustomerTransactionType.Reversal, invoice.BranchId, FinancialDocumentType.SalesInvoice, invoice.Id, DateTime.UtcNow, invoice.PaymentMethod, $"عكس دفعة فاتورة مبيعات ملغاة رقم {invoice.InvoiceNumber}", ct);
                await _financialPosting.PostCustomerBalanceAsync(customer, -invoice.TotalAmount, CustomerTransactionType.Reversal, invoice.BranchId, FinancialDocumentType.SalesInvoice, invoice.Id, DateTime.UtcNow, invoice.PaymentMethod, $"عكس فاتورة مبيعات ملغاة رقم {invoice.InvoiceNumber}", ct);
            }

            if (paymentSafe is not null)
            {
                await _financialPosting.PostSafeBalanceAsync(paymentSafe, -invoice.PaidAmount, FinancialDocumentType.SalesInvoice, invoice.Id, DateTime.UtcNow, $"رد تحصيل فاتورة مبيعات ملغاة رقم {invoice.InvoiceNumber}", ct);
            }

            // حفظ التغييرات وحركات المخزون
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return ServiceResult<SalesInvoiceResponseDto>.Failure("تغيرت الفاتورة أو أرصدتها أثناء الإلغاء؛ أعد تحميلها قبل إعادة المحاولة", ErrorCodes.ConcurrencyError);
        }
        catch (OverflowException)
        {
            await _unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            return ServiceResult<SalesInvoiceResponseDto>.Failure("إلغاء الفاتورة يتجاوز سعة المخزون المسموحة", ErrorCodes.ValidationError);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }

        // إعادة جلب الفاتورة مع التفاصيل
        var updated = await _unitOfWork.SalesInvoices.GetDetailsAsync(id, ct) ?? invoice;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(updated);
        // إرجاع النتيجة بنجاح
        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام الفاتورة ككيان متتبع
        var invoice = await _unitOfWork.SalesInvoices.GetDetailsForUpdateAsync(id, ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ عدم وجود الفاتورة
            return ServiceResult.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // منع حذف فاتورة لها مرتجعات مرتبطة لحماية التكامل المرجعي
        bool hasReturns = await _unitOfWork.SalesReturns.HasInvoiceReferencesAsync(id, ct);
        if (hasReturns)
        {
            // إرجاع خطأ ارتباط الفاتورة بمرتجعات
            return ServiceResult.Failure("لا يمكن حذف فاتورة لها مرتجعات مرتبطة — احذف المرتجعات أولاً", ErrorCodes.ValidationError);
        }

        if (invoice.Status != InvoiceStatus.Draft)
            return ServiceResult.Failure("لا يجوز حذف فاتورة مرحلة حفاظاً على سجل التدقيق؛ استخدم الإلغاء بدلاً من الحذف", ErrorCodes.ValidationError);
        // تطبيق الحذف المنطقي للفاتورة
        _unitOfWork.SalesInvoices.SoftDelete(invoice);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
