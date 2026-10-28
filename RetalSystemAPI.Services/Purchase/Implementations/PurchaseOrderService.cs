using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.Models.Common;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Purchase.Interfaces;

namespace RetalSystemAPI.Services.Purchase.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة أوامر الشراء ومتابعة دورة حياة الطلبيات وحساب إجمالياتها وتوليد فواتير المشتريات آلياً عند الاستلام.
/// </summary>
public class PurchaseOrderService : IPurchaseOrderService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // المحول لرسم الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;
    // خدمة فواتير المشتريات لإنشاء الفاتورة وتغذية المخزون عند استلام أمر الشراء
    private readonly IPurchaseInvoiceService _purchaseInvoices;

    /// <summary>
    /// تهيئة خدمة أوامر الشراء مع حقن وحدة العمل ومحول الكيانات وخدمة فواتير المشتريات.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للتعامل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل الكائنات</param>
    /// <param name="purchaseInvoices">خدمة فواتير المشتريات لتوليد فاتورة الاستلام</param>
    public PurchaseOrderService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPurchaseInvoiceService purchaseInvoices)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول البيانات
        _mapper = mapper;
        // تعيين مرجع خدمة فواتير المشتريات
        _purchaseInvoices = purchaseInvoices;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseOrderResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // الاستعلام عن أمر الشراء بالمعرف مع تضمين الفرع والمستودع والمورد وتفاصيل الأصناف والباركود
        var order = await _unitOfWork.PurchaseOrders.GetDetailsAsync(id, ct);

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

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        PurchaseOrderStatus? status = null,
        CancellationToken ct = default)
    {
        // جلب قائمة أوامر الشراء المطابقة للمواصفة المحددة بالفلاتر
        var orders = await _unitOfWork.PurchaseOrders.ListAsync(new PurchaseOrderQuery
        {
            BranchId = branchId,
            WarehouseId = warehouseId,
            Status = status
        }, ct);

        // تحويل قائمة الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<PurchaseOrderSummaryDto>>(orders);

        // إرجاع النتيجة الناجحة مع قائمة الملخصات
        return ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        PurchaseOrderStatus? status = null,
        string? search = null,
        CancellationToken ct = default)
    {
        // تنفيذ الاستعلام الصفحي لجلب عناصر الصفحة الحالية وإجمالي عدد السجلات
        var (items, totalCount) = await _unitOfWork.PurchaseOrders.SearchAsync(new PurchaseOrderQuery
        {
            BranchId = branchId,
            WarehouseId = warehouseId,
            Status = status,
            Search = search,
            PageNumber = pageNumber,
            PageSize = pageSize
        }, ct);

        // تحويل عناصر الصفحة الحالية إلى قائمة ملخصات
        var dtos = _mapper.Map<IReadOnlyList<PurchaseOrderSummaryDto>>(items);

        // إنشاء كائن النتيجة المصفحة مع حساب إجمالي الصفحات
        var pagedResult = PagedResult<PurchaseOrderSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة الناجحة بالبيانات المصفحة
        return ServiceResult<PagedResult<PurchaseOrderSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        // التحقق من صحة بيانات النموذج المدخل
        if (DtoValidation.GetError(dto) is { } validationError)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        }

        // التحقق من صحة كميات البنود وأنها تمثل أعداداً صحيحة موجبة
        if (dto.Items?.Any(i => !StockQuantity.IsValid(i.Quantity)) == true)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الكميات يجب أن تكون أعداداً صحيحة موجبة من أصغر وحدة، ضمن الحد المسموح", ErrorCodes.ValidationError);
        }

        // التحقق من صحة وجود الفرع المسند لأمر الشراء
        bool branchExists = await _unitOfWork.Branches.ExistsByIdAsync(dto.BranchId, ct);
        if (!branchExists)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // التحقق من وجود المورد إن تم تحديده في الطلبية
        if (dto.SupplierId.HasValue && dto.SupplierId.Value != Guid.Empty)
        {
            bool supplierExists = await _unitOfWork.Suppliers.ExistsByIdAsync(dto.SupplierId.Value, ct);
            if (!supplierExists)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
            }
        }

        // التحقق من وجود المستودع المستهدف إن تم تحديده
        if (dto.WarehouseId.HasValue)
        {
            bool warehouseExists = await _unitOfWork.Warehouses.ExistsByIdAsync(dto.WarehouseId.Value, ct);
            if (!warehouseExists)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المخزن المحدد غير موجود", ErrorCodes.WarehouseNotFound);
            }
        }

        // التحقق من عدم تكرار رقم أمر الشراء
        bool orderNumExists = await _unitOfWork.PurchaseOrders.NumberExistsAsync(dto.OrderNumber, ct);
        if (orderNumExists)
        {
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
            order.Items = dto.Items.Select(item => new PurchaseOrderItem
            {
                ProductBarCodeId = item.ProductBarCodeId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.Quantity * item.UnitPrice
            }).ToList();

            // احتساب إجمالي أمر الشراء كمجموع إجماليات البنود
            order.TotalAmount = order.Items.Sum(i => i.LineTotal);
        }
        else
        {
            order.TotalAmount = 0;
        }

        // إضافة أمر الشراء الجديد إلى المستودع
        await _unitOfWork.PurchaseOrders.AddAsync(order, ct);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب أمر الشراء مع العلاقات الكاملة
        var createdOrder = await _unitOfWork.PurchaseOrders.GetDetailsAsync(order.Id, ct) ?? order;

        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(createdOrder);

        // إرجاع النتيجة الناجحة
        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseOrderResponseDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken ct = default)
    {
        // التحقق من صحة بيانات التحديث المدخلة
        if (DtoValidation.GetError(dto) is { } validationError)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        }

        // التحقق من صحة كميات البنود
        if (dto.Items?.Any(i => !StockQuantity.IsValid(i.Quantity)) == true)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("الكميات يجب أن تكون أعداداً صحيحة موجبة من أصغر وحدة، ضمن الحد المسموح", ErrorCodes.ValidationError);
        }

        // استعلام أمر الشراء المطلوب تعديله مع كافة تفاصيله
        var order = await _unitOfWork.PurchaseOrders.GetDetailsAsync(id, ct);
        if (order is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // منع تعديل أمر شراء تم استلامه وإغلاقه نهائياً
        if (order.Status == PurchaseOrderStatus.Received)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("لا يمكن تعديل طلبية تم استلامها وإغلاقها بالفعل", ErrorCodes.ValidationError);
        }

        // التحقق من المورد الجديد إن تم تمريره
        if (dto.SupplierId.HasValue)
        {
            if (dto.SupplierId.Value != Guid.Empty)
            {
                bool supplierExists = await _unitOfWork.Suppliers.ExistsByIdAsync(dto.SupplierId.Value, ct);
                if (!supplierExists)
                {
                    return ServiceResult<PurchaseOrderResponseDto>.Failure("المورد المحدد غير موجود", ErrorCodes.SupplierNotFound);
                }
            }

            order.SupplierId = dto.SupplierId.Value == Guid.Empty ? null : dto.SupplierId;
        }

        // التحقق من المستودع الجديد إن تم تمريره
        if (dto.WarehouseId.HasValue)
        {
            bool warehouseExists = await _unitOfWork.Warehouses.ExistsByIdAsync(dto.WarehouseId.Value, ct);
            if (!warehouseExists)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("المخزن المحدد غير موجود", ErrorCodes.WarehouseNotFound);
            }
            order.WarehouseId = dto.WarehouseId;
        }

        // تحديث التاريخ المتوقع لوصول البضاعة
        order.ExpectedDate = dto.ExpectedDate;

        // تحديث بنود أمر الشراء في حال تم إرسالها
        if (dto.Items != null)
        {
            // حذف البنود القديمة المسجلة حالياً
            if (order.Items != null && order.Items.Any())
            {
                foreach (var existingItem in order.Items.ToList())
                {
                    _unitOfWork.PurchaseOrders.HardDeleteItem(existingItem);
                }
            }

            // إنشاء قائمة البنود الجديدة
            order.Items = dto.Items.Select(item => new PurchaseOrderItem
            {
                PurchaseOrderId = id,
                ProductBarCodeId = item.ProductBarCodeId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
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
        var updatedOrder = await _unitOfWork.PurchaseOrders.GetDetailsAsync(id, ct) ?? order;

        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<PurchaseOrderResponseDto>(updatedOrder);

        // إرجاع النتيجة الناجحة
        return ServiceResult<PurchaseOrderResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PurchaseOrderResponseDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default)
    {
        // التحقق من صحة قيمة الحالة المطلوبة
        if (!Enum.IsDefined(status))
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("حالة الطلبية غير صحيحة", ErrorCodes.ValidationError);
        }

        // استعلام أمر الشراء للتعديل ككيان متتبع
        var order = await _unitOfWork.PurchaseOrders.GetDetailsForUpdateAsync(id, ct);
        if (order is null)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("أمر الشراء/الطلبية غير موجودة", ErrorCodes.PurchaseOrderNotFound);
        }

        // منع تغيير حالة طلبية مستلمة أو ملغاة مسبقاً
        if (order.Status is PurchaseOrderStatus.Received or PurchaseOrderStatus.Cancelled)
        {
            return ServiceResult<PurchaseOrderResponseDto>.Failure("لا يمكن تغيير حالة طلبية مستلمة أو ملغاة", ErrorCodes.ValidationError);
        }

        // عند تغيير الحالة إلى "مستلم" يتم توليد فاتورة مشتريات وتغذية المخزون آلياً
        if (status == PurchaseOrderStatus.Received)
        {
            // التحقق من تحديد المورد والمستودع وبنود الطلبية قبل الاستلام
            if (!order.SupplierId.HasValue || !order.WarehouseId.HasValue || order.Items.Count == 0)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("يجب تحديد المورد والمستودع وبنود الطلبية قبل الاستلام", ErrorCodes.ValidationError);
            }

            // التحقق من وجود كافة باركودات الأصناف المسجلة في الطلبية
            var barcodeIds = order.Items.Select(i => i.ProductBarCodeId).Distinct().ToList();
            var barcodes = (await _unitOfWork.Products.GetBarcodesByIdsAsync(barcodeIds, ct)).ToDictionary(b => b.Id);
            if (barcodes.Count != barcodeIds.Count)
            {
                return ServiceResult<PurchaseOrderResponseDto>.Failure("أحد باركودات الطلبية غير موجود", ErrorCodes.BarCodeNotFound);
            }

            // تجهيز نموذج إنشاء فاتورة المشتريات المرتبطة بأمر الشراء
            var request = new CreatePurchaseInvoiceDto
            {
                InvoiceNumber = $"PO-{order.Id:N}",
                InvoiceDate = DateTime.UtcNow,
                SupplierId = order.SupplierId.Value,
                BranchId = order.BranchId,
                WarehouseId = order.WarehouseId.Value,
                PurchaseOrderId = order.Id,
                Status = InvoiceStatus.Pending,
                PaymentMethod = PaymentMethod.Credit,
                PaidAmount = 0,
                Notes = $"فاتورة مشتريات بموجب استلام طلبية الشراء رقم {order.OrderNumber}",
                Items = order.Items.Select(i => new CreatePurchaseInvoiceItemDto
                {
                    ProductId = barcodes[i.ProductBarCodeId].ProductId,
                    ProductBarCodeId = i.ProductBarCodeId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            // الاحتفاظ بالحالة السابقة وتعيين الحالة الجديدة
            var previousStatus = order.Status;
            order.Status = status;

            // إنشاء فاتورة المشتريات (تتولى خدمة الفواتير المعاملة الذرية وتغذية المخزون والقيود المحاسبية)
            var result = await _purchaseInvoices.CreateAsync(request, ct);
            if (!result.IsSuccess)
            {
                // استعادة الحالة السابقة عند فشل إنشاء الفاتورة
                order.Status = previousStatus;
                return ServiceResult<PurchaseOrderResponseDto>.Failure(result.ErrorMessage!, result.ErrorCode);
            }
        }
        else
        {
            // تحديث حالة الطلبية وحفظ التغيير
            order.Status = status;
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // جلب أمر الشراء المحدث وإرجاعه
        var updated = await _unitOfWork.PurchaseOrders.GetDetailsAsync(id, ct) ?? order;
        return ServiceResult<PurchaseOrderResponseDto>.Success(_mapper.Map<PurchaseOrderResponseDto>(updated));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام أمر الشراء المطلوب حذفه ككيان متتبع
        var order = await _unitOfWork.PurchaseOrders.GetDetailsForUpdateAsync(id, ct);
        if (order is null)
        {
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
