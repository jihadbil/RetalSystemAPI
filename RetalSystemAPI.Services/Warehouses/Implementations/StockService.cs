using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Services.Warehouses.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة ومراقبة أرصدة المخازن وصالات العرض وتنبيهات مستويات النقص والتغذية التلقائية للأصناف.
/// </summary>
public class StockService : IStockService
{
    // وحدة العمل للتعامل مع مستودعات الأرصدة والمستودعات والمنتجات
    private readonly IUnitOfWork _unitOfWork;
    // محول النماذج للتحويل بين الكيانات وDTOs
    private readonly IMapper _mapper;
    /// <summary>
    /// تهيئة خدمة المخزون مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name = "mapper">محول الكيانات</param>
    public StockService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع المحول
        _mapper = mapper;
    }

    // ── Storge Stock Implementation (مخزون المخازن بالباركود) ──
    /// <inheritdoc/>
    public async Task<ServiceResult<StorgeStockResponseDto>> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام رصيد الباركود في المخزن المحدد
        // جلب سجل الرصيد من المستودع
        var stock = await _unitOfWork.Stock.GetStorageDetailsAsync(warehouseId, productBarcodeId, ct);
        // التحقق من وجود سجل الرصيد
        if (stock is null)
        {
            // إرجاع خطأ عدم وجود الرصيد
            return ServiceResult<StorgeStockResponseDto>.Failure("سجل مخزون المخزن غير موجود لهذا الباركود", ErrorCodes.StockNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<StorgeStockResponseDto>(stock);
        // إرجاع النتيجة بنجاح
        return ServiceResult<StorgeStockResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetStorgeStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(warehouseId, ct);
        if (warehouse is null || warehouse.Type != WarehouseType.Storge)
            return ServiceResult<IReadOnlyList<StorgeStockResponseDto>>.Failure("المخزن المحدد غير موجود أو نوعه غير صحيح", ErrorCodes.WarehouseNotFound);
        var stocks = await _unitOfWork.Stock.ListStorageAsync(new StorageStockQuery { WarehouseId = warehouseId }, ct);
        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<StorgeStockResponseDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<StorgeStockResponseDto>>> GetPagedStorgeStocksByWarehouseAsync(Guid warehouseId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool exactBarcode = false, CancellationToken ct = default)
    {
        // ضبط رقم الصفحة في حال كان أقل من 1
        if (pageNumber < 1)
            pageNumber = 1;
        // ضبط حجم الصفحة في حال كان أقل من 1
        if (pageSize < 1)
            pageSize = 10;
        // وضع حد أقصى لحجم الصفحة لمنع إثقال الذاكرة
        if (pageSize > 100)
            pageSize = 100;
        // تجهيز مواصفة حساب إجمالي عدد الأرصدة المطابقة
        // حساب إجمالي السجلات
        // تجهيز مواصفة استعلام الصفحة مع الفلترة والتجزئة
        // جلب عناصر الصفحة
        var(stocks, totalCount) = await _unitOfWork.Stock.SearchStorageAsync(new StorageStockQuery { WarehouseId = warehouseId, PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm, ExactBarcode = exactBarcode }, ct);
        // تحويل العناصر إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);
        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<StorgeStockResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<StorgeStockResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<StorgeStockResponseDto>> SetStorgeStockAsync(SetStorgeStockDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<StorgeStockResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // استعلام المستودع والتحقق من نوعه كمخزن تخزين
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        // التحقق من وجود المستودع ومطابقة نوعه
        if (warehouse is null || warehouse.Type != WarehouseType.Storge)
        {
            // إرجاع خطأ عدم وجود المخزن أو اختلاف نوعه
            return ServiceResult<StorgeStockResponseDto>.Failure("المخزن المحدد غير موجود أو ليس مخزن تخزين", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من وجود الباركود في قاعدة البيانات
        bool barcodeExists = await _unitOfWork.Products.BarcodeIdExistsAsync(dto.ProductBarcodeId, ct);
        // إذا لم يتم العثور على الباركود
        if (!barcodeExists)
        {
            // إرجاع خطأ عدم وجود الباركود
            return ServiceResult<StorgeStockResponseDto>.Failure("الباركود المحدد غير موجود", ErrorCodes.BarCodeNotFound);
        }

        // البحث عن سجل الرصيد الحالي
        var stock = await _unitOfWork.Stock.GetStorageAsync(dto.WarehouseId, dto.ProductBarcodeId, ct);
        // في حال عدم وجود سجل رصيد مسبق
        if (stock is null)
        {
            // إنشاء سجل رصيد جديد
            stock = new StorgeStock
            {
                TenantId = warehouse.TenantId,
                WarehouseId = dto.WarehouseId,
                ProductBarcodeId = dto.ProductBarcodeId,
                Quantity = (int)dto.Quantity,
                MinStockLevel = dto.MinStockLevel
            };
            // إضافة السجل الجديد
            await _unitOfWork.Stock.AddStorgeStockAsync(stock, ct);
        }
        else
        {
            // تعديل الكمية الحالية
            stock.Quantity = (int)dto.Quantity;
            // تعديل الحد الأدنى للمخزون
            stock.MinStockLevel = dto.MinStockLevel;
            // وسم السجل للتحديث
            _unitOfWork.Stock.UpdateStorgeStock(stock);
        }

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب السجل مع تفاصيل المنتج والباركود
        var updatedStock = await _unitOfWork.Stock.GetStorageDetailsAsync(stock.WarehouseId, stock.ProductBarcodeId, ct) ?? stock;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<StorgeStockResponseDto>(updatedStock);
        // إرجاع النتيجة بنجاح
        return ServiceResult<StorgeStockResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetLowStorgeStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام الأصناف التي وصلت للحد الأدنى
        // استرجاع سجلات النواقص المطابقة
        var stocks = await _unitOfWork.Stock.GetLowStorageAsync(warehouseId, ct);
        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<StorgeStockResponseDto>>.Success(dtos);
    }

    // ── Showroom Stock Implementation (مخزون صالة العرض بالمنتج) ──
    /// <inheritdoc/>
    public async Task<ServiceResult<ShowroomStockResponseDto>> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام رصيد الصنف في صالة العرض
        // جلب سجل الرصيد
        var stock = await _unitOfWork.Stock.GetShowroomDetailsAsync(warehouseId, productId, ct);
        // التحقق من وجود السجل
        if (stock is null)
        {
            // إرجاع خطأ عدم وجود الرصيد
            return ServiceResult<ShowroomStockResponseDto>.Failure("سجل مخزون الصالة غير موجود لهذا المنتج", ErrorCodes.StockNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<ShowroomStockResponseDto>(stock);
        // إرجاع النتيجة بنجاح
        return ServiceResult<ShowroomStockResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetShowroomStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(warehouseId, ct);
        if (warehouse is null || warehouse.Type != WarehouseType.Show)
            return ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>.Failure("الصالة المحددة غير موجودة أو نوعها غير صحيح", ErrorCodes.WarehouseNotFound);
        var stocks = await _unitOfWork.Stock.ListShowroomAsync(new ShowroomStockQuery { WarehouseId = warehouseId }, ct);
        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<ShowroomStockResponseDto>>> GetPagedShowroomStocksByWarehouseAsync(Guid warehouseId, int pageNumber = 1, int pageSize = 10, string? searchTerm = null, bool exactBarcode = false, CancellationToken ct = default)
    {
        // ضبط رقم الصفحة الأدنى
        if (pageNumber < 1)
            pageNumber = 1;
        // ضبط حجم الصفحة الأدنى
        if (pageSize < 1)
            pageSize = 10;
        // تقييد الحد الأقصى لحجم الصفحة
        if (pageSize > 100)
            pageSize = 100;
        // تجهيز مواصفة عد الأصناف
        // حساب إجمالي السجلات المطابقة
        // تجهيز مواصفة استعلام الصفحة مع التجزئة
        // استرجاع عناصر الصفحة
        var(stocks, totalCount) = await _unitOfWork.Stock.SearchShowroomAsync(new ShowroomStockQuery { WarehouseId = warehouseId, PageNumber = pageNumber, PageSize = pageSize, SearchTerm = searchTerm, ExactBarcode = exactBarcode }, ct);
        // تحويل العناصر إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);
        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<ShowroomStockResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<ShowroomStockResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<ShowroomStockResponseDto>> SetShowroomStockAsync(SetShowroomStockDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ShowroomStockResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // استعلام المستودع والتحقق من أنه صالة عرض
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        // التحقق من وجود الصالة ونوعها
        if (warehouse is null || warehouse.Type != WarehouseType.Show)
        {
            // إرجاع خطأ عدم وجود الصالة
            return ServiceResult<ShowroomStockResponseDto>.Failure("المخزن المحدد غير موجود أو ليس صالة عرض", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من وجود المنتج
        bool productExists = await _unitOfWork.Products.ExistsByIdAsync(dto.ProductId, ct);
        // إذا لم يتم العثور على المنتج
        if (!productExists)
        {
            // إرجاع خطأ عدم وجود المنتج
            return ServiceResult<ShowroomStockResponseDto>.Failure("المنتج المحدد غير موجود", ErrorCodes.ProductNotFound);
        }

        // البحث عن سجل الرصيد الحالي
        var stock = await _unitOfWork.Stock.GetShowroomAsync(dto.WarehouseId, dto.ProductId, ct);
        // في حال عدم وجود سجل سابق
        if (stock is null)
        {
            // إنشاء سجل رصيد صالة جديد
            stock = new ShowroomStock
            {
                TenantId = warehouse.TenantId,
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                Quantity = (int)dto.Quantity,
                MinStockLevel = dto.MinStockLevel
            };
            // إضافة السجل
            await _unitOfWork.Stock.AddShowroomStockAsync(stock, ct);
        }
        else
        {
            // تعديل الكمية
            stock.Quantity = (int)dto.Quantity;
            // تعديل الحد الأدنى للمخزون
            stock.MinStockLevel = dto.MinStockLevel;
            // وسم السجل للتحديث
            _unitOfWork.Stock.UpdateShowroomStock(stock);
        }

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إعادة جلب السجل مع تفاصيل المنتج
        var updatedStock = await _unitOfWork.Stock.GetShowroomDetailsAsync(stock.WarehouseId, stock.ProductId, ct) ?? stock;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<ShowroomStockResponseDto>(updatedStock);
        // إرجاع النتيجة بنجاح
        return ServiceResult<ShowroomStockResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetLowShowroomStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام النواقص بصالة العرض
        // جلب سجلات النواقص المطابقة
        var stocks = await _unitOfWork.Stock.GetLowShowroomAsync(warehouseId, ct);
        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>.Success(dtos);
    }
}
