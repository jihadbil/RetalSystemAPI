using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.ProductBarCode;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة باركودات ونكهات المنتجات وتحديث أرصدة التخزين التلقائية.
/// </summary>
public class ProductBarCodeService : IProductBarCodeService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;
    /// <summary>
    /// تهيئة خدمة الباركود مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name = "mapper">خدمة تحويل النماذج</param>
    public ProductBarCodeService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetAllAsync(string? search = null, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام الباركودات الشاملة مع البحث
        // جلب قائمة الباركودات المطابقة
        var barCodes = await _unitOfWork.Products.SearchBarcodesAsync(new BarcodeQuery { Search = search }, ct);
        // تحويل الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<ProductBarCodeResponseDto>>(barCodes);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام باركودات المنتج المحدد
        // جلب باركودات ونكهات المنتج
        var barCodes = await _unitOfWork.Products.GetBarcodesWithDetailsAsync(productId, ct);
        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ProductBarCodeResponseDto>>(barCodes);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ProductBarCodeResponseDto>> AddBarCodeAsync(Guid productId, CreateProductBarCodeDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ProductBarCodeResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من صحة وجود المنتج المستهدف
        bool productExists = await _unitOfWork.Products.ExistsByIdAsync(productId, ct);
        // في حال عدم وجود المنتج
        if (!productExists)
        {
            // إرجاع خطأ بعدم وجود المنتج
            return ServiceResult<ProductBarCodeResponseDto>.Failure("المنتج غير موجود", ErrorCodes.ProductNotFound);
        }

        // التحقق من عدم استخدام كود الباركود مسبقاً في النظام
        bool barCodeExists = await _unitOfWork.Products.BarCodeExistsAsync(dto.BarCode, ct);
        // في حال وجود الباركود
        if (barCodeExists)
        {
            // إرجاع خطأ تكرار الباركود
            return ServiceResult<ProductBarCodeResponseDto>.Failure("قيمة الباركود مستخدمة بالفعل لمنتج آخر", ErrorCodes.BarCodeDuplicate);
        }

        var storgeWarehouses = await _unitOfWork.Warehouses.GetByTypeAsync(WarehouseType.Storge, ct);
        Guid? openingWarehouseId = dto.WarehouseId;
        if (openingWarehouseId.HasValue && !storgeWarehouses.Any(w => w.Id == openingWarehouseId.Value && w.IsActive))
            return ServiceResult<ProductBarCodeResponseDto>.Failure("مخزن الرصيد الافتتاحي غير موجود أو معطل", ErrorCodes.WarehouseNotFound);
        if (dto.InitialQuantity > 0 && !openingWarehouseId.HasValue)
        {
            var active = storgeWarehouses.Where(w => w.IsActive).ToList();
            if (active.Count != 1)
                return ServiceResult<ProductBarCodeResponseDto>.Failure("حدد مخزن الرصيد الافتتاحي عند وجود كمية؛ لا يوجد مخزن فعال وحيد", ErrorCodes.ValidationError);
            openingWarehouseId = active[0].Id;
        }

        // تحويل بيانات الإدخال إلى كيان ProductBarCode
        var barCodeEntity = _mapper.Map<ProductBarCode>(dto);
        barCodeEntity.Id = Guid.NewGuid();
        // ربط معرف المنتج بالباركود
        barCodeEntity.ProductId = productId;
        // إضافة الباركود إلى المستودع
        await _unitOfWork.Products.AddBarCodeAsync(barCodeEntity, ct);
        // المرور على مخازن التخزين القائمة
        foreach (var storgeWh in storgeWarehouses)
        {
            // إنشاء سجل رصيد المخزن للباركود الجديد
            var storgeStock = new StorgeStock
            {
                WarehouseId = storgeWh.Id,
                ProductBarcodeId = barCodeEntity.Id,
                Quantity = storgeWh.Id == openingWarehouseId ? dto.InitialQuantity : 0,
                MinStockLevel = 0
            };
            // إضافة سجل الرصيد المخزني
            await _unitOfWork.Stock.AddStorgeStockAsync(storgeStock, ct);
        }

        // حفظ سجلات الأرصدة المخزنية في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // تحويل الكيان إلى كائن الاستجابة
        var responseDto = _mapper.Map<ProductBarCodeResponseDto>(barCodeEntity);
        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductBarCodeResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ProductBarCodeResponseDto>> UpdateBarCodeAsync(Guid barCodeId, UpdateProductBarCodeDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ProductBarCodeResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // استعلام الباركود المطلوب تعديله بالمعرف
        var barCodeEntity = await _unitOfWork.Products.GetBarCodeByIdAsync(barCodeId, ct);
        // التحقق من وجود الباركود
        if (barCodeEntity is null)
        {
            // إرجاع خطأ بعدم وجود الباركود
            return ServiceResult<ProductBarCodeResponseDto>.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        // التحقق من عدم استخدام كود الباركود الجديد لدى باركود آخر
        bool barCodeExists = await _unitOfWork.Products.BarCodeExistsAsync(dto.BarCode, barCodeId, ct);
        // في حال تكرار الباركود
        if (barCodeExists)
        {
            // إرجاع خطأ تكرار الباركود
            return ServiceResult<ProductBarCodeResponseDto>.Failure("قيمة الباركود مستخدمة بالفعل لمنتج آخر", ErrorCodes.BarCodeDuplicate);
        }

        // تحديث رقم الباركود
        barCodeEntity.BarCode = dto.BarCode;
        // تحديث عنوان أو نكهة الباركود
        barCodeEntity.Title = dto.Title;
        // تحديث الوصف
        barCodeEntity.Description = dto.Description;
        // تحديث السجل في المستودع
        _unitOfWork.Products.UpdateBarCode(barCodeEntity);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<ProductBarCodeResponseDto>(barCodeEntity);
        // إرجاع النتيجة الناجحة
        return ServiceResult<ProductBarCodeResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> RemoveBarCodeAsync(Guid barCodeId, CancellationToken ct = default)
    {
        // استعلام الباركود المراد حذفه
        var barCodeEntity = await _unitOfWork.Products.GetBarCodeByIdAsync(barCodeId, ct);
        // التحقق من وجود الباركود
        if (barCodeEntity is null)
        {
            // إرجاع خطأ بعدم وجود الباركود
            return ServiceResult.Failure("الباركود غير موجود", ErrorCodes.BarCodeNotFound);
        }

        Guid[] barcodeIds = [barCodeId];
        if (await _unitOfWork.SalesInvoices.HasBarcodeReferencesAsync(barcodeIds, ct) ||
            await _unitOfWork.SalesReturns.HasBarcodeReferencesAsync(barcodeIds, ct) ||
            await _unitOfWork.PurchaseInvoices.HasBarcodeReferencesAsync(barcodeIds, ct) ||
            await _unitOfWork.PurchaseReturns.HasBarcodeReferencesAsync(barcodeIds, ct) ||
            await _unitOfWork.PurchaseOrders.HasBarcodeReferencesAsync(barcodeIds, ct) ||
            await _unitOfWork.StockTransfers.HasBarcodeReferencesAsync(barcodeIds, ct) ||
            await _unitOfWork.StockAdjustments.HasBarcodeReferencesAsync(barcodeIds, ct))
            return ServiceResult.Failure("لا يمكن حذف باركود له مستندات أو حركات مخزنية مرتبطة", ErrorCodes.BarCodeInUse);

        var stocks = await _unitOfWork.Stock.GetStorageByBarcodesForUpdateAsync(barcodeIds, ct);
        if (stocks.Any(s => s.Quantity != 0))
            return ServiceResult.Failure("لا يمكن حذف باركود له رصيد مخزني", ErrorCodes.BarCodeInUse);

        foreach (var image in await _unitOfWork.Products.GetImagesByBarcodesForUpdateAsync(barcodeIds, ct))
            _unitOfWork.Products.SoftDeleteImage(image);
        foreach (var stock in stocks)
            _unitOfWork.Stock.SoftDeleteStorgeStock(stock);
        _unitOfWork.Products.SoftDeleteBarCode(barCodeEntity);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
