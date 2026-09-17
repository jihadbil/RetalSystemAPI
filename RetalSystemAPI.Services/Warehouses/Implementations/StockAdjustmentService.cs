using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;
using RetalSystemAPI.Services.Warehouses.Specifications;

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
    /// <param name="unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name="mapper">محول الكيانات</param>
    public StockAdjustmentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;

        // تعيين مرجع المحول
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<StockAdjustmentResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام التسوية الجردية بالمعرف مع تفاصيل البنود والمنتجات
        var adjustment = await _unitOfWork.StockAdjustments.FirstOrDefaultAsync(new StockAdjustmentWithDetailsSpec(id), ct);

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

    /// <inheritdoc />
    public async Task<ServiceResult<StockAdjustmentResponseDto>> GetByAdjustmentNumberAsync(string adjustmentNumber, CancellationToken ct = default)
    {
        // استعلام التسوية الجردية برقم السند
        var adjustment = await _unitOfWork.StockAdjustments.FirstOrDefaultAsync(new StockAdjustmentWithDetailsSpec(adjustmentNumber), ct);

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

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>> GetAllAsync(
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام التسويات مع الفلاتر
        var spec = new StockAdjustmentWithDetailsSpec(warehouseId, reason, fromDate, toDate, search);

        // جلب قائمة التسويات المطابقة
        var adjustments = await _unitOfWork.StockAdjustments.FindAsync(spec, ct);

        // تحويل الكيانات إلى ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<StockAdjustmentSummaryDto>>(adjustments);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<StockAdjustmentSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default)
    {
        // تجهيز مواصفة الاستعلام المفلترة
        var spec = new StockAdjustmentWithDetailsSpec(warehouseId, reason, fromDate, toDate, search);

        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var (items, totalCount) = await _unitOfWork.StockAdjustments.GetPagedAsync(spec, pageNumber, pageSize, ct);

        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<StockAdjustmentSummaryDto>>(items);

        // إنشاء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<StockAdjustmentSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<StockAdjustmentSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<StockAdjustmentResponseDto>> CreateAsync(CreateStockAdjustmentDto dto, CancellationToken ct = default)
    {
        // استعلام المستودع للتأكد من وجوده ومعرفة نوعه (صالة أم مخزن)
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(dto.WarehouseId, ct);

        // التحقق من وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult<StockAdjustmentResponseDto>.Failure("المستودع أو الصالة المحددة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من عدم تكرار رقم التسوية الجردية
        bool numExists = await _unitOfWork.StockAdjustments.ExistsAsync(a => a.AdjustmentNumber == dto.AdjustmentNumber, ct);

        // في حال وجود سند تسوية بنفس الرقم
        if (numExists)
        {
            // إرجاع خطأ تكرار رقم التسوية
            return ServiceResult<StockAdjustmentResponseDto>.Failure("رقم التسوية الجردية مستخدم بالفعل", ErrorCodes.StockAdjustmentNumberExists);
        }

        // التحقق من احتواء التسوية على بند واحد على الأقل
        if (dto.Items == null || !dto.Items.Any())
        {
            // إرجاع خطأ تحقق لغياب البنود
            return ServiceResult<StockAdjustmentResponseDto>.Failure("يجب إضافة بند واحد على الأقل للتسوية الجردية", ErrorCodes.ValidationError);
        }

        // تحويل كائن DTO إلى كيان التسوية الجردية
        var adjustment = _mapper.Map<StockAdjustment>(dto);

        // تعيين تاريخ التسوية الجردية
        adjustment.AdjustmentDate = dto.AdjustmentDate == default ? DateTime.UtcNow : dto.AdjustmentDate;

        // بناء بنود التسوية مع حساب فوارق الجرد تلقائياً
        adjustment.Items = dto.Items.Select(item => new StockAdjustmentItem
        {
            // تعيين معرف المنتج
            ProductId = item.ProductId,
            // تعيين معرف الباركود
            ProductBarCodeId = item.ProductBarCodeId,
            // الكمية المقيدة دفترياً
            SystemQuantity = item.SystemQuantity,
            // الكمية الفعلية الناتجة عن الجرد
            ActualQuantity = item.ActualQuantity,
            // الفارق بين الفعلي والدفتري (موجب = زيادة، سالب = عجز)
            DifferenceQuantity = item.ActualQuantity - item.SystemQuantity,
            // تكلفة الوحدة
            UnitCost = item.UnitCost,
            // سبب البند إن حُدد، وإلا يتم توريث سبب التسوية العام
            Reason = item.Reason ?? adjustment.Reason
        }).ToList();

        // تحديث أرصدة المخزون بناءً على الكميات الفعلية للجرد دفعة واحدة
        if (warehouse.Type == WarehouseType.Show)
        {
            // استخراج معرفات منتجات صالة العرض
            var productIds = adjustment.Items.Select(i => i.ProductId).Distinct().ToList();

            // استرجاع أرصدة الصالة المتتبعة في قاموس
            var stocksByProduct = productIds.Count > 0
                ? (await _unitOfWork.ShowroomStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && productIds.Contains(s.ProductId), ct))
                    .ToDictionary(s => s.ProductId)
                : new Dictionary<Guid, ShowroomStock>();

            // ضبط الكمية لكل بند بناءً على الجرد الفعلي
            foreach (var item in adjustment.Items)
            {
                // إذا كان سجل الرصيد موجوداً يتم تعديله
                if (stocksByProduct.TryGetValue(item.ProductId, out var stock))
                {
                    // مطابقة الرصيد مع الكمية الفعلية
                    stock.Quantity = item.ActualQuantity;
                }
                else
                {
                    // إنشاء سجل رصيد جديد في الصالة بالكمية الفعلية
                    var newStock = new ShowroomStock
                    {
                        TenantId = adjustment.TenantId,
                        WarehouseId = warehouse.Id,
                        ProductId = item.ProductId,
                        Quantity = item.ActualQuantity,
                        MinStockLevel = 0
                    };
                    // إضافة السجل للمستودع
                    await _unitOfWork.ShowroomStocks.AddAsync(newStock, ct);
                    // حفظه بالقاموس
                    stocksByProduct[item.ProductId] = newStock;
                }
            }
        }
        else if (warehouse.Type == WarehouseType.Storge)
        {
            // استخراج معرفات الباركودات لمخزن التخزين
            var barcodeIds = adjustment.Items
                .Where(i => i.ProductBarCodeId.HasValue)
                .Select(i => i.ProductBarCodeId!.Value)
                .Distinct()
                .ToList();

            // استرجاع أرصدة المخزن المتتبعة في قاموس
            var stocksByBarcode = barcodeIds.Count > 0
                ? (await _unitOfWork.StorgeStocks.FindTrackedAsync(
                    s => s.WarehouseId == warehouse.Id && barcodeIds.Contains(s.ProductBarcodeId), ct))
                    .ToDictionary(s => s.ProductBarcodeId)
                : new Dictionary<Guid, StorgeStock>();

            // ضبط الكمية لكل باركود
            foreach (var item in adjustment.Items)
            {
                // التحقق من وجود معرف الباركود
                if (!item.ProductBarCodeId.HasValue) continue;

                // إذا كان سجل الرصيد موجوداً يتم تعديله
                if (stocksByBarcode.TryGetValue(item.ProductBarCodeId.Value, out var stock))
                {
                    // مطابقة الرصيد مع الكمية الفعلية
                    stock.Quantity = item.ActualQuantity;
                }
                else
                {
                    // إنشاء سجل رصيد جديد بالكمية الفعلية
                    var newStock = new StorgeStock
                    {
                        TenantId = adjustment.TenantId,
                        WarehouseId = warehouse.Id,
                        ProductBarcodeId = item.ProductBarCodeId.Value,
                        Quantity = item.ActualQuantity,
                        MinStockLevel = 0
                    };
                    // إضافة السجل للمستودع
                    await _unitOfWork.StorgeStocks.AddAsync(newStock, ct);
                    // حفظه بالقاموس
                    stocksByBarcode[item.ProductBarCodeId.Value] = newStock;
                }
            }
        }

        // إضافة سند التسوية إلى المستودع
        await _unitOfWork.StockAdjustments.AddAsync(adjustment, ct);

        // حفظ كافة التغييرات وحركات المخزون في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب التسوية مع تفاصيل المستودع والبنود
        var created = await _unitOfWork.StockAdjustments.FirstOrDefaultAsync(new StockAdjustmentWithDetailsSpec(adjustment.Id), ct) ?? adjustment;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<StockAdjustmentResponseDto>(created);

        // إرجاع النتيجة بنجاح
        return ServiceResult<StockAdjustmentResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام سند التسوية ككيان متتبع
        var adjustment = await _unitOfWork.StockAdjustments.FirstOrDefaultTrackedAsync(new StockAdjustmentWithDetailsSpec(id), ct);

        // التحقق من وجود التسوية
        if (adjustment is null)
        {
            // إرجاع خطأ عدم وجود التسوية
            return ServiceResult.Failure("التسوية الجردية غير موجودة", ErrorCodes.StockAdjustmentNotFound);
        }

        // تطبيق الحذف المنطقي لسند التسوية
        _unitOfWork.StockAdjustments.SoftDelete(adjustment);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
