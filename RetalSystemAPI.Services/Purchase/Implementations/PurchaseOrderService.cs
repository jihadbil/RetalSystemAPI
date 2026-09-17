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
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Purchase.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة أوامر الشراء وحساب إجماليات وتفاصيل الأصناف وتغيير الحالات وتغذية المخزون وتوليد الفواتير آلياً.
/// </summary>
public class PurchaseOrderService : IPurchaseOrderService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // المحول لرسم الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة أوامر الشراء مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للتعامل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل الكائنات</param>
    public PurchaseOrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول البيانات
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseOrderResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // الاستعلام عن أمر الشراء بالمعرف مع تضمين الفرع والمستودع والمورد وتفاصيل الأصناف والباركود
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        // التحقق من وجود أمر الشراء في قاعدة البيانات
        if (order is null)
        {
            // إرجاع نتيجة فشل تفيد بعدم العثور على أمر الشراء
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // تحويل كيان أمر الشراء إلى كائن الاستجابة المنقول
        var dto = _mapper.Map<PurchaseOrderResponseDto>(order);
        // إرجاع نتيجة نجاح محملة ببيانات أمر الشراء
        return ServiceResult<PurchaseOrderResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام خفيفة لقائمة أوامر الشراء بحسب الفلاتر
        var spec = new PurchaseOrderListSpec(branchId, warehouseId, status);
        // جلب قائمة أوامر الشراء المطابقة للمواصفة
        var orders = await _unitOfWork.PurchaseOrders.FindAsync(spec, ct);
        // تحويل قائمة الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<PurchaseOrderSummaryDto>>(orders);

        // إرجاع النتيجة الناجحة مع قائمة الملخصات
        return ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, string? search = null, CancellationToken ct = default)
    {
        // بناء مواصفة الاستعلام المصفاة مع البحث بالرقم
        var spec = new PurchaseOrderListSpec(branchId, warehouseId, status, search);
        // تنفيذ الاستعلام الصفحي لجلب عناصر الصفحة الحالية وإجمالي عدد السجلات
        var (items, totalCount) = await _unitOfWork.PurchaseOrders.GetPagedAsync(spec, pageNumber, pageSize, ct);

        // تحويل عناصر الصفحة الحالية إلى قائمة ملخصات
        var dtos = _mapper.Map<IReadOnlyList<PurchaseOrderSummaryDto>>(items);
        // إنشاء كائن النتيجة المصفحة مع حساب إجمالي الصفحات
        var pagedResult = PagedResult<PurchaseOrderSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة الناجحة بالبيانات المصفحة
        return ServiceResult<PagedResult<PurchaseOrderSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        // التحقق من صحة وجود الفرع المسند لأمر الشراء
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);
        // في حال عدم وجود الفرع
        if (!branchExists)
        {
            // إرجاع رسالة خطأ بعدم وجود الفرع
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // التحقق من صحة المورد إن تم تحديده في الطلبية
        if (dto.SupplierId.HasValue && dto.SupplierId.Value != Guid.Empty)
        {
            // التحقق من وجود المورد في قاعدة البيانات
            bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId.Value, ct);
            // في حال عدم وجود المورد
            if (!supplierExists)
            {
                // إرجاع رسالة خطأ بعدم وجود المورد
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
            }
        }

        // التحقق من صحة المستودع المستهدف إن تم تحديده
        if (dto.WarehouseId.HasValue)
        {
            // التحقق من وجود المستودع في قاعدة البيانات
            bool warehouseExists = await _unitOfWork.Warehouses.ExistsAsync(w => w.Id == dto.WarehouseId.Value, ct);
            // في حال عدم وجود المستودع
            if (!warehouseExists)
            {
                // إرجاع خطأ بعدم وجود المخزن
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المخزن المحدد غير موجود", ErrorCodes.WarehouseNotFound);
            }
        }

        // التحقق من عدم تكرار رقم أمر الشراء
        bool orderNumExists = await _unitOfWork.PurchaseOrders.ExistsAsync(p => p.OrderNumber == dto.OrderNumber, ct);
        // في حال وجود رقم الطلبية مسبقاً
        if (orderNumExists)
        {
            // إرجاع خطأ تكرار رقم الطلبية
            return ServiceResult<PurchaseOrderResponseDto>.Failure("رقم الطلبية مستخدم بالفعل", ErrorCodes.PurchaseOrderNumberExists);
        }

        // تحويل بيانات الإدخال إلى كيان أمر الشراء
        var order = _mapper.Map<PurchaseOrder>(dto);
        // ضبط معرف المورد مع التأكد من تفادي المعرفات الفارغة
        order.SupplierId = (dto.SupplierId.HasValue && dto.SupplierId.Value != Guid.Empty) ? dto.SupplierId.Value : null;
        // تعيين تاريخ الطلبية الحالي إذا لم يُحدد تاريخ مسبق
        order.OrderDate = dto.OrderDate == default ? DateTime.UtcNow : dto.OrderDate;
        // تعيين الحالة المبدئية كمسودة
        order.Status = PurchaseOrderStatus.Draft;

        // معالجة بنود الطلبية واحتساب الإجماليات
        if (dto.Items != null && dto.Items.Any())
        {
            // تحويل بنود الإدخال إلى كيانات PurchaseOrderItem
            order.Items = dto.Items.Select(item => new PurchaseOrderItem
            {
                // ربط معرف باركود الصنف
                ProductBarCodeId = item.ProductBarCodeId,
                // الكمية المطلوبة
                Quantity = item.Quantity,
                // سعر الوحدة التقديري
                UnitPrice = item.UnitPrice,
                // إجمالي البند (الكمية × السعر)
                LineTotal = item.Quantity * item.UnitPrice
            }).ToList();

            // احتساب إجمالي أمر الشراء كمجموع إجماليات البنود
            order.TotalAmount = order.Items.Sum(i => i.LineTotal);
        }
        else
        {
            // تصفير إجمالي أمر الشراء لعدم وجود بنود
            order.TotalAmount = 0;
        }

        // إضافة أمر الشراء الجديد إلى المستودع
        await _unitOfWork.PurchaseOrders.AddAsync(order, ct);
        // حفظ التغييرات في قاعدة البيانات لتوليد المعرف
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب أمر الشراء مع العلاقات الكاملة للتأكد من اكتمال كائن الاستجابة
        var createdOrder = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(order.Id), ct) ?? order;
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(createdOrder);

        // إرجاع النتيجة الناجحة مع بيانات أمر الشراء المنشأ
        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseOrderResponseDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        // استعلام أمر الشراء المطلوب تعديله مع كافة تفاصيله
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        // التحقق من وجود أمر الشراء
        if (order is null)
        {
            // إرجاع فشل في حال عدم وجود الطلبية
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // منع تعديل أمر شراء تم استلامه وإغلاقه نهائياً
        if (order.Status == PurchaseOrderStatus.Received)
        {
            // إرجاع خطأ بعدم إمكانية تعديل طلبية مستلمة
            return ServiceResult<PurchaseOrderResponseDto>.Failure("لا يمكن تعديل طلبية تم استلامها وإغلاقها بالفعل", ErrorCodes.ValidationError);
        }

        // التحقق من المورد الجديد إن تم تمريره
        if (dto.SupplierId.HasValue)
        {
            // إذا لم يكن المعرف فارغاً يتم التحقق من وجوده
            if (dto.SupplierId.Value != Guid.Empty)
            {
                // الاستعلام عن وجود المورد
                bool supplierExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Id == dto.SupplierId.Value, ct);
                // في حال عدم وجود المورد
                if (!supplierExists)
                {
                    // إرجاع خطأ عدم وجود المورد
                    return ServiceResult<PurchaseOrderResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
                }
            }
            // تحديث معرف المورد أو تفريغه
            order.SupplierId = dto.SupplierId.Value == Guid.Empty ? null : dto.SupplierId;
        }

        // التحقق من المستودع الجديد إن تم تمريره
        if (dto.WarehouseId.HasValue)
        {
            // الاستعلام عن وجود المستودع
            bool warehouseExists = await _unitOfWork.Warehouses.ExistsAsync(w => w.Id == dto.WarehouseId.Value, ct);
            // في حال عدم وجود المستودع
            if (!warehouseExists)
            {
                // إرجاع خطأ عدم وجود المخزن
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المخزن المحدد غير موجود", ErrorCodes.WarehouseNotFound);
            }
        }

        // تحديث معرف المستودع
        order.WarehouseId = dto.WarehouseId;
        // تحديث التاريخ المتوقع لوصول البضاعة
        order.ExpectedDate = dto.ExpectedDate;

        // تحديث بنود أمر الشراء في حال تم إرسالها
        if (dto.Items != null)
        {
            // حذف البنود القديمة المسجلة حالياً
            if (order.Items != null && order.Items.Any())
            {
                // المرور على كافة البنود القديمة وحذفها نهائياً
                foreach (var existingItem in order.Items.ToList())
                {
                    // الحذف الصلب للبند من قاعدة البيانات
                    _unitOfWork.PurchaseOrderItems.HardDelete(existingItem);
                }
            }

            // إنشاء قائمة البنود الجديدة من بيانات الإدخال
            order.Items = dto.Items.Select(item => new PurchaseOrderItem
            {
                // ربط البند بأمر الشراء
                PurchaseOrderId = id,
                // معرف باركود الصنف
                ProductBarCodeId = item.ProductBarCodeId,
                // الكمية المطلوبة
                Quantity = item.Quantity,
                // سعر الوحدة
                UnitPrice = item.UnitPrice,
                // إجمالي البند
                LineTotal = item.Quantity * item.UnitPrice
            }).ToList();

            // إعادة احتساب إجمالي مبلغ أمر الشراء
            order.TotalAmount = order.Items.Sum(i => i.LineTotal);
        }

        // تحديث أمر الشراء في المستودع
        _unitOfWork.PurchaseOrders.Update(order);
        // حفظ التعديلات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب أمر الشراء المحدث بكامل تفاصيله
        var updatedOrder = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct) ?? order;
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(updatedOrder);

        // إرجاع النتيجة الناجحة
        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PurchaseOrderResponseDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default)
    {
        // استعلام أمر الشراء بكامل تفاصيله وبنوده
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        // التحقق من وجود أمر الشراء
        if (order is null)
        {
            // إرجاع خطأ بعدم العثور على الطلبية
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // الحماية الصارمة من تكرار الاستلام أو التلاعب بالطلبيات المغلقة
        if (order.Status == PurchaseOrderStatus.Received)
        {
            // منع إعادة استلام أو تعديل طلبية تم استلامها وإغلاقها مسبقاً
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الطلبية مستلمة ومغلقة نهائياً، لا يمكن تعديل حالتها مجدداً منعاً لتكرار المخزون", ErrorCodes.ValidationError);
        }

        // منع تغيير حالة طلبية تم إلغاؤها بالفعل
        if (order.Status == PurchaseOrderStatus.Cancelled)
        {
            // إرجاع خطأ بعدم جواز إعادة تفعيل طلبية ملغاة
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الطلبية ملغاة ولا يمكن إعادة تفعيلها", ErrorCodes.ValidationError);
        }

        // فحص ما إذا كان التعديل يمثل استلاماً جديداً للبضاعة
        bool isNewlyReceived = (status == PurchaseOrderStatus.Received);

        // التحقق من اكتمال البيانات الإلزامية في حال الاستلام
        if (isNewlyReceived)
        {
            // التأكد من تحديد المورد قبل إتمام الاستلام لإنشاء الفاتورة
            if (!order.SupplierId.HasValue || order.SupplierId.Value == Guid.Empty)
            {
                // إرجاع خطأ طلب تحديد المورد
                return ServiceResult<PurchaseOrderResponseDto>.Failure("يجب تحديد المورد أولاً قبل إتمام استلام الطلبية وتوليد فاتورتها", ErrorCodes.ValidationError);
            }

            // التأكد من تحديد المستودع أو الصالة المستلمة
            if (!order.WarehouseId.HasValue || order.WarehouseId.Value == Guid.Empty)
            {
                // إرجاع خطأ طلب تحديد المستودع
                return ServiceResult<PurchaseOrderResponseDto>.Failure("يجب تحديد المستودع أو الصالة المستلمة أولاً قبل إتمام استلام الطلبية", ErrorCodes.ValidationError);
            }
        }

        // جلب نسخة الكيان المتتبعة لتعديل الحالة
        var orderToUpdate = await _unitOfWork.PurchaseOrders.GetByIdAsync(id, ct);
        // التحقق من وجود الكيان المتتبع
        if (orderToUpdate is null)
        {
            // إرجاع خطأ في حال عدم وجود الطلبية
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // تحديث حالة أمر الشراء إلى الحالة الجديدة
        orderToUpdate.Status = status;
        // تعليم الكيان كمحدث في المستودع
        _unitOfWork.PurchaseOrders.Update(orderToUpdate);

        // خريطة استنباط ProductId من الباركود دفعة واحدة — تخدم تحديث المخزون وتوليد الفاتورة معاً
        var receivedProductIds = new Dictionary<Guid, Guid>();

        // تنفيذ منطق تغذية المخزون وتوليد الفاتورة عند تحويل الحالة إلى مستلم (Received)
        if (isNewlyReceived && order.WarehouseId.HasValue && order.Items != null && order.Items.Count > 0)
        {
            // جلب بيانات المستودع لمعرفة نوعه (مخزن تخزين رئيسي أم صالة عرض)
            var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(order.WarehouseId.Value, ct);
            // التأكد من وجود المستودع
            if (warehouse != null)
            {
                // إذا كان المستودع مخزن تخزين رئيسي (Storge)
                if (warehouse.Type == WarehouseType.Storge)
                {
                    // استخراج البنود ذات الباركود الصالح
                    var validItems = order.Items.Where(i => i.ProductBarCodeId != Guid.Empty).ToList();
                    // تجميع معرفات الباركود الفريدة دفعة واحدة لتفادي مشكلة N+1
                    var barcodeIds = validItems.Select(i => i.ProductBarCodeId).Distinct().ToList();

                    // جلب كيانات الباركود دفعة واحدة وبناء قاموس البحث السريع بالمعرف
                    var barcodesById = barcodeIds.Count > 0
                        ? (await _unitOfWork.ProductBarCodes.FindAsync(b => barcodeIds.Contains(b.Id), ct))
                            .ToDictionary(b => b.Id)
                        : new Dictionary<Guid, ProductBarCode>();

                    // جلب أرصدة المخزن المتتبعة للباركودات المعنية دفعة واحدة
                    var stocksByBarcode = barcodeIds.Count > 0
                        ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                            s => s.WarehouseId == order.WarehouseId.Value && barcodeIds.Contains(s.ProductBarcodeId), ct))
                            .ToDictionary(s => s.ProductBarcodeId)
                        : new Dictionary<Guid, StorgeStock>();

                    // المرور على بنود الطلبية وتحديث رصيد المخزن لكل صنف ونكهة
                    foreach (var item in validItems)
                    {
                        // فحص وجود سجل رصيد مخزني مسبق لهذا الباركود في هذا المخزن
                        if (stocksByBarcode.TryGetValue(item.ProductBarCodeId, out var stock))
                        {
                            // زيادة الكمية المستلمة إلى الرصيد الحالي
                            stock.Quantity += (int)item.Quantity;
                        }
                        else
                        {
                            // إنشاء سجل رصيد مخزني جديد للباركود المستلم
                            var newStock = new StorgeStock
                            {
                                // تعيين معرف المستأجر
                                TenantId = order.TenantId,
                                // تعيين معرف المخزن
                                WarehouseId = order.WarehouseId.Value,
                                // تعيين معرف الباركود
                                ProductBarcodeId = item.ProductBarCodeId,
                                // تعيين الكمية المستلمة
                                Quantity = (int)item.Quantity,
                                // ضبط الحد الأدنى الافتراضي للرصيد
                                MinStockLevel = 0
                            };
                            // إضافة سجل الرصيد المخزني الجديد للمستودع
                            await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                            // حفظ السجل الجديد في القاموس لمنع التكرار في نفس الدفعة
                            stocksByBarcode[item.ProductBarCodeId] = newStock;
                        }
                    }

                    // استنباط ProductId للبنود التي تحتاجه (للفاتورة المولدة أدناه) من نفس دفعة الباركودات
                    foreach (var item in validItems)
                    {
                        // إضافة معرف الصنف إلى الخريطة للاستخدام لاحقاً
                        receivedProductIds.TryAdd(item.ProductBarCodeId,
                            barcodesById.TryGetValue(item.ProductBarCodeId, out var bc) ? bc.ProductId : Guid.Empty);
                    }
                }
                // إذا كان المستودع عبارة عن صالة عرض (Showroom)
                else if (warehouse.Type == WarehouseType.Show)
                {
                    // استخراج البنود ذات الباركود الصالح
                    var validItems = order.Items.Where(i => i.ProductBarCodeId != Guid.Empty).ToList();

                    // استنباط الباركودات دفعة واحدة
                    var barcodeIds = validItems.Select(i => i.ProductBarCodeId).Distinct().ToList();
                    // جلب الباركودات وبناء القاموس
                    var barcodesById = barcodeIds.Count > 0
                        ? (await _unitOfWork.ProductBarCodes.FindAsync(b => barcodeIds.Contains(b.Id), ct))
                            .ToDictionary(b => b.Id)
                        : new Dictionary<Guid, ProductBarCode>();

                    // استخراج معرفات الأصناف الفريدة لصالة العرض
                    var productIds = validItems
                        .Select(i => barcodesById.TryGetValue(i.ProductBarCodeId, out var bc) ? bc.ProductId : Guid.Empty)
                        .Where(pid => pid != Guid.Empty)
                        .Distinct()
                        .ToList();

                    // جلب أرصدة صالة العرض المتتبعة دفعة واحدة للأصناف المعنية
                    var stocksByProduct = productIds.Count > 0
                        ? (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                            s => s.WarehouseId == order.WarehouseId.Value && productIds.Contains(s.ProductId), ct))
                            .ToDictionary(s => s.ProductId)
                        : new Dictionary<Guid, ShowroomStock>();

                    // المرور على البنود وتحديث رصيد صالة العرض
                    foreach (var item in validItems)
                    {
                        // تخطي البند في حال تعذر الحصول على معرف الصنف
                        if (!barcodesById.TryGetValue(item.ProductBarCodeId, out var bc) || bc.ProductId == Guid.Empty) continue;

                        // إذا كان للصنف رصيد مسبق في الصالة
                        if (stocksByProduct.TryGetValue(bc.ProductId, out var stock))
                        {
                            // زيادة رصيد الصالة بالكمية المستلمة
                            stock.Quantity += (int)item.Quantity;
                        }
                        else
                        {
                            // إنشاء رصيد جديد في صالة العرض لهذا الصنف
                            var newStock = new ShowroomStock
                            {
                                // تعيين معرف المستأجر
                                TenantId = order.TenantId,
                                // تعيين معرف الصالة
                                WarehouseId = order.WarehouseId.Value,
                                // تعيين معرف الصنف
                                ProductId = bc.ProductId,
                                // تعيين الكمية
                                Quantity = (int)item.Quantity,
                                // ضبط الحد الأدنى
                                MinStockLevel = 0
                            };
                            // إضافة سجل رصيد الصالة
                            await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                            // حفظ السجل في القاموس
                            stocksByProduct[bc.ProductId] = newStock;
                        }
                    }

                    // استنباط ProductId للفاتورة المولدة
                    foreach (var item in validItems)
                    {
                        // فحص الباركود وإضافته لخريطة الأصناف
                        if (barcodesById.TryGetValue(item.ProductBarCodeId, out var bc))
                        {
                            // تخزين معرف الصنف
                            receivedProductIds.TryAdd(item.ProductBarCodeId, bc.ProductId);
                        }
                    }
                }
            }

            // توليد فاتورة مشتريات معتمدة آلياً لتوثيق استلام الطلبية
            var invoiceNumber = $"PINV-{DateTime.UtcNow:yyyyMMddHHmmss}";
            // بناء كيان فاتورة المشتريات الآلية
            var purchaseInvoice = new PurchaseInvoice
            {
                // تعيين معرف المستأجر
                TenantId = order.TenantId,
                // تعيين رقم الفاتورة الفريد
                InvoiceNumber = invoiceNumber,
                // تعيين تاريخ الفاتورة بالوقت الحالي
                InvoiceDate = DateTime.UtcNow,
                // تعيين المورد
                SupplierId = order.SupplierId!.Value,
                // تعيين الفرع
                BranchId = order.BranchId,
                // تعيين المستودع المستلم
                WarehouseId = order.WarehouseId.Value,
                // تعيين حالة الفاتورة كمدفوعة ومكتملة
                Status = InvoiceStatus.Paid,
                // تعيين طريقة الدفع نقداً
                PaymentMethod = PaymentMethod.Cash,
                // ربط الفاتورة بأمر الشراء الحالي
                PurchaseOrderId = order.Id,
                // تدوين ملاحظة آلية توضح مصدر الفاتورة
                Notes = $"فاتورة مشتريات منشأة آلياً بموجب استلام طلبية الشراء رقم {order.OrderNumber}",
                // تهيئة قائمة بنود الفاتورة
                Items = new List<PurchaseInvoiceItem>()
            };

            // تحويل بنود أمر الشراء إلى بنود فاتورة مشتريات
            foreach (var item in order.Items)
            {
                // استنباط معرف الصنف للبند
                Guid productId = Guid.Empty;
                // محاولة الحصول عليه من الخريطة المجمعة مسبقاً
                if (receivedProductIds.TryGetValue(item.ProductBarCodeId, out var mappedPid))
                {
                    // تعيين معرف الصنف المستنبط
                    productId = mappedPid;
                }
                // محاولة الحصول عليه من خاصية التنقل
                else if (item.ProductBarCode != null)
                {
                    // تعيين معرف الصنف من الباركود
                    productId = item.ProductBarCode.ProductId;
                }

                // تخطي البند في حال عدم التمكن من تحديد الصنف
                if (productId == Guid.Empty) continue;

                // إضافة البند إلى الفاتورة المنشأة
                purchaseInvoice.Items.Add(new PurchaseInvoiceItem
                {
                    // تعيين معرف المستأجر
                    TenantId = order.TenantId,
                    // معرف الصنف
                    ProductId = productId,
                    // معرف باركود الصنف
                    ProductBarCodeId = item.ProductBarCodeId,
                    // الكمية المستلمة
                    Quantity = item.Quantity,
                    // سعر الوحدة
                    UnitPrice = item.UnitPrice,
                    // لا يوجد خصم افتراضي على البند
                    DiscountAmount = 0,
                    // إجمالي قيمة البند
                    LineTotal = item.Quantity * item.UnitPrice
                });
            }

            // احتساب إجمالي الفاتورة الفرعي
            purchaseInvoice.SubTotal = purchaseInvoice.Items.Sum(i => i.LineTotal);
            // احتساب إجمالي الفاتورة النهائي
            purchaseInvoice.TotalAmount = purchaseInvoice.SubTotal;
            // تعيين المبلغ المدفوع بالكامل
            purchaseInvoice.PaidAmount = purchaseInvoice.TotalAmount;
            // تصفير المبلغ المتبقي
            purchaseInvoice.RemainingAmount = 0;

            // إضافة فاتورة المشتريات الجديدة للمستودع
            await _unitOfWork.PurchaseInvoices.AddAsync(purchaseInvoice, ct);
        }

        // حفظ كافة التغييرات وحركات المخزون والفاتورة في معاملة واحدة
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة تحميل أمر الشراء بكامل تفاصيله لعرض أحدث البيانات
        var updatedOrder = await _unitOfWork.PurchaseOrders.FirstOrDefaultAsync(new PurchaseOrderWithDetailsSpec(id), ct) ?? order;
        // تحويل الكيان إلى كائن الاستجابة DTO
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(updatedOrder);

        // إرجاع النتيجة الناجحة
        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // تحميل متتبع لتفادي تضارب النسخ المكررة عند تكرار الصنف في البنود أثناء SoftDelete
        var order = await _unitOfWork.PurchaseOrders.FirstOrDefaultTrackedAsync(new PurchaseOrderWithDetailsSpec(id), ct);
        // التحقق من وجود أمر الشراء المطلوب حذفه
        if (order is null)
        {
            // إرجاع خطأ بعدم وجود أمر الشراء
            return ServiceResult.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // إجراء الحذف المنطقي لأمر الشراء وبنوده
        _unitOfWork.PurchaseOrders.SoftDelete(order);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة نجاح العملية
        return ServiceResult.Success();
    }
}
