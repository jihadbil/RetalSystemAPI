using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;
using RetalSystemAPI.Services.Warehouses.Specifications;

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
    /// <param name="unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name="mapper">محول الكيانات</param>
    public StockService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;

        // تعيين مرجع المحول
        _mapper = mapper;
    }

    // ── Storge Stock Implementation (مخزون المخازن بالباركود) ──

    /// <inheritdoc />
    public async Task<ServiceResult<StorgeStockResponseDto>> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام رصيد الباركود في المخزن المحدد
        var spec = new StorgeStockWithDetailsSpec(warehouseId, productBarcodeId);

        // جلب سجل الرصيد من المستودع
        var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(spec, ct);

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

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetStorgeStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        // 1. استخراج معرفات الباركودات والمستأجر فقط لتحسين الأداء وتجنب تحميل الكيانات الثقيلة
        var allBarCodeIdsWithTenants = await _unitOfWork.ProductBarCodes.SelectAsync(bc => new { bc.Id, bc.TenantId }, ct);

        // استخراج معرفات الباركودات المسجلة فعلياً في مخزون هذا المستودع
        var existingBarcodeIds = (await _unitOfWork.StorgeStocks.SelectAsync(
            s => new { s.WarehouseId, s.ProductBarcodeId }, ct))
            .Where(s => s.WarehouseId == warehouseId)
            .Select(s => s.ProductBarcodeId)
            .ToHashSet();

        // تحديد الباركودات التي لم ينشأ لها سجل رصيد بعد
        var missingBarcodes = allBarCodeIdsWithTenants.Where(bc => !existingBarcodeIds.Contains(bc.Id)).ToList();

        // في حال وجود باركودات ناقصة، يتم إنشاؤها تلقائياً برصيد صفري
        if (missingBarcodes.Count > 0)
        {
            // بناء سجلات الأرصدة الجديدة
            var newStocks = missingBarcodes.Select(bc => new StorgeStock
            {
                TenantId = bc.TenantId,
                WarehouseId = warehouseId,
                ProductBarcodeId = bc.Id,
                Quantity = 0,
                MinStockLevel = 0
            }).ToList();

            // إدراج السجلات الجديدة دفعة واحدة
            await _unitOfWork.StorgeStocks.AddRangeAsync(newStocks, ct);

            // حفظ التغييرات في قاعدة البيانات
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // استعلام كافة أرصدة المستودع مع تفاصيل الباركود والمنتج والصور
        var spec = new StorgeStockWithDetailsSpec(warehouseId);

        // جلب قائمة الأرصدة
        var stocks = await _unitOfWork.StorgeStocks.FindAsync(spec, ct);

        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<StorgeStockResponseDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<StorgeStockResponseDto>>> GetPagedStorgeStocksByWarehouseAsync(
        Guid warehouseId,
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool exactBarcode = false,
        CancellationToken ct = default)
    {
        // ضبط رقم الصفحة في حال كان أقل من 1
        if (pageNumber < 1) pageNumber = 1;

        // ضبط حجم الصفحة في حال كان أقل من 1
        if (pageSize < 1) pageSize = 10;

        // وضع حد أقصى لحجم الصفحة لمنع إثقال الذاكرة
        if (pageSize > 100) pageSize = 100;

        // تجهيز مواصفة حساب إجمالي عدد الأرصدة المطابقة
        var countSpec = new StorgeStockCountSpec(warehouseId, searchTerm, exactBarcode);

        // حساب إجمالي السجلات
        int totalCount = await _unitOfWork.StorgeStocks.CountAsync(countSpec, ct);

        // تجهيز مواصفة استعلام الصفحة مع الفلترة والتجزئة
        var pagedSpec = new StorgeStockWithDetailsSpec(warehouseId, pageNumber, pageSize, searchTerm, exactBarcode, isPaged: true);

        // جلب عناصر الصفحة
        var stocks = await _unitOfWork.StorgeStocks.FindAsync(pagedSpec, ct);

        // تحويل العناصر إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);

        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<StorgeStockResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<StorgeStockResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<StorgeStockResponseDto>> SetStorgeStockAsync(SetStorgeStockDto dto, CancellationToken ct = default)
    {
        // استعلام المستودع والتحقق من نوعه كمخزن تخزين
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);

        // التحقق من وجود المستودع ومطابقة نوعه
        if (warehouse is null || warehouse.Type != WarehouseType.Storge)
        {
            // إرجاع خطأ عدم وجود المخزن أو اختلاف نوعه
            return ServiceResult<StorgeStockResponseDto>.Failure("المخزن المحدد غير موجود أو ليس مخزن تخزين", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من وجود الباركود في قاعدة البيانات
        bool barcodeExists = await _unitOfWork.ProductBarCodes.ExistsAsync(b => b.Id == dto.ProductBarcodeId, ct);

        // إذا لم يتم العثور على الباركود
        if (!barcodeExists)
        {
            // إرجاع خطأ عدم وجود الباركود
            return ServiceResult<StorgeStockResponseDto>.Failure("الباركود المحدد غير موجود", ErrorCodes.BarCodeNotFound);
        }

        // البحث عن سجل الرصيد الحالي
        var stock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(
            s => s.WarehouseId == dto.WarehouseId && s.ProductBarcodeId == dto.ProductBarcodeId, ct);

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
            await _unitOfWork.StorgeStocks.AddAsync(stock, ct);
        }
        else
        {
            // تعديل الكمية الحالية
            stock.Quantity = (int)dto.Quantity;

            // تعديل الحد الأدنى للمخزون
            stock.MinStockLevel = dto.MinStockLevel;

            // وسم السجل للتحديث
            _unitOfWork.StorgeStocks.Update(stock);
        }

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب السجل مع تفاصيل المنتج والباركود
        var updatedStock = await _unitOfWork.StorgeStocks.FirstOrDefaultAsync(new StorgeStockWithDetailsSpec(stock.WarehouseId, stock.ProductBarcodeId), ct) ?? stock;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<StorgeStockResponseDto>(updatedStock);

        // إرجاع النتيجة بنجاح
        return ServiceResult<StorgeStockResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetLowStorgeStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام الأصناف التي وصلت للحد الأدنى
        var spec = new LowStorgeStockSpec(warehouseId);

        // استرجاع سجلات النواقص المطابقة
        var stocks = await _unitOfWork.StorgeStocks.FindAsync(spec, ct);

        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StorgeStockResponseDto>>(stocks);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<StorgeStockResponseDto>>.Success(dtos);
    }

    // ── Showroom Stock Implementation (مخزون صالة العرض بالمنتج) ──

    /// <inheritdoc />
    public async Task<ServiceResult<ShowroomStockResponseDto>> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام رصيد الصنف في صالة العرض
        var spec = new ShowroomStockWithDetailsSpec(warehouseId, productId);

        // جلب سجل الرصيد
        var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(spec, ct);

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

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetShowroomStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default)
    {
        // 1. استخراج معرفات المنتجات والمستأجر بأعمدة نحيفة لتحسين الأداء
        var allProductIdsWithTenants = await _unitOfWork.Products.SelectAsync(p => new { p.Id, p.TenantId }, ct);

        // استخراج معرفات المنتجات المسجلة في صالة العرض حالياً
        var existingProductIds = (await _unitOfWork.ShowroomStocks.SelectAsync(
            s => new { s.WarehouseId, s.ProductId }, ct))
            .Where(s => s.WarehouseId == warehouseId)
            .Select(s => s.ProductId)
            .ToHashSet();

        // تحديد المنتجات غير المسجلة في الصالة
        var missingProducts = allProductIdsWithTenants.Where(p => !existingProductIds.Contains(p.Id)).ToList();

        // إضافة سجلات صفرية للمنتجات غير المسجلة
        if (missingProducts.Count > 0)
        {
            // إنشاء سجلات الرصيد الافتتاحي
            var newStocks = missingProducts.Select(p => new ShowroomStock
            {
                TenantId = p.TenantId,
                WarehouseId = warehouseId,
                ProductId = p.Id,
                Quantity = 0,
                MinStockLevel = 0
            }).ToList();

            // إدراج السجلات الجديدة دفعة واحدة
            await _unitOfWork.ShowroomStocks.AddRangeAsync(newStocks, ct);

            // حفظ التغييرات
            await _unitOfWork.SaveChangesAsync(ct);
        }

        // استعلام أرصدة صالة العرض مع بيانات المنتجات والصور
        var spec = new ShowroomStockWithDetailsSpec(warehouseId);

        // جلب الأرصدة
        var stocks = await _unitOfWork.ShowroomStocks.FindAsync(spec, ct);

        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<ShowroomStockResponseDto>>> GetPagedShowroomStocksByWarehouseAsync(
        Guid warehouseId,
        int pageNumber = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool exactBarcode = false,
        CancellationToken ct = default)
    {
        // ضبط رقم الصفحة الأدنى
        if (pageNumber < 1) pageNumber = 1;

        // ضبط حجم الصفحة الأدنى
        if (pageSize < 1) pageSize = 10;

        // تقييد الحد الأقصى لحجم الصفحة
        if (pageSize > 100) pageSize = 100;

        // تجهيز مواصفة عد الأصناف
        var countSpec = new ShowroomStockCountSpec(warehouseId, searchTerm, exactBarcode);

        // حساب إجمالي السجلات المطابقة
        int totalCount = await _unitOfWork.ShowroomStocks.CountAsync(countSpec, ct);

        // تجهيز مواصفة استعلام الصفحة مع التجزئة
        var pagedSpec = new ShowroomStockWithDetailsSpec(warehouseId, pageNumber, pageSize, searchTerm, exactBarcode, isPaged: true);

        // استرجاع عناصر الصفحة
        var stocks = await _unitOfWork.ShowroomStocks.FindAsync(pagedSpec, ct);

        // تحويل العناصر إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);

        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<ShowroomStockResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<ShowroomStockResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<ShowroomStockResponseDto>> SetShowroomStockAsync(SetShowroomStockDto dto, CancellationToken ct = default)
    {
        // استعلام المستودع والتحقق من أنه صالة عرض
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);

        // التحقق من وجود الصالة ونوعها
        if (warehouse is null || warehouse.Type != WarehouseType.Show)
        {
            // إرجاع خطأ عدم وجود الصالة
            return ServiceResult<ShowroomStockResponseDto>.Failure("المخزن المحدد غير موجود أو ليس صالة عرض", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من وجود المنتج
        bool productExists = await _unitOfWork.Products.ExistsAsync(p => p.Id == dto.ProductId, ct);

        // إذا لم يتم العثور على المنتج
        if (!productExists)
        {
            // إرجاع خطأ عدم وجود المنتج
            return ServiceResult<ShowroomStockResponseDto>.Failure("المنتج المحدد غير موجود", ErrorCodes.ProductNotFound);
        }

        // البحث عن سجل الرصيد الحالي
        var stock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(
            s => s.WarehouseId == dto.WarehouseId && s.ProductId == dto.ProductId, ct);

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
            await _unitOfWork.ShowroomStocks.AddAsync(stock, ct);
        }
        else
        {
            // تعديل الكمية
            stock.Quantity = (int)dto.Quantity;

            // تعديل الحد الأدنى للمخزون
            stock.MinStockLevel = dto.MinStockLevel;

            // وسم السجل للتحديث
            _unitOfWork.ShowroomStocks.Update(stock);
        }

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب السجل مع تفاصيل المنتج
        var updatedStock = await _unitOfWork.ShowroomStocks.FirstOrDefaultAsync(new ShowroomStockWithDetailsSpec(stock.WarehouseId, stock.ProductId), ct) ?? stock;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<ShowroomStockResponseDto>(updatedStock);

        // إرجاع النتيجة بنجاح
        return ServiceResult<ShowroomStockResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetLowShowroomStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام النواقص بصالة العرض
        var spec = new LowShowroomStockSpec(warehouseId);

        // جلب سجلات النواقص المطابقة
        var stocks = await _unitOfWork.ShowroomStocks.FindAsync(spec, ct);

        // تحويل الكيانات إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<ShowroomStockResponseDto>>(stocks);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>.Success(dtos);
    }
}
