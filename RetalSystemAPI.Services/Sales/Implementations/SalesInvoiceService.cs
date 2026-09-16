using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Sales.Interfaces;
using RetalSystemAPI.Services.Sales.Specifications;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Sales.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة فواتير المبيعات وحركات البيع وخصم المخزون والتحقق من كفاية الأرصدة.
/// </summary>
public class SalesInvoiceService : ISalesInvoiceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة فواتير المبيعات مع حقن وحدة العمل والمحول.
    /// </summary>
    public SalesInvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        var dto = _mapper.Map<SalesInvoiceResponseDto>(invoice);
        return ServiceResult<SalesInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesInvoiceResponseDto>> GetByInvoiceNumberAsync(string invoiceNumber, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(invoiceNumber), ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        var dto = _mapper.Map<SalesInvoiceResponseDto>(invoice);
        return ServiceResult<SalesInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new SalesInvoiceListSpec(branchId, warehouseId, customerId, status, paymentMethod, fromDate, toDate, search);
        var invoices = await _unitOfWork.SalesInvoices.FindAsync(spec, ct);
        var dtos = _mapper.Map<IReadOnlyList<SalesInvoiceSummaryDto>>(invoices);

        return ServiceResult<IReadOnlyList<SalesInvoiceSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<SalesInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        var spec = new SalesInvoiceListSpec(branchId, warehouseId, customerId, status, paymentMethod, fromDate, toDate, search);
        var (items, totalCount) = await _unitOfWork.SalesInvoices.GetPagedAsync(spec, pageNumber, pageSize, ct);

        var dtos = _mapper.Map<IReadOnlyList<SalesInvoiceSummaryDto>>(items);
        var pagedResult = PagedResult<SalesInvoiceSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        return ServiceResult<PagedResult<SalesInvoiceSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesInvoiceResponseDto>> CreateAsync(CreateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        if (dto.CustomerId.HasValue)
        {
            bool customerExists = await _unitOfWork.Customers.ExistsAsync(c => c.Id == dto.CustomerId.Value, ct);
            if (!customerExists)
            {
                return ServiceResult<SalesInvoiceResponseDto>.Failure("العميل المحدد غير موجود", ErrorCodes.CustomerNotFound);
            }
        }

        bool numExists = await _unitOfWork.SalesInvoices.ExistsAsync(s => s.InvoiceNumber == dto.InvoiceNumber, ct);
        if (numExists)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("رقم الفاتورة مستخدم بالفعل", ErrorCodes.SalesInvoiceNumberExists);
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب إضافة بند واحد على الأقل للفاتورة", ErrorCodes.ValidationError);
        }

        // حل الباركود الافتراضي للأصناف إن لم يتم تحديده — دفعة واحدة لكل الأصناف
        var productIdsNeedingDefaultBarcode = dto.Items
            .Where(i => !i.ProductBarCodeId.HasValue)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        // ملاحظة: اختيار الباركود "الأول" لكل صنف يتبع نفس عدم الحتمية السابق (FirstOrDefault بلا ترتيب)
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

        // جلب أرصدة المستودع دفعة واحدة حسب نوعه (فحص + خصم من نفس الكيانات المتتبعة)
        if (warehouse.Type == WarehouseType.Show)
        {
            var productTotals = dto.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
            var warehouseProductIds = productTotals.Keys.ToList();
            var stocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                s => s.WarehouseId == warehouse.Id && warehouseProductIds.Contains(s.ProductId), ct))
                .ToDictionary(s => s.ProductId);

            foreach (var (productId, totalQty) in productTotals)
            {
                if (!stocksByProduct.TryGetValue(productId, out var stock) || stock.Quantity < totalQty)
                {
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("الكمية المطلوبة غير متوفرة في صالة العرض للصنف المحدد", ErrorCodes.InsufficientShowroomStock);
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Storge)
        {
            foreach (var item in dto.Items)
            {
                if (!item.ProductBarCodeId.HasValue)
                {
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("لم يتم العثور على باركود للصنف المحدد في المخزن", ErrorCodes.InsufficientStock);
                }
            }

            var barcodeTotals = dto.Items.GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
            var warehouseBarcodeIds = barcodeTotals.Keys.ToList();
            var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                s => s.WarehouseId == warehouse.Id && warehouseBarcodeIds.Contains(s.ProductBarcodeId), ct))
                .ToDictionary(s => s.ProductBarcodeId);

            foreach (var (barcodeId, totalQty) in barcodeTotals)
            {
                if (!stocksByBarcode.TryGetValue(barcodeId, out var stock) || stock.Quantity < totalQty)
                {
                    return ServiceResult<SalesInvoiceResponseDto>.Failure("الكمية المطلوبة غير متوفرة في المخزن للصنف المحدد", ErrorCodes.InsufficientStock);
                }
            }
        }

        var invoice = _mapper.Map<SalesInvoice>(dto);
        invoice.InvoiceDate = dto.InvoiceDate == default ? DateTime.UtcNow : dto.InvoiceDate;
        invoice.RemainingAmount = dto.TotalAmount - dto.PaidAmount;

        invoice.Items = dto.Items.Select(item => new SalesInvoiceItem
        {
            ProductId = item.ProductId,
            ProductBarCodeId = item.ProductBarCodeId,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            UnitCost = item.UnitCost,
            DiscountAmount = item.DiscountAmount,
            LineTotal = (item.Quantity * item.UnitPrice) - item.DiscountAmount
        }).ToList();

        // خصم الكميات من المخزون — على نفس الأرصدة المتتبعة التي تم فحصها أعلاه (بدون استعلامات إضافية)
        if (warehouse.Type == WarehouseType.Show)
        {
            var productTotals = invoice.Items.GroupBy(i => i.ProductId).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
            var warehouseProductIds = productTotals.Keys.ToList();
            var stocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                s => s.WarehouseId == warehouse.Id && warehouseProductIds.Contains(s.ProductId), ct))
                .ToDictionary(s => s.ProductId);

            foreach (var (productId, totalQty) in productTotals)
            {
                if (stocksByProduct.TryGetValue(productId, out var stock))
                {
                    stock.Quantity -= totalQty;
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Storge)
        {
            var barcodeTotals = invoice.Items.Where(i => i.ProductBarCodeId.HasValue).GroupBy(i => i.ProductBarCodeId!.Value).ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));
            var warehouseBarcodeIds = barcodeTotals.Keys.ToList();
            var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                s => s.WarehouseId == warehouse.Id && warehouseBarcodeIds.Contains(s.ProductBarcodeId), ct))
                .ToDictionary(s => s.ProductBarcodeId);

            foreach (var (barcodeId, totalQty) in barcodeTotals)
            {
                if (stocksByBarcode.TryGetValue(barcodeId, out var stock))
                {
                    stock.Quantity -= totalQty;
                }
            }
        }

        await _unitOfWork.SalesInvoices.AddAsync(invoice, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var created = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(invoice.Id), ct) ?? invoice;
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(created);

        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesInvoiceResponseDto>> UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultTrackedAsync(new SalesInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // قاعدة قفل الترحيل: الفواتير المدفوعة/المرحلة وفواتير نقاط البيع مغلقة نهائياً — التعديل عبر الإلغاء أو المرتجع
        if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.PartiallyPaid)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة مدفوعة — استخدم الإلغاء أو تسجيل مرتجع", ErrorCodes.ValidationError);
        }

        if (invoice.Status == InvoiceStatus.Cancelled || invoice.Status == InvoiceStatus.Voided)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة ملغاة أو باطلة", ErrorCodes.ValidationError);
        }

        if (invoice.InvoiceNumber.StartsWith("POS-", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فواتير نقاط البيع مغلقة نهائياً — لا يمكن تعديلها، استخدم المرتجع أو الإلغاء", ErrorCodes.ValidationError);
        }

        var warehouse = invoice.Warehouse ?? await _unitOfWork.Warehouses.GetByIdAsync(invoice.WarehouseId, ct);
        if (warehouse is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("المستودع أو صالة العرض غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // في حال تم إرسال بنود جديدة لتعديل الفاتورة (القائمة الفارغة مرفوضة كي لا يُفلت استرجاع كمية البنود المحذوفة)
        if (dto.Items != null)
        {
            if (!dto.Items.Any())
            {
                return ServiceResult<SalesInvoiceResponseDto>.Failure("يجب أن تحتوي الفاتورة على بند واحد على الأقل", ErrorCodes.ValidationError);
            }

            // حل الباركود الافتراضي دفعة واحدة
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

            // 1. استعادة أرصدة المخزون القديمة مؤقتاً وجلب الأرصدة المعنية دفعة واحدة
            if (warehouse.Type == WarehouseType.Show)
            {
                var oldProductIds = invoice.Items.Select(i => i.ProductId).Distinct().ToList();
                var stocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && oldProductIds.Contains(s.ProductId), ct))
                    .ToDictionary(s => s.ProductId);

                foreach (var oldItem in invoice.Items)
                {
                    if (stocksByProduct.TryGetValue(oldItem.ProductId, out var stock))
                    {
                        stock.Quantity += oldItem.Quantity;
                    }
                }

                // 2. التحقق من كفاية الأرصدة للبنود الجديدة (نفس الكيانات المتتبعة)
                var requiredShowStock = dto.Items
                    .GroupBy(i => i.ProductId)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

                foreach (var (productId, qty) in requiredShowStock)
                {
                    if (!stocksByProduct.TryGetValue(productId, out var stock) || stock.Quantity < qty)
                    {
                        // التراجع عن الزيادة المؤقتة
                        foreach (var oldItem in invoice.Items)
                        {
                            if (stocksByProduct.TryGetValue(oldItem.ProductId, out var s))
                            {
                                s.Quantity -= oldItem.Quantity;
                            }
                        }
                        return ServiceResult<SalesInvoiceResponseDto>.Failure("الكمية المطلوبة غير متوفرة في صالة العرض للصنف المحدد", ErrorCodes.InsufficientShowroomStock);
                    }
                }

                // 3. خصم الكميات الجديدة (نفس الكيانات المتتبعة)
                foreach (var (productId, qty) in requiredShowStock)
                {
                    if (stocksByProduct.TryGetValue(productId, out var stock))
                    {
                        stock.Quantity -= qty;
                    }
                }
            }
            else if (warehouse.Type == WarehouseType.Storge)
            {
                var oldBarcodeIds = invoice.Items
                    .Where(i => i.ProductBarCodeId.HasValue)
                    .Select(i => i.ProductBarCodeId!.Value)
                    .Distinct()
                    .ToList();

                var stocksByBarcode = oldBarcodeIds.Count > 0
                    ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                        s => s.WarehouseId == warehouse.Id && oldBarcodeIds.Contains(s.ProductBarcodeId), ct))
                        .ToDictionary(s => s.ProductBarcodeId)
                    : new Dictionary<Guid, StorgeStock>();

                foreach (var oldItem in invoice.Items)
                {
                    if (oldItem.ProductBarCodeId.HasValue &&
                        stocksByBarcode.TryGetValue(oldItem.ProductBarCodeId.Value, out var stock))
                    {
                        stock.Quantity += oldItem.Quantity;
                    }
                }

                var requiredStorgeStock = dto.Items
                    .Where(i => i.ProductBarCodeId.HasValue)
                    .GroupBy(i => i.ProductBarCodeId!.Value)
                    .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

                foreach (var (barcodeId, qty) in requiredStorgeStock)
                {
                    if (!stocksByBarcode.TryGetValue(barcodeId, out var stock) || stock.Quantity < qty)
                    {
                        // التراجع عن الزيادة المؤقتة
                        foreach (var oldItem in invoice.Items)
                        {
                            if (oldItem.ProductBarCodeId.HasValue &&
                                stocksByBarcode.TryGetValue(oldItem.ProductBarCodeId.Value, out var s))
                            {
                                s.Quantity -= oldItem.Quantity;
                            }
                        }
                        return ServiceResult<SalesInvoiceResponseDto>.Failure("الكمية المطلوبة غير متوفرة في المخزن للصنف المحدد", ErrorCodes.InsufficientStock);
                    }
                }

                foreach (var (barcodeId, qty) in requiredStorgeStock)
                {
                    if (stocksByBarcode.TryGetValue(barcodeId, out var stock))
                    {
                        stock.Quantity -= qty;
                    }
                }
            }

            // 4. تحديث قائمة بنود الفاتورة وحساب الإجماليات
            invoice.Items.Clear();
            decimal subTotal = 0;
            decimal totalDiscount = 0;
            foreach (var item in dto.Items)
            {
                var lineDiscount = item.DiscountAmount;
                var lineTotal = (item.Quantity * item.UnitPrice) - lineDiscount;
                subTotal += (item.Quantity * item.UnitPrice);
                totalDiscount += lineDiscount;

                invoice.Items.Add(new SalesInvoiceItem
                {
                    TenantId = invoice.TenantId,
                    SalesInvoiceId = invoice.Id,
                    ProductId = item.ProductId,
                    ProductBarCodeId = item.ProductBarCodeId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    UnitCost = item.UnitCost,
                    DiscountAmount = lineDiscount,
                    LineTotal = lineTotal
                });
            }

            invoice.SubTotal = subTotal;
            invoice.DiscountAmount = totalDiscount;
            invoice.TotalAmount = subTotal - totalDiscount;
        }

        invoice.Status = dto.Status;
        invoice.PaymentMethod = dto.PaymentMethod;
        invoice.PaidAmount = dto.PaidAmount;
        invoice.RemainingAmount = invoice.TotalAmount - dto.PaidAmount;
        invoice.Notes = dto.Notes;

        _unitOfWork.SalesInvoices.Update(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct) ?? invoice;
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(updated);

        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SalesInvoiceResponseDto>> UpdateStatusAsync(Guid id, InvoiceStatus status, CancellationToken ct = default)
    {
        // جلب الفاتورة متتبعةً حتى تُحفظ تعديلاتها (الحالة + أثر استرجاع المخزون) عند الحفظ
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultTrackedAsync(new SalesInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult<SalesInvoiceResponseDto>.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        bool isCancelling = (status == InvoiceStatus.Cancelled || status == InvoiceStatus.Voided) &&
                            (invoice.Status != InvoiceStatus.Cancelled && invoice.Status != InvoiceStatus.Voided);

        invoice.Status = status;

        // استعادة المخزون في حال إلغاء الفاتورة — دفعة أرصدة واحدة
        if (isCancelling && invoice.Items != null && invoice.Items.Any())
        {
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(invoice.WarehouseId, ct);
            if (warehouse != null)
            {
                if (warehouse.Type == WarehouseType.Show)
                {
                    var productIds = invoice.Items.Select(i => i.ProductId).Distinct().ToList();
                    var stocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                        s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                        .ToDictionary(s => s.ProductId);

                    foreach (var item in invoice.Items)
                    {
                        if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                        {
                            stock.Quantity += item.Quantity;
                        }
                    }
                }
                else if (warehouse.Type == WarehouseType.Storge)
                {
                    var barcodeIds = invoice.Items
                        .Where(i => i.ProductBarCodeId.HasValue)
                        .Select(i => i.ProductBarCodeId!.Value)
                        .Distinct()
                        .ToList();

                    if (barcodeIds.Count > 0)
                    {
                        var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                            s => s.WarehouseId == warehouse.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                            .ToDictionary(s => s.ProductBarcodeId);

                        foreach (var item in invoice.Items)
                        {
                            if (item.ProductBarCodeId.HasValue &&
                                stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                            {
                                stock.Quantity += item.Quantity;
                            }
                        }
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var updated = await _unitOfWork.SalesInvoices.FirstOrDefaultAsync(new SalesInvoiceWithDetailsSpec(id), ct) ?? invoice;
        var responseDto = _mapper.Map<SalesInvoiceResponseDto>(updated);

        return ServiceResult<SalesInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var invoice = await _unitOfWork.SalesInvoices.FirstOrDefaultTrackedAsync(new SalesInvoiceWithDetailsSpec(id), ct);
        if (invoice is null)
        {
            return ServiceResult.Failure("فاتورة المبيعات غير موجودة", ErrorCodes.SalesInvoiceNotFound);
        }

        // منع حذف فاتورة لها مرتجعات مرتبطة — يلزم حذف المرتجعات أولاً
        bool hasReturns = await _unitOfWork.SalesReturns.ExistsAsync(r => r.OriginalInvoiceId == id, ct);
        if (hasReturns)
        {
            return ServiceResult.Failure("لا يمكن حذف فاتورة لها مرتجعات مرتبطة — احذف المرتجعات أولاً", ErrorCodes.ValidationError);
        }

        // استرجاع المخزون إذا تم حذف فاتورة نشطة — دفعة أرصدة واحدة
        if (invoice.Status != InvoiceStatus.Cancelled && invoice.Status != InvoiceStatus.Voided && invoice.Items != null && invoice.Items.Any())
        {
            var warehouse = invoice.Warehouse ?? await _unitOfWork.Warehouses.GetByIdAsync(invoice.WarehouseId, ct);
            if (warehouse != null)
            {
                if (warehouse.Type == WarehouseType.Show)
                {
                    var productIds = invoice.Items.Select(i => i.ProductId).Distinct().ToList();
                    var stocksByProduct = (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                        s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                        .ToDictionary(s => s.ProductId);

                    foreach (var item in invoice.Items)
                    {
                        if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                        {
                            stock.Quantity += item.Quantity;
                        }
                    }
                }
                else if (warehouse.Type == WarehouseType.Storge)
                {
                    var barcodeIds = invoice.Items
                        .Where(i => i.ProductBarCodeId.HasValue)
                        .Select(i => i.ProductBarCodeId!.Value)
                        .Distinct()
                        .ToList();

                    if (barcodeIds.Count > 0)
                    {
                        var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                            s => s.WarehouseId == warehouse.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                            .ToDictionary(s => s.ProductBarcodeId);

                        foreach (var item in invoice.Items)
                        {
                            if (item.ProductBarCodeId.HasValue &&
                                stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                            {
                                stock.Quantity += item.Quantity;
                            }
                        }
                    }
                }
            }
        }

        _unitOfWork.SalesInvoices.SoftDelete(invoice);
        await _unitOfWork.SaveChangesAsync(ct);

        return ServiceResult.Success();
    }
}
