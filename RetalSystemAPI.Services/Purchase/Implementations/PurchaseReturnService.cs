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
/// تنفيذ خدمة إدارة مرتجعات المشتريات وخصم البضائع المرتجعة من المخازن بدقة على مستوى النكهات والباركودات.
/// </summary>
public class PurchaseReturnService : IPurchaseReturnService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول الكيانات لتحويل النماذج إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة مرتجعات المشتريات مع حقن وحدة العمل ومحول الكيانات.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل النماذج</param>
    public PurchaseReturnService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول البيانات
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام مرتجع المشتريات بالمعرف مع تضمين الفرع والمستودع والمورد والفاتورة وتفاصيل الأصناف
        var purchaseReturn = await _unitOfWork.PurchaseReturns.FirstOrDefaultAsync(new PurchaseReturnWithDetailsSpec(id), ct);
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

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default)
    {
        // استعلام مرتجع المشتريات بواسطة رقم إشعار المرتجع مع التفاصيل الكاملة
        var purchaseReturn = await _unitOfWork.PurchaseReturns.FirstOrDefaultAsync(new PurchaseReturnWithDetailsSpec(returnNumber), ct);
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
        // بناء مواصفة استعلام القوائم لمرتجعات المشتريات وفق محددات الفلترة
        var spec = new PurchaseReturnListSpec(branchId, warehouseId, supplierId, reason, paymentMethod, fromDate, toDate, search);
        // جلب قائمة المرتجعات المطابقة
        var returns = await _unitOfWork.PurchaseReturns.FindAsync(spec, ct);
        // تحويل الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<PurchaseReturnSummaryDto>>(returns);

        // إرجاع النتيجة الناجحة مع قائمة الملخصات
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
        // بناء مواصفة الفلترة والترقيم لمرتجعات المشتريات
        var spec = new PurchaseReturnListSpec(branchId, warehouseId, supplierId, reason, paymentMethod, fromDate, toDate, search);
        // تنفيذ الاستعلام الصفحي لجلب سجلات الصفحة وإجمالي العدد
        var (items, totalCount) = await _unitOfWork.PurchaseReturns.GetPagedAsync(spec, pageNumber, pageSize, ct);

        // تحويل عناصر الصفحة الحالية إلى قائمة ملخصات
        var dtos = _mapper.Map<IReadOnlyList<PurchaseReturnSummaryDto>>(items);
        // إنشاء نتيجة الترقيم مع حساب عدد الصفحات
        var pagedResult = PagedResult<PurchaseReturnSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة الناجحة المصفحة
        return ServiceResult<PagedResult<PurchaseReturnSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseReturnResponseDto>> CreateAsync(CreatePurchaseReturnDto dto, CancellationToken ct = default)
    {
        // 1. التحقق من وجود المورد المحدد
        bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId, ct);
        // في حال عدم وجود المورد
        if (!supplierExists)
        {
            // إرجاع خطأ عدم وجود المورد
            return ServiceResult<PurchaseReturnResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // 2. التحقق من وجود الفرع المحدد
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        // في حال عدم وجود الفرع
        if (!branchExists)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<PurchaseReturnResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // 3. التحقق من وجود المستودع
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
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
            return ServiceResult<PurchaseReturnResponseDto>.Failure(
                "مرتجع المشتريات متاح فقط من المستودع الرئيسي (المخزن) لضمان دقة النكهات والباركودات. يرجى تحويل البضاعة من الصالة إلى المخزن أولاً عبر التحويلات المخزنية.",
                ErrorCodes.PurchaseReturnInvalidWarehouse);
        }

        // 5. التحقق من عدم تكرار رقم إشعار المرتجع
        bool numExists = await _unitOfWork.PurchaseReturns.ExistsAsync(r => r.ReturnNumber == dto.ReturnNumber, ct);
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
            originalInvoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(
                new PurchaseInvoiceWithDetailsSpec(dto.PurchaseInvoiceId.Value), ct);
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
            if (originalInvoice.Status == InvoiceStatus.Cancelled || originalInvoice.Status == InvoiceStatus.Voided)
            {
                // إرجاع خطأ عدم جواز الإرجاع على فاتورة ملغاة
                return ServiceResult<PurchaseReturnResponseDto>.Failure("لا يمكن إنشاء مرتجع على فاتورة ملغاة", ErrorCodes.ValidationError);
            }

            // المرتجع يخرج حصراً من نفس فرع ومستودع الفاتورة الأصلية
            dto.BranchId = originalInvoice.BranchId;
            dto.WarehouseId = originalInvoice.WarehouseId;
        }

        // 7. التحقق من وجود بنود للمرتجع
        if (dto.Items == null || !dto.Items.Any())
        {
            // إرجاع خطأ اشتراط وجود بند واحد على الأقل
            return ServiceResult<PurchaseReturnResponseDto>.Failure("يجب إضافة بند واحد على الأقل لمرتجع المشتريات", ErrorCodes.ValidationError);
        }

        // 8. التحقق من كفاية المخزون في المخزن لكل صنف ونكهة/باركود — دفعات موحدة
        var itemProductIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        // جلب الأصناف المشاركة دفعة واحدة وبناء القاموس
        var productsById = (await _unitOfWork.Products.FindAsync(p => itemProductIds.Contains(p.Id), ct))
            .ToDictionary(p => p.Id);

        // جمع كل الباركودات المعنية وحل الافتراضي دفعة واحدة
        var fallbackProductIds = dto.Items
            .Where(i => !i.ProductBarCodeId.HasValue || i.ProductBarCodeId.Value == Guid.Empty)
            .Select(i => i.ProductId)
            .Distinct()
            .ToList();

        // جلب الباركودات الافتراضية دفعة واحدة
        var defaultBarcodeByProduct = fallbackProductIds.Count > 0
            ? (await _unitOfWork.ProductBarCodes.FindAsync(
                b => fallbackProductIds.Contains(b.ProductId), ct))
                .GroupBy(b => b.ProductId)
                .ToDictionary(g => g.Key, g => g.First())
            : new Dictionary<Guid, ProductBarCode>();

        // قائمة لتجميع البنود بعد التحقق منها وربطها بالباركود الصحيح
        var validatedItems = new List<(CreatePurchaseReturnItemDto dtoItem, Guid barcodeId)>();
        // المرور على بنود المرتجع للتحقق من صحتها وتوفرها
        foreach (var item in dto.Items)
        {
            // التحقق من وجود الصنف في قاعدة البيانات
            if (!productsById.TryGetValue(item.ProductId, out var product))
            {
                // إرجاع خطأ بعدم وجود الصنف
                return ServiceResult<PurchaseReturnResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
            }

            // استخراج باركود الصنف
            var barcodeId = item.ProductBarCodeId;
            // إذا لم يتم تحديد باركود يتم أخذ الباركود الافتراضي
            if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty)
            {
                if (defaultBarcodeByProduct.TryGetValue(item.ProductId, out var bc))
                {
                    barcodeId = bc.Id;
                }
            }

            // التحقق من وجود باركود صالح للصنف
            if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty)
            {
                // إرجاع خطأ بعدم وجود باركود أو نكهة مسجلة
                return ServiceResult<PurchaseReturnResponseDto>.Failure(
                    $"الصنف '{product.Name}' لا يحتوي على باركود/نكهة مسجلة في المخزن.",
                    ErrorCodes.BarCodeNotFound);
            }

            // مع فاتورة أصلية: البند يجب أن يكون ضمن بنودها (باركود البند المطابق) وسقف الكمية المرتجعة ≤ المشتُرى
            if (originalInvoice != null)
            {
                // تصفية بنود الفاتورة الأصلية المطابقة لنفس الصنف
                var matchingInvoiceItems = originalInvoice.Items
                    .Where(i => i.ProductId == item.ProductId)
                    .ToList();

                // التحقق من وجود الصنف بالفاتورة الأصلية
                if (matchingInvoiceItems.Count == 0)
                {
                    // إرجاع خطأ بعدم وجود الصنف بالفاتورة الأصلية
                    return ServiceResult<PurchaseReturnResponseDto>.Failure(
                        $"الصنف '{product.Name}' غير موجود ضمن بنود الفاتورة الأصلية.",
                        ErrorCodes.ValidationError);
                }

                // العثور على البند المطابق لنفس النكهة/الباركود
                var matchedInvoiceItem = matchingInvoiceItems.FirstOrDefault(i => i.ProductBarCodeId == barcodeId.Value)
                    ?? matchingInvoiceItems.FirstOrDefault(i => i.ProductBarCodeId.HasValue);

                // التأكد من تطابق الباركود
                if (matchedInvoiceItem == null || matchedInvoiceItem.ProductBarCodeId != barcodeId.Value)
                {
                    // إرجاع خطأ بعدم وجود هذه النكهة في الفاتورة الأصلية
                    return ServiceResult<PurchaseReturnResponseDto>.Failure(
                        $"النكهة / الباركود المحدد للصنف '{product.Name}' غير موجود ضمن بنود الفاتورة الأصلية.",
                        ErrorCodes.ValidationError);
                }

                // حساب إجمالي الكمية المشتراة لهذا الباركود
                var purchasedQuantity = matchingInvoiceItems
                    .Where(i => i.ProductBarCodeId == barcodeId.Value)
                    .Sum(i => i.Quantity);

                // حساب إجمالي الكميات المرتجعة سابقاً من هذه الفاتورة لنفس الباركود
                var previouslyReturned = (await _unitOfWork.PurchaseReturnItems.FindAsync(
                    ri => ri.PurchaseReturn != null &&
                          ri.PurchaseReturn.PurchaseInvoiceId == originalInvoice.Id &&
                          ri.ProductBarCodeId == barcodeId.Value, ct))
                    .Sum(ri => ri.Quantity);

                // التأكد من عدم تجاوز سقف الكمية المشتراة
                if (previouslyReturned + item.Quantity > purchasedQuantity)
                {
                    // إرجاع خطأ تجاوز الكمية المشتراة
                    return ServiceResult<PurchaseReturnResponseDto>.Failure(
                        $"الكمية المرتجعة للصنف '{product.Name}' تتجاوز الكمية المشتراة (المشتراة: {purchasedQuantity} — المرتجع سابقاً: {previouslyReturned}).",
                        ErrorCodes.PurchaseReturnInvalidQuantity);
                }
            }

            // إضافة البند المحقق لقائمة البنود الجاهزة
            validatedItems.Add((item, barcodeId.Value));
        }

        // جلب أرصدة المخزن المعنية دفعة واحدة لتفادي استعلامات N+1
        var validatedBarcodeIds = validatedItems.Select(v => v.barcodeId).Distinct().ToList();
        // استعلام أرصدة المخزن المتتبعة
        var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
            s => s.WarehouseId == warehouse.Id && validatedBarcodeIds.Contains(s.ProductBarcodeId), ct))
            .ToDictionary(s => s.ProductBarcodeId);

        // التحقق من كفاية الرصيد المخزني الفعلي لكل بند
        foreach (var (dtoItem, barcodeId) in validatedItems)
        {
            // محاولة جلب رصيد الباركود
            stocksByBarcode.TryGetValue(barcodeId, out var stock);
            // تقريب كمية الإرجاع للأعلى
            int returnQty = (int)Math.Ceiling(dtoItem.Quantity);
            // فحص كفاية الرصيد في المخزن
            if (stock is null || stock.Quantity < returnQty)
            {
                // الكمية المتوفرة حالياً
                int available = stock?.Quantity ?? 0;
                // إرجاع خطأ عدم كفاية الرصيد المخزني للإرجاع
                return ServiceResult<PurchaseReturnResponseDto>.Failure(
                    $"رصيد المخزون المتوفر في المخزن للصنف '{productsById[dtoItem.ProductId].Name}' ({available}) غير كافٍ لإتمام الإرجاع ({returnQty}).",
                    ErrorCodes.InsufficientStock);
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
        purchaseReturn.Items = validatedItems.Select(v => new PurchaseReturnItem
        {
            // معرف الصنف
            ProductId = v.dtoItem.ProductId,
            // معرف باركود الصنف
            ProductBarCodeId = v.barcodeId,
            // الكمية المرتجعة
            Quantity = v.dtoItem.Quantity,
            // سعر وحدة الإرجاع
            UnitPrice = v.dtoItem.UnitPrice,
            // إجمالي سطر المرتجع
            LineTotal = Math.Max(0, v.dtoItem.Quantity * v.dtoItem.UnitPrice),
            // ملاحظات البند
            Notes = v.dtoItem.Notes
        }).ToList();

        // احتساب إجمالي مبلغ المرتجع
        purchaseReturn.TotalAmount = purchaseReturn.Items.Sum(i => i.LineTotal);

        // 10. خصم البضاعة المرتجعة من مخزون المستودع الرئيسي — على نفس الأرصدة المتتبعة أعلاه
        foreach (var item in purchaseReturn.Items)
        {
            // استخراج رصيد الباركود وتخفيضه بالكمية المرتجعة
            if (stocksByBarcode.TryGetValue(item.ProductBarCodeId!.Value, out var stock))
            {
                // خصم الكمية المرتجعة من رصيد المخزن
                stock.Quantity -= (int)item.Quantity;
            }
        }

        // إضافة سجل المرتجع إلى المستودع
        await _unitOfWork.PurchaseReturns.AddAsync(purchaseReturn, ct);
        // حفظ كافة التغييرات وحركات المخزون في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة تحميل سجل المرتجع بكامل تفاصيله
        var createdReturn = await _unitOfWork.PurchaseReturns.FirstOrDefaultAsync(new PurchaseReturnWithDetailsSpec(purchaseReturn.Id), ct) ?? purchaseReturn;
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<PurchaseReturnResponseDto>(createdReturn);

        // إرجاع النتيجة الناجحة مع بيانات المرتجع
        return ServiceResult<PurchaseReturnResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع: التحميل بدون تتبع ينشئ نسخاً متعددة للكيان نفسه عند تكرار
        // الصنف في البنود، ثم يفشل Attach داخل SoftDelete بتضارب المفاتيح
        var purchaseReturn = await _unitOfWork.PurchaseReturns.FirstOrDefaultTrackedAsync(new PurchaseReturnWithDetailsSpec(id), ct);
        // التحقق من وجود سجل المرتجع المطلوب حذفه
        if (purchaseReturn is null)
        {
            // إرجاع خطأ بعدم وجود سجل المرتجع
            return ServiceResult.Failure("مرتجع المشتريات غير موجود", ErrorCodes.PurchaseReturnNotFound);
        }

        // إعادة البضاعة المرتجعة إلى المخزن عند حذف أو إلغاء سجل المرتجع — دفعة أرصدة واحدة
        if (purchaseReturn.Items != null && purchaseReturn.Items.Any())
        {
            // استخراج معرفات الباركودات للبنود
            var barcodeIds = purchaseReturn.Items
                .Where(i => i.ProductBarCodeId.HasValue)
                .Select(i => i.ProductBarCodeId!.Value)
                .Distinct()
                .ToList();

            // إذا وجدت باركودات يتم تحميل أرصدتها وإعادتها للمخزن
            if (barcodeIds.Count > 0)
            {
                // استعلام أرصدة المخزن المتتبعة دفعة واحدة
                var stocksByBarcode = (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => s.WarehouseId == purchaseReturn.WarehouseId && barcodeIds.Contains(s.ProductBarcodeId), ct))
                    .ToDictionary(s => s.ProductBarcodeId);

                // زيادة الأرصدة بالكميات المحذوفة من المرتجع
                foreach (var item in purchaseReturn.Items)
                {
                    // التحقق من وجود الباركود وتحديث الرصيد
                    if (item.ProductBarCodeId.HasValue &&
                        stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                    {
                        // إعادة الكمية إلى المخزن
                        stock.Quantity += (int)item.Quantity;
                    }
                }
            }
        }

        // تنفيذ الحذف المنطقي لسجل المرتجع
        _unitOfWork.PurchaseReturns.SoftDelete(purchaseReturn);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
