using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Warehouses.Interfaces;
using RetalSystemAPI.Services.Warehouses.Specifications;

namespace RetalSystemAPI.Services.Warehouses.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة المستودعات وصالات العرض وتوليد سجلات الأرصدة الافتتاحية والتحقق من سلامة المخزون قبل الحذف.
/// </summary>
public class WarehouseService : IWarehouseService
{
    // وحدة العمل للوصول إلى مستودعات البيانات وحفظ التغييرات
    private readonly IUnitOfWork _unitOfWork;

    // محول النماذج للتحويل بين الكيانات وDTOs
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة المستودعات مع حقن وحدة العمل والمحول.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name="mapper">محول الكيانات</param>
    public WarehouseService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;

        // تعيين مرجع المحول
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WarehouseResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام المستودع بالمعرف مع تفاصيل الفرع عبر المواصفة
        var warehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(new WarehouseWithDetailsSpec(id), ct);

        // التحقق من وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult<WarehouseResponseDto>.Failure("المخزن أو الصالة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // تحويل الكيان إلى DTO
        var dto = _mapper.Map<WarehouseResponseDto>(warehouse);

        // إرجاع النتيجة بنجاح
        return ServiceResult<WarehouseResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<WarehouseSummaryDto>>> GetAllAsync(Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default)
    {
        // إعداد مواصفة الفلترة بالفرع والنوع
        var spec = new WarehouseWithDetailsSpec(branchId, type);

        // جلب قائمة المستودعات المطابقة للمواصفة
        var warehouses = await _unitOfWork.Warehouses.FindAsync(spec, ct);

        // تحويل قائمة الكيانات إلى قائمة ملخصات DTO
        var dtos = _mapper.Map<IReadOnlyList<WarehouseSummaryDto>>(warehouses);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<WarehouseSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<WarehouseSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة الفلترة بالفرع والنوع
        var spec = new WarehouseWithDetailsSpec(branchId, type);

        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var (items, totalCount) = await _unitOfWork.Warehouses.GetPagedAsync(spec, pageNumber, pageSize, ct);

        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<WarehouseSummaryDto>>(items);

        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<WarehouseSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<WarehouseSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WarehouseResponseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default)
    {
        // التحقق من وجود الفرع المحدد
        bool branchExists = await _unitOfWork.Branches.ExistsAsync(b => b.Id == dto.BranchId, ct);

        // إذا لم يكن الفرع موجوداً
        if (!branchExists)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<WarehouseResponseDto>.Failure("الفرع المحدد غير موجود", ErrorCodes.BranchNotFound);
        }

        // التحقق من عدم تكرار اسم المستودع ضمن نفس الفرع
        bool nameExists = await _unitOfWork.Warehouses.ExistsAsync(w => w.Name == dto.Name && w.BranchId == dto.BranchId, ct);

        // في حال وجود مستودع بنفس الاسم في الفرع
        if (nameExists)
        {
            // إرجاع خطأ تكرار الاسم
            return ServiceResult<WarehouseResponseDto>.Failure("اسم المخزن/الصالة مستخدم بالفعل في هذا الفرع", ErrorCodes.WarehouseNameExists);
        }

        // تحويل بيانات DTO إلى كيان المستودع
        var warehouse = _mapper.Map<Warehouse>(dto);

        // تعيين حالة النشاط افتراضياً بنشط
        warehouse.IsActive = true;

        // إضافة كيان المستودع إلى المستودع
        await _unitOfWork.Warehouses.AddAsync(warehouse, ct);

        // حفظ التغييرات للحصول على المعرف المنشأ
        await _unitOfWork.SaveChangesAsync(ct);

