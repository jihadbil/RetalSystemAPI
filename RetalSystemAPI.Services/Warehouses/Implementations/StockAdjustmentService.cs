using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;

namespace RetalSystemAPI.Services.Warehouses.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة التسويات الجردية وضبط كميات المخزون الفعلية ومعالجة الفوارق المحاسبية.
/// </summary>
public class StockAdjustmentService : IStockAdjustmentService
{
    // وحدة العمل للتعامل مع مستودعات التسويات والمخازن والأرصدة
    private readonly IUnitOfWork _unitOfWork;
    // محول النماذج لتحويل الكيانات إلى DTOs
    private readonly IMapper _mapper;
    /// <summary>
    /// تهيئة خدمة التسويات الجردية مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name = "mapper">محول الكيانات</param>
    public StockAdjustmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع المحول
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<StockAdjustmentResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام التسوية الجردية بالمعرف مع تفاصيل البنود والمنتجات
        var adjustment = await _unitOfWork.StockAdjustments.GetDetailsAsync(id, ct);
        // التحقق من وجود التسوية
        if (adjustment is null)
        {
            // إرجاع خطأ عدم العثور على التسوية
            return ServiceResult<StockAdjustmentResponseDto>.Failure("التسوية الجردية غير موجودة", ErrorCodes.StockAdjustmentNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<StockAdjustmentResponseDto>(adjustment);
        // إرجاع النتيجة بنجاح
        return ServiceResult<StockAdjustmentResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<StockAdjustmentResponseDto>> GetByAdjustmentNumberAsync(string adjustmentNumber, CancellationToken ct = default)
    {
        // استعلام التسوية الجردية برقم السند
        var adjustment = await _unitOfWork.StockAdjustments.GetByNumberAsync(adjustmentNumber, ct);
        // التحقق من وجود التسوية
        if (adjustment is null)
        {
            // إرجاع خطأ عدم العثور على التسوية
            return ServiceResult<StockAdjustmentResponseDto>.Failure("التسوية الجردية غير موجودة", ErrorCodes.StockAdjustmentNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<StockAdjustmentResponseDto>(adjustment);
        // إرجاع النتيجة بنجاح
        return ServiceResult<StockAdjustmentResponseDto>.Success(dto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>> GetAllAsync(Guid? warehouseId = null, StockAdjustmentReason? reason = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام التسويات مع الفلاتر
        // جلب قائمة التسويات المطابقة
        var adjustments = await _unitOfWork.StockAdjustments.ListAsync(new StockAdjustmentQuery { WarehouseId = warehouseId, Reason = reason, FromDate = fromDate, ToDate = toDate, Search = search }, ct);
        // تحويل الكيانات إلى ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<StockAdjustmentSummaryDto>>(adjustments);
        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<StockAdjustmentSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? warehouseId = null, StockAdjustmentReason? reason = null, DateTime? fromDate = null, DateTime? toDate = null, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة الاستعلام المفلترة
        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var(items, totalCount) = await _unitOfWork.StockAdjustments.SearchAsync(new StockAdjustmentQuery { WarehouseId = warehouseId, Reason = reason, FromDate = fromDate, ToDate = toDate, Search = search, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StockAdjustmentSummaryDto>>(items);
        // إنشاء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<StockAdjustmentSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<StockAdjustmentSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<StockAdjustmentResponseDto>> CreateAsync(CreateStockAdjustmentDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<StockAdjustmentResponseDto>.Failure(validationError, ErrorCodes.ValidationError);
        if (dto.Items is null || dto.Items.Count == 0)
            return ServiceResult<StockAdjustmentResponseDto>.Failure("يجب إضافة بند واحد على الأقل للتسوية", ErrorCodes.ValidationError);
        var warehouse = await _unitOfWork.Warehouses.GetByIdForUpdateAsync(dto.WarehouseId, ct);
        if (warehouse is null || !Enum.IsDefined(warehouse.Type))
            return ServiceResult<StockAdjustmentResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        if (await _unitOfWork.StockAdjustments.NumberExistsAsync(dto.AdjustmentNumber, ct))
            return ServiceResult<StockAdjustmentResponseDto>.Failure("رقم التسوية مستخدم بالفعل", ErrorCodes.StockAdjustmentNumberExists);
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        if ((await _unitOfWork.Products.GetByIdsAsync(productIds, ct)).Count != productIds.Count)
            return ServiceResult<StockAdjustmentResponseDto>.Failure("أحد الأصناف المحددة غير موجود", ErrorCodes.ProductNotFound);
        var barcodeIds = dto.Items.Where(i => i.ProductBarCodeId.HasValue).Select(i => i.ProductBarCodeId!.Value).Distinct().ToList();
        var barcodes = (await _unitOfWork.Products.GetBarcodesByIdsAsync(barcodeIds, ct)).ToDictionary(b => b.Id);
        foreach (var item in dto.Items)
        {
            if (warehouse.Type == WarehouseType.Storge && !item.ProductBarCodeId.HasValue ||
                item.ProductBarCodeId.HasValue && (!barcodes.TryGetValue(item.ProductBarCodeId.Value, out var barcode) || barcode.ProductId != item.ProductId))
                return ServiceResult<StockAdjustmentResponseDto>.Failure("الباركود مطلوب للمخزن ويجب أن يتبع الصنف المحدد", ErrorCodes.ValidationError);
        }
        if (warehouse.Type == WarehouseType.Show && dto.Items.Select(i => i.ProductId).Distinct().Count() != dto.Items.Count ||
            warehouse.Type == WarehouseType.Storge && barcodeIds.Count != dto.Items.Count)
            return ServiceResult<StockAdjustmentResponseDto>.Failure("لا يجوز تكرار الصنف أو الباركود في التسوية", ErrorCodes.ValidationError);

        var adjustment = _mapper.Map<StockAdjustment>(dto);
        adjustment.TenantId = warehouse.TenantId;
        adjustment.AdjustmentDate = dto.AdjustmentDate == default ? DateTime.UtcNow : dto.AdjustmentDate;
        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var showroom = warehouse.Type == WarehouseType.Show
                ? (await _unitOfWork.Stock.GetShowroomForUpdateAsync(warehouse.Id, productIds, ct)).ToDictionary(s => s.ProductId)
                : new Dictionary<Guid, ShowroomStock>();
            var storage = warehouse.Type == WarehouseType.Storge
                ? (await _unitOfWork.Stock.GetStorageForUpdateAsync(warehouse.Id, barcodeIds, ct)).ToDictionary(s => s.ProductBarcodeId)
                : new Dictionary<Guid, StorgeStock>();
            foreach (var item in dto.Items)
            {
                var systemQuantity = 0;
                if (warehouse.Type == WarehouseType.Show)
                {
                    if (!showroom.TryGetValue(item.ProductId, out var stock))
                    {
                        stock = new ShowroomStock { TenantId = warehouse.TenantId, WarehouseId = warehouse.Id, ProductId = item.ProductId };
                        await _unitOfWork.Stock.AddShowroomStockAsync(stock, ct);
                    }
                    systemQuantity = stock.Quantity;
                    stock.Quantity = item.ActualQuantity;
                }
                else
                {
                    if (!storage.TryGetValue(item.ProductBarCodeId!.Value, out var stock))
                    {
                        stock = new StorgeStock { TenantId = warehouse.TenantId, WarehouseId = warehouse.Id, ProductBarcodeId = item.ProductBarCodeId.Value };
                        await _unitOfWork.Stock.AddStorgeStockAsync(stock, ct);
                    }
                    systemQuantity = stock.Quantity;
                    stock.Quantity = item.ActualQuantity;
                }
                adjustment.Items.Add(new StockAdjustmentItem
                {
                    TenantId = warehouse.TenantId, ProductId = item.ProductId, ProductBarCodeId = item.ProductBarCodeId,
                    SystemQuantity = systemQuantity, ActualQuantity = item.ActualQuantity,
                    DifferenceQuantity = checked(item.ActualQuantity - systemQuantity), UnitCost = item.UnitCost,
                    Reason = item.Reason ?? adjustment.Reason
                });
            }
            await _unitOfWork.StockAdjustments.AddAsync(adjustment, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            await _unitOfWork.CommitTransactionAsync(ct);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(ct);
            throw;
        }
        var created = await _unitOfWork.StockAdjustments.GetDetailsAsync(adjustment.Id, ct) ?? adjustment;
        return ServiceResult<StockAdjustmentResponseDto>.Success(_mapper.Map<StockAdjustmentResponseDto>(created));
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام سند التسوية ككيان متتبع
        var adjustment = await _unitOfWork.StockAdjustments.GetDetailsForUpdateAsync(id, ct);
        // التحقق من وجود التسوية
        if (adjustment is null)
        {
            // إرجاع خطأ عدم وجود التسوية
            return ServiceResult.Failure("التسوية الجردية غير موجودة", ErrorCodes.StockAdjustmentNotFound);
        }

        return ServiceResult.Failure("التسوية مرحلة ولا يمكن حذفها؛ سجل تسوية جديدة لعكس أثرها مع الاحتفاظ بسجل الجرد", ErrorCodes.ValidationError);
    }
}
