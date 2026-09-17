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
/// تنفيذ خدمة فواتير المشتريات وإدارة استلام المخزون حسب تفصيل النكهات واحتساب التكاليف والضرائب.
/// </summary>
public class PurchaseInvoiceService : IPurchaseInvoiceService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة فواتير المشتريات مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للتعامل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل البيانات</param>
    public PurchaseInvoiceService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>> GetPagedAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? supplierId = null,
        Guid? branchId = null,
        Guid? warehouseId = null,
        InvoiceStatus? status = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        // بناء مواصفة فلترة وترقيم فواتير المشتريات وفق المعايير المدخلة
        var spec = new PurchaseInvoiceFilterSpec(
            supplierId, branchId, warehouseId, status, paymentMethod, fromDate, toDate, searchTerm);

        // تنفيذ الاستعلام الصفحي لجلب عناصر الصفحة والعدد الكلي للسجلات
        var (items, totalCount) = await _unitOfWork.PurchaseInvoices.GetPagedAsync(spec, pageNumber, pageSize, ct);
        // تحويل عناصر الصفحة إلى قائمة ملخصات فواتير المشتريات
        var dtos = _mapper.Map<IReadOnlyList<PurchaseInvoiceSummaryDto>>(items);
        // إنشاء كائن النتيجة المصفحة مع حساب أرقام الصفحات
        var pagedResult = PagedResult<PurchaseInvoiceSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة الناجحة المصفحة
        return ServiceResult<PagedResult<PurchaseInvoiceSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // الاستعلام عن فاتورة المشتريات بالمعرف مع تفاصيل البنود وتفريعات النكهات والمورد والفرع
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        // التحقق من وجود الفاتورة في قاعدة البيانات
        if (invoice is null)
        {
            // إرجاع رسالة خطأ بعدم العثور على الفاتورة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // تحويل كيان الفاتورة إلى كائن الاستجابة التفصيلي
        var dto = _mapper.Map<PurchaseInvoiceResponseDto>(invoice);
        // إرجاع النتيجة الناجحة مع بيانات الفاتورة
        return ServiceResult<PurchaseInvoiceResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> CreateAsync(CreatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        // التحقق من وجود المورد المحدد في قاعدة البيانات
        bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId, ct);
        // في حال عدم وجود المورد
        if (!supplierExists)
        {
            // إرجاع خطأ بعدم وجود المورد
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // التحقق من وجود الفرع المسجل
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        // في حال عدم وجود الفرع
        if (!branchExists)
        {
            // إرجاع خطأ بعدم وجود الفرع
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // الاستعلام عن المستودع أو الصالة المستلمة للبضاعة
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        // في حال عدم وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ بعدم وجود المستودع أو الصالة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من عدم تكرار رقم فاتورة المشتريات
        bool numExists = await _unitOfWork.PurchaseInvoices.ExistsAsync(p => p.InvoiceNumber == dto.InvoiceNumber, ct);
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
                invItem.Breakdowns = item.Breakdowns.Select(b => new PurchaseInvoiceItemBreakdown
                {
                    // معرف باركود النكهة
                    ProductBarCodeId = b.ProductBarCodeId,
                    // عدد الكراتين أو الطرود
                    PackageQuantity = b.PackageQuantity,
                    // عدد الوحدات في كل طرد
                    UnitsPerPackage = b.UnitsPerPackage,
                    // احتساب الكمية الإجمالية بالوحدات
                    Quantity = b.Quantity > 0 ? b.Quantity : (b.PackageQuantity * (b.UnitsPerPackage > 0 ? b.UnitsPerPackage : 1)),
                    // سعر الوحدة للنكهة
                    UnitPrice = b.UnitPrice > 0 ? b.UnitPrice : item.UnitPrice
                }).ToList();
            }

            // إرجاع بند الفاتورة المجهز
            return invItem;
        }).ToList();

        // احتساب الإجمالي الفرعي للفاتورة
        invoice.SubTotal = invoice.Items.Sum(i => i.LineTotal);
        // احتساب المبلغ الإجمالي بعد الخصومات والضرائب
        invoice.TotalAmount = Math.Max(0, invoice.SubTotal - invoice.DiscountAmount + invoice.TaxAmount);

        // ضبط المبالغ المدفوعة والمتبقية وفق حالة سداد الفاتورة
        if (invoice.Status == InvoiceStatus.Paid)
        {
            // إذا كانت مدفوعة بالكامل يتم سداد المبلغ الإجمالي وتصفير المتبقي
            invoice.PaidAmount = invoice.TotalAmount;
            invoice.RemainingAmount = 0;
        }
        else
        {
            // احتساب المبلغ المتبقي بناء على المبلغ المدفوع جزئياً
            invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        }

        // إضافة فاتورة المشتريات إلى المستودع
        await _unitOfWork.PurchaseInvoices.AddAsync(invoice, ct);

        // ── زيادة رصيد المخزون فوراً وتحديث أسعار التكلفة عند إنشاء الفاتورة وإضافة البند ──
        // زيادة أرصدة المخزون بالكميات المستلمة في المستودع
        await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: true, ct);
        // تحديث سعر التكلفة ومتوسط السعر للأصناف الواردة في الفاتورة
        await UpdateProductsCostAsync(invoice.Items, ct);

        // حفظ كافة التغييرات وحركات المخزون في معاملة واحدة
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة تحميل الفاتورة بالتفاصيل الكاملة لبناء DTO الاستجابة
        var createdInvoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(invoice.Id), ct) ?? invoice;
        // تحويل الكيان إلى كائن الاستجابة المنقول
        var responseDto = _mapper.Map<PurchaseInvoiceResponseDto>(createdInvoice);

        // إرجاع النتيجة الناجحة مع بيانات الفاتورة المنشأة
        return ServiceResult<PurchaseInvoiceResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseInvoiceResponseDto>> UpdateAsync(Guid id, UpdatePurchaseInvoiceDto dto, CancellationToken ct = default)
    {
        // استعلام فاتورة المشتريات بالمعرف مع بنودها وتفصيلاتها
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ بعدم العثور على الفاتورة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // قاعدة قفل التعديل: لا يُسمح بتعديل أي فاتورة مشتريات مغلقة أو مدفوعة ومرحلة للمخازن
        if (invoice.Status == InvoiceStatus.Paid)
        {
            // إرجاع خطأ بمنع تعديل الفاتورة المغلقة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة مشتريات مغلقة ومرحلة للمخازن", ErrorCodes.ValidationError);
        }

        // منع تعديل الفواتير الملغاة أو الباطلة
        if (invoice.Status == InvoiceStatus.Cancelled || invoice.Status == InvoiceStatus.Voided)
        {
            // إرجاع خطأ بعدم جواز تعديل فاتورة ملغاة
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("لا يمكن تعديل فاتورة مشتريات ملغاة أو باطلة", ErrorCodes.PurchaseOrderInvalidStatus);
        }

        // التحقق من وجود المستودع الجديد
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);
        // في حال عدم وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ بعدم وجود المستودع
            return ServiceResult<PurchaseInvoiceResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // ── المزامنة التراكمية للبند الجديد المضاف فقط دون المساس بالبنود السابقة ──
        if (dto.Items != null)
        {
            // عدد البنود السابقة الموجودة حالياً
            int existingCount = invoice.Items?.Count ?? 0;
            // إجمالي عدد البنود الجديد بعد التعديل
            int newTotalCount = dto.Items.Count;

            // 1. إضافة البنود الجديدة فقط (إن تم إدراج بند جديد)
            if (newTotalCount > existingCount)
            {
                // قائمة تجميع البنود المضافة حديثاً
                var newlyAddedItems = new List<PurchaseInvoiceItem>();

                // المرور على البنود الجديدة بدءاً من فهرس آخر بند قديم
                for (int i = existingCount; i < newTotalCount; i++)
                {
                    // جلب DTO البند الجديد
                    var itemDto = dto.Items[i];
                    // بناء كيان البند الجديد
                    var invItem = new PurchaseInvoiceItem
                    {
                        // ربط معرف الفاتورة
                        PurchaseInvoiceId = invoice.Id,
                        // معرف الصنف
                        ProductId = itemDto.ProductId,
                        // معرف الباركود
                        ProductBarCodeId = itemDto.ProductBarCodeId,
                        // الكمية
                        Quantity = itemDto.Quantity,
                        // سعر الوحدة
                        UnitPrice = itemDto.UnitPrice,
                        // الخصم
                        DiscountAmount = itemDto.DiscountAmount,
                        // إجمالي السطر
                        LineTotal = Math.Max(0, (itemDto.Quantity * itemDto.UnitPrice) - itemDto.DiscountAmount)
                    };

                    // إضافة تفريعات النكهات للبند الجديد إن وُجدت
                    if (itemDto.Breakdowns != null && itemDto.Breakdowns.Any())
                    {
                        // تحويل تفصيلات النكهات إلى كيانات
                        invItem.Breakdowns = itemDto.Breakdowns.Select(b => new PurchaseInvoiceItemBreakdown
                        {
                            // معرف باركود النكهة
                            ProductBarCodeId = b.ProductBarCodeId,
                            // عدد الطرود
                            PackageQuantity = b.PackageQuantity,
                            // الوحدات لكل طرد
                            UnitsPerPackage = b.UnitsPerPackage,
                            // الكمية المحسوبة
                            Quantity = b.Quantity > 0 ? b.Quantity : (b.PackageQuantity * (b.UnitsPerPackage > 0 ? b.UnitsPerPackage : 1)),
                            // السعر
                            UnitPrice = b.UnitPrice > 0 ? b.UnitPrice : itemDto.UnitPrice
                        }).ToList();
                    }

                    // إضافة البند لقائمة البنود الجديدة
                    newlyAddedItems.Add(invItem);
                    // إضافة البند لقاعدة البيانات
                    await _unitOfWork.PurchaseInvoiceItems.AddAsync(invItem, ct);
                }

                // زيادة المخزون وتحديث أسعار التكلفة للبند/البنود الجديدة المضافة فقط!
                await AdjustInvoiceItemsStockAsync(newlyAddedItems, dto.WarehouseId, isIncrement: true, ct);
                // تحديث سعر التكلفة للأصناف المضافة حديثاً
                await UpdateProductsCostAsync(newlyAddedItems, ct);
            }
            // 2. معالجة حذف بند في حال قام المستخدم بحذف بند من الواجهة
            else if (newTotalCount < existingCount && invoice.Items != null)
            {
                // حساب عدد البنود المحذوفة
                var removedCount = existingCount - newTotalCount;
                // استخراج البنود المطلوب حذفها
                var itemsToRemove = invoice.Items.Skip(newTotalCount).Take(removedCount).ToList();

                // عكس المخزون للبند المحذوف بإنقاص رصيده من المستودع
                await AdjustInvoiceItemsStockAsync(itemsToRemove, invoice.WarehouseId, isIncrement: false, ct);

                // حذف تفصيلات البنود المحذوفة دفعة واحدة (كيانات متتبعة) بدل استعلام لكل بند وكل تفصيلة
                var removedBreakdownIds = itemsToRemove
                    .Where(i => i.Breakdowns != null)
                    .SelectMany(i => i.Breakdowns!)
                    .Select(b => b.Id)
                    .ToList();
                // استخراج معرفات البنود المحذوفة
                var removedItemIds = itemsToRemove.Select(i => i.Id).ToList();

                // استعلام الكيانات المتتبعة لتفريعات النكهات المحذوفة
                var trackedBreakdowns = await _unitOfWork.PurchaseInvoiceItemBreakdowns.FindTrackedAsync(
                    b => removedBreakdownIds.Contains(b.Id), ct);
                // حذف كل تفريعة من قاعدة البيانات
                foreach (var trackedBd in trackedBreakdowns)
                {
                    // حذف صلب لتفريعة النكهة
                    _unitOfWork.PurchaseInvoiceItemBreakdowns.HardDelete(trackedBd);
                }

                // استعلام الكيانات المتتبعة لبنود الفاتورة المحذوفة
                var trackedItems = await _unitOfWork.PurchaseInvoiceItems.FindTrackedAsync(
                    i => removedItemIds.Contains(i.Id), ct);
                // حذف كل بند من المستودع
                foreach (var trackedItem in trackedItems)
                {
                    // حذف صلب لبند الفاتورة
                    _unitOfWork.PurchaseInvoiceItems.HardDelete(trackedItem);
                }
            }
        }
        else if (dto.WarehouseId != invoice.WarehouseId && invoice.Items != null && invoice.Items.Any())
        {
            // إذا تغير المستودع فقط، ننقل المخزون من المستودع القديم إلى الجديد
            // خصم الكميات من المستودع القديم
            await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: false, ct);
            // إضافة الكميات إلى المستودع الجديد
            await AdjustInvoiceItemsStockAsync(invoice.Items, dto.WarehouseId, isIncrement: true, ct);
        }

        // تحديث الخصائص الأساسية للفاتورة
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
        invoice.SubTotal = dto.Items?.Sum(i => Math.Max(0, (i.Quantity * i.UnitPrice) - i.DiscountAmount)) ?? invoice.SubTotal;
        // احتساب الإجمالي النهائي
        invoice.TotalAmount = Math.Max(0, invoice.SubTotal - invoice.DiscountAmount + invoice.TaxAmount);

        // فحص ما إذا كان الطلب يتضمن إغلاق الفاتورة (التحويل إلى Paid)
        bool isClosing = dto.Status == InvoiceStatus.Paid;
        // تعيين الحالة الجديدة
        invoice.Status = dto.Status;

        // ضبط المدفوع والمتبقي عند الإغلاق أو الدفع الجزئي
        if (isClosing)
        {
            // في حالة الإغلاق يتم دفع كامل المبلغ وتصفير المتبقي
            invoice.PaidAmount = invoice.TotalAmount;
            invoice.RemainingAmount = 0;
        }
        else
        {
            // تحديث المبلغ المدفوع واحتساب المتبقي
            invoice.PaidAmount = dto.PaidAmount;
            invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);
        }

        // تفريغ كائنات الربط المنفصلة لتجنب أي تعارض في تتبع الكيانات
        invoice.Items = null!;
        invoice.Supplier = null!;
        invoice.Branch = null!;
        invoice.Warehouse = null!;
        invoice.PurchaseOrder = null;

        // تحديث الفاتورة في المستودع
        _unitOfWork.PurchaseInvoices.Update(invoice);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب الفاتورة المحدثة مع تفاصيلها لبناء كائن الاستجابة
        var updatedInvoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct) ?? invoice;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<PurchaseInvoiceResponseDto>(updatedInvoice);

        // إرجاع النتيجة الناجحة
        return ServiceResult<PurchaseInvoiceResponseDto>.Success(responseDto);
    }

    /// <summary>
    /// تطبيق أو عكس تأثير بنود الفاتورة على المخزون (في المستودع أو الصالة) مع دعم تفصيلات النكهات والباركودات.
    /// تُجلب أرصدة المستودع المعنية دفعة واحدة ثم تُعدَّل في الذاكرة (معالجة نمط N+1).
    /// </summary>
    /// <param name="items">قائمة بنود الفاتورة المعنية بالتعديل</param>
    /// <param name="warehouseId">معرف المستودع أو الصالة المستهدفة</param>
    /// <param name="isIncrement">تحديد ما إذا كانت الحركة إضافة للمخزون (true) أو خصم (false)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    private async Task AdjustInvoiceItemsStockAsync(IEnumerable<PurchaseInvoiceItem> items, Guid warehouseId, bool isIncrement, CancellationToken ct)
    {
        // استعلام المستودع للتعرف على نوعه
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(warehouseId, ct);
        // في حال عدم وجود المستودع يتم إنهاء الدالة
        if (warehouse == null) return;

        // تحويل البنود إلى قائمة محلية
        var itemsList = items.ToList();
        // تحديد إشارة الحركة المخزنية (+1 للإضافة، -1 للخصم)
        int sign = isIncrement ? 1 : -1;

        // معالجة المخزون في المخزن الرئيسي (مستودع تخزين Storge - بنية نكهات وباركودات)
        if (warehouse.Type == WarehouseType.Storge)
        {
            // جمع كل الأصناف التي تحتاج استنباط الباركود الافتراضي لها
            var fallbackProductIds = itemsList
                .Where(i => (i.Breakdowns == null || !i.Breakdowns.Any()) &&
                            (!i.ProductBarCodeId.HasValue || i.ProductBarCodeId.Value == Guid.Empty))
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            // جلب الباركودات الافتراضية دفعة واحدة لتفادي استعلامات N+1
            var defaultBarcodeByProduct = fallbackProductIds.Count > 0
                ? (await _unitOfWork.ProductBarCodes.FindAsync(
                    b => fallbackProductIds.Contains(b.ProductId), ct))
                    .GroupBy(b => b.ProductId)
                    .ToDictionary(g => g.Key, g => g.First())
                : new Dictionary<Guid, ProductBarCode>();

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
            var stocksByBarcode = barcodeIdList.Count > 0
                ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && barcodeIdList.Contains(s.ProductBarcodeId), ct))
                    .ToDictionary(s => s.ProductBarcodeId)
                : new Dictionary<Guid, StorgeStock>();

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
                            stock.Quantity += sign * (int)bd.Quantity;
                        }
                        // في حال كانت العملية إضافة رصيد ولم يكن الرصيد موجوداً مسبقاً
                        else if (isIncrement)
                        {
                            // إنشاء سجل رصيد جديد للنكهة
                            var newStock = new StorgeStock
                            {
                                WarehouseId = warehouse.Id,
                                ProductBarcodeId = bd.ProductBarCodeId,
                                Quantity = (int)bd.Quantity,
                                MinStockLevel = 0
                            };
                            // إضافة السجل للمستودع
                            await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
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
                    if (!barcodeId.HasValue || barcodeId.Value == Guid.Empty) continue;

                    // فحص وجود رصيد مسجل مسبقاً لهذا الباركود
                    if (stocksByBarcode.TryGetValue(barcodeId.Value, out var stock))
                    {
                        // تعديل الرصيد
                        stock.Quantity += sign * (int)item.Quantity;
                    }
                    // إذا كانت حركة إضافة ورصيد جديد
                    else if (isIncrement)
                    {
                        // إنشاء سجل رصيد مخزني جديد
                        var newStock = new StorgeStock
                        {
                            WarehouseId = warehouse.Id,
                            ProductBarcodeId = barcodeId.Value,
                            Quantity = (int)item.Quantity,
                            MinStockLevel = 0
                        };
                        // إضافة السجل الجديد
                        await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
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
            var stocksByProduct = productIds.Count > 0
                ? (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                    .ToDictionary(s => s.ProductId)
                : new Dictionary<Guid, ShowroomStock>();

            // المرور على بنود الفاتورة وتحديث أرصدة صالة العرض
            foreach (var item in itemsList)
            {
                // إذا كان للصنف رصيد مسجل مسبقاً
                if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                {
                    // تعديل كمية الرصيد
                    stock.Quantity += sign * (int)item.Quantity;
                }
                // إذا كانت حركة إضافة ورصيد جديد في الصالة
                else if (isIncrement)
                {
                    // إنشاء سجل رصيد جديد في صالة العرض
                    var newStock = new ShowroomStock
                    {
                        WarehouseId = warehouse.Id,
                        ProductId = item.ProductId,
                        Quantity = (int)item.Quantity,
                        MinStockLevel = 0
                    };
                    // إضافة سجل رصيد الصالة
                    await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                    // حفظ السجل في القاموس
                    stocksByProduct[item.ProductId] = newStock;
                }
            }
        }
    }

    /// <summary>
    /// تحديث آخر سعر شراء (CostPrice) ومتوسط التكلفة (AveragePrice) للأصناف — جلب المنتجات المعنية دفعة واحدة.
    /// </summary>
    /// <param name="items">بنود الفاتورة التي تحتوي على أسعار الشراء الجديدة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    private async Task UpdateProductsCostAsync(IEnumerable<PurchaseInvoiceItem> items, CancellationToken ct)
    {
        // تحويل البنود إلى قائمة
        var itemsList = items.ToList();
        // استخراج معرفات الأصناف الفريدة
        var productIds = itemsList.Select(i => i.ProductId).Distinct().ToList();

        // استعلام الأصناف المتتبعة دفعة واحدة لتحديث أسعارها
        var productsById = productIds.Count > 0
            ? (await _unitOfWork.Products.FindTrackedAsync(p => productIds.Contains(p.Id), ct))
                .ToDictionary(p => p.Id)
            : new Dictionary<Guid, Product>();

        // المرور على البنود وتحديث التكاليف
        foreach (var item in itemsList)
        {
            // التحقق من وجود الصنف في القاموس
            if (productsById.TryGetValue(item.ProductId, out var product))
            {
                // تعيين سعر التكلفة الأخير بسعر الوحدة في الفاتورة
                product.CostPrice = item.UnitPrice;
                // إذا لم يكن للصنف متوسط سعر شراء سابق
                if (product.AveragePrice <= 0)
                {
                    // تعيين متوسط السعر مساوياً لسعر الشراء الحالي
                    product.AveragePrice = item.UnitPrice;
                }
                else
                {
                    // احتساب متوسط التكلفة المرجح البسيط وتقريبه لأربعة منازل عشرية
                    product.AveragePrice = Math.Round((product.AveragePrice + item.UnitPrice) / 2m, 4);
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult> CancelAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام فاتورة المشتريات بالمعرف مع تفاصيل البنود
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ بعدم وجود الفاتورة
            return ServiceResult.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // منع إلغاء فاتورة ملغاة مسبقاً
        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            // إرجاع خطأ بكون الفاتورة ملغاة بالفعل
            return ServiceResult.Failure("الفاتورة ملغاة بالفعل", ErrorCodes.PurchaseOrderInvalidStatus);
        }

        // عكس تأثير المخزون بخصم الكميات التي كانت قد أضيفت بموجب الفاتورة
        if (invoice.Items != null && invoice.Items.Any())
        {
            // خصم بنود الفاتورة من المخزن المعني
            await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: false, ct);
        }

        // تحديث حالة الفاتورة إلى ملغاة
        invoice.Status = InvoiceStatus.Cancelled;
        // تعليم الفاتورة كمحدثة
        _unitOfWork.PurchaseInvoices.Update(invoice);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع لتفادي تضارب النسخ المكررة عند تكرار الصنف في البنود أثناء SoftDelete
        var invoice = await _unitOfWork.PurchaseInvoices.FirstOrDefaultTrackedAsync(new PurchaseInvoiceWithDetailsSpec(id), ct);
        // التحقق من وجود الفاتورة
        if (invoice is null)
        {
            // إرجاع خطأ بعدم العثور على الفاتورة
            return ServiceResult.Failure("فاتورة المشتريات غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // منع حذف فاتورة لها مرتجعات مرتبطة — يلزم حذف المرتجعات أولاً
        bool hasReturns = await _unitOfWork.PurchaseReturns.ExistsAsync(r => r.PurchaseInvoiceId == id, ct);
        // في حال وجود مرتجعات مرتبطة
        if (hasReturns)
        {
            // إرجاع خطأ منع الحذف
            return ServiceResult.Failure("لا يمكن حذف فاتورة مشتريات لها مرتجعات مرتبطة — احذف المرتجعات أولاً", ErrorCodes.ValidationError);
        }

        // عكس المخزون في حال لم تكن الفاتورة ملغاة مسبقاً
        if (invoice.Status != InvoiceStatus.Cancelled && invoice.Items != null && invoice.Items.Any())
        {
            // خصم الكميات من المستودع
            await AdjustInvoiceItemsStockAsync(invoice.Items, invoice.WarehouseId, isIncrement: false, ct);
        }

        // تنفيذ الحذف المنطقي للفاتورة وبنودها
        _unitOfWork.PurchaseInvoices.SoftDelete(invoice);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