        // توليد أسطر المخزون التلقائية الافتتاحية للمخزن الجديد
        if (warehouse.Type == WarehouseType.Storge)
        {
            // جلب معرفات كافة الباركودات المسجلة
            var allBarCodeIds = await _unitOfWork.ProductBarCodes.SelectAsync(bc => bc.Id, ct);

            // إنشاء سجل رصيد افتتاحي لكل باركود بكمية صفرية
            var newStocks = allBarCodeIds.Select(bcId => new StorgeStock
            {
                TenantId = warehouse.TenantId,
                WarehouseId = warehouse.Id,
                ProductBarcodeId = bcId,
                Quantity = 0,
                MinStockLevel = 0
            }).ToList();

            // إضافة سجلات الأرصدة كدفعة واحدة
            await _unitOfWork.StorgeStocks.AddRangeAsync(newStocks, ct);

            // حفظ التغييرات في حال وجود سجلات
            if (newStocks.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
        else if (warehouse.Type == WarehouseType.Show)
        {
            // جلب معرفات كافة المنتجات المسجلة لصالة العرض
            var allProductIds = await _unitOfWork.Products.SelectAsync(p => p.Id, ct);

            // إنشاء سجل رصيد افتتاحي لكل منتج بكمية صفرية
            var newStocks = allProductIds.Select(pId => new ShowroomStock
            {
                TenantId = warehouse.TenantId,
                WarehouseId = warehouse.Id,
                ProductId = pId,
                Quantity = 0,
                MinStockLevel = 0
            }).ToList();

            // إضافة سجلات الأرصدة كدفعة واحدة
            await _unitOfWork.ShowroomStocks.AddRangeAsync(newStocks, ct);

            // حفظ التغييرات في حال وجود سجلات
            if (newStocks.Count > 0)
            {
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }

        // إعادة جلب المستودع مع بيانات الفرع
        var created = await _unitOfWork.Warehouses.FirstOrDefaultAsync(new WarehouseWithDetailsSpec(warehouse.Id), ct) ?? warehouse;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<WarehouseResponseDto>(created);

        // إرجاع نتيجة النجاح
        return ServiceResult<WarehouseResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<WarehouseResponseDto>> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken ct = default)
    {
        // استعلام المستودع الحالي بالمعرف
        var warehouse = await _unitOfWork.Warehouses.FirstOrDefaultAsync(new WarehouseWithDetailsSpec(id), ct);

        // التحقق من وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult<WarehouseResponseDto>.Failure("المخزن أو الصالة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // التحقق من عدم تعارض الاسم الجديد مع مستودع آخر في نفس الفرع
        bool nameExists = await _unitOfWork.Warehouses.ExistsAsync(w => w.Name == dto.Name && w.BranchId == warehouse.BranchId && w.Id != id, ct);

        // في حال وجود تعارض
        if (nameExists)
        {
            // إرجاع خطأ تكرار الاسم
            return ServiceResult<WarehouseResponseDto>.Failure("اسم المخزن/الصالة مستخدم بالفعل في هذا الفرع", ErrorCodes.WarehouseNameExists);
        }

        // تطبيق التعديلات على كيان المستودع
        _mapper.Map(dto, warehouse);

        // تثبيت معرف المستودع
        warehouse.Id = id;

        // وسم الكيان للتحديث
        _unitOfWork.Warehouses.Update(warehouse);

        // حفظ التعديلات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب المستودع المحدث مع التفاصيل
        var updated = await _unitOfWork.Warehouses.FirstOrDefaultAsync(new WarehouseWithDetailsSpec(id), ct) ?? warehouse;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<WarehouseResponseDto>(updated);

        // إرجاع نتيجة النجاح
        return ServiceResult<WarehouseResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام المستودع بالمعرف
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, ct);

        // التحقق من وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult.Failure("المخزن أو الصالة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // فحص وجود كميات مخزنية في مخازن التخزين التابعة لهذا المستودع
        bool hasStorgeStock = await _unitOfWork.StorgeStocks.ExistsAsync(s => s.WarehouseId == id && s.Quantity > 0, ct);

        // فحص وجود كميات مخزنية في صالات العرض التابعة لهذا المستودع
        bool hasShowroomStock = await _unitOfWork.ShowroomStocks.ExistsAsync(s => s.WarehouseId == id && s.Quantity > 0, ct);

        // منع الحذف في حال وجود أي أرصدة موجبة لحماية البيانات المالية
        if (hasStorgeStock || hasShowroomStock)
        {
            // إرجاع خطأ احتواء المستودع على مخزون
            return ServiceResult.Failure("لا يمكن حذف المخزن/الصالة لوجود كميات مخزونية مسجلة بها", ErrorCodes.WarehouseHasStock);
        }

        // تطبيق الحذف المنطقي للمستودع
        _unitOfWork.Warehouses.SoftDelete(warehouse);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام المستودع بالمعرف
        var warehouse = await _unitOfWork.Warehouses.GetByIdAsync(id, ct);

        // التحقق من وجود المستودع
        if (warehouse is null)
        {
            // إرجاع خطأ عدم وجود المستودع
            return ServiceResult.Failure("المخزن أو الصالة غير موجودة", ErrorCodes.WarehouseNotFound);
        }

        // تبديل حالة النشاط الحالية
        warehouse.IsActive = !warehouse.IsActive;

        // وسم الكيان للتحديث
        _unitOfWork.Warehouses.Update(warehouse);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
