using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Tenant.Interfaces;
using TenantEntity = RetalSystemAPI.Models.Tenant;

namespace RetalSystemAPI.Services.Tenant.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة المستأجرين على مستوى المنظومة وتدقيق الأسماء وحالات النشاط وإحصائيات المنشأة.
/// </summary>
public class TenantService : ITenantService
{
    // وحدة العمل للتعامل مع مستودع المستأجرين وحفظ التغييرات
    private readonly IUnitOfWork _unitOfWork;

    // سياق قاعدة البيانات للاستعلامات المباشرة غير المقيدة بفلاتر الأمان (مثل إحصائيات المستأجر)
    private readonly AppDbContext _dbContext;

    // محول الكيانات لتحويل الكائنات بين نماذج قواعد البيانات وDTOs
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة المستأجرين مع حقن وحدة العمل وسياق البيانات والمحول.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name="dbContext">سياق قاعدة البيانات للعمليات المتقدمة</param>
    /// <param name="mapper">محول الكائنات لنماذج DTO</param>
    public TenantService(IUnitOfWork unitOfWork, AppDbContext dbContext, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;

        // تعيين مرجع سياق قاعدة البيانات
        _dbContext = dbContext;

        // تعيين مرجع المحول
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // البحث عن المستأجر في المستودع بالمعرف المحدد
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);

        // التحقق من وجود المستأجر
        if (tenant is null)
        {
            // إرجاع خطأ عدم العثور على المستأجر
            return ServiceResult<TenantResponseDto>.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        // تحويل كيان المستأجر إلى كائن استجابة DTO
        var result = _mapper.Map<TenantResponseDto>(tenant);

        // تعبئة إحصائيات الفروع والمستودعات والمستخدمين التابعين له
        await PopulateTenantStatsAsync(result, id, ct);

        // إرجاع نتيجة النجاح مع البيانات
        return ServiceResult<TenantResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        // استرجاع كافة سجلات المستأجرين غير المحذوفة
        var tenants = await _unitOfWork.Tenants.GetAllAsync(ct);

        // تحويل قائمة الكيانات إلى قائمة كائنات الاستجابة DTO
        var result = _mapper.Map<IReadOnlyList<TenantResponseDto>>(tenants);

        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<TenantResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<TenantResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        // جلب صفحة بيانات مجزأة من مستودع المستأجرين مرتبة تنازلياً بحسب تاريخ الإنشاء
        var (items, totalCount) = await _unitOfWork.Tenants.GetPagedAsync(
            pageNumber,
            pageSize,
            orderBy: t => t.CreatedAt,
            ascending: false,
            ct: ct);

        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<TenantResponseDto>>(items);

        // بناء كائن النتيجة المجزأة الموحد مع إجمالي السجلات والصفحات
        var pagedResult = PagedResult<TenantResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة الناجحة
        return ServiceResult<PagedResult<TenantResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TenantResponseDto>> CreateAsync(CreateTenantDto dto, CancellationToken ct = default)
    {
        // التحقق مما إذا كان اسم المستأجر مستخدماً بالفعل
        bool nameExists = await _unitOfWork.Tenants.ExistsAsync(t => t.Name == dto.Name, ct);

        // إذا كان الاسم مسجلاً من قبل
        if (nameExists)
        {
            // إرجاع خطأ تكرار اسم المستأجر
            return ServiceResult<TenantResponseDto>.Failure("اسم المستأجر مستخدم بالفعل", ErrorCodes.TenantNameExists);
        }

        // تحويل بيانات DTO إلى كيان المستأجر
        var tenant = _mapper.Map<TenantEntity>(dto);

        // إضافة المستأجر الجديد إلى المستودع
        await _unitOfWork.Tenants.AddAsync(tenant, ct);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // تحويل الكيان المحفوظ إلى كائن استجابة DTO
        var responseDto = _mapper.Map<TenantResponseDto>(tenant);

        // إرجاع النتيجة الناجحة مع بيانات المستأجر المنشأ
        return ServiceResult<TenantResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto dto, CancellationToken ct = default)
    {
        // البحث عن المستأجر الحالي المراد تعديله
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);

        // التحقق من وجود المستأجر
        if (tenant is null)
        {
            // إرجاع خطأ عدم العثور على المستأجر
            return ServiceResult<TenantResponseDto>.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        // التحقق من عدم استخدام الاسم الجديد لدى مستأجر آخر مختلف
        bool nameExists = await _unitOfWork.Tenants.ExistsAsync(t => t.Name == dto.Name && t.Id != id, ct);

        // إذا وجد مستأجر آخر بنفس الاسم
        if (nameExists)
        {
            // إرجاع خطأ تعارض الاسم
            return ServiceResult<TenantResponseDto>.Failure("اسم المستأجر مستخدم بالفعل لدى مستأجر آخر", ErrorCodes.TenantNameExists);
        }

        // نقل وتطبيق التعديلات من DTO إلى الكيان الأصلي
        _mapper.Map(dto, tenant);

        // ضمان ثبات معرف المستأجر الأصلي وعدم استبداله
        tenant.Id = id;

        // وسم الكيان للتحديث في المستودع
        _unitOfWork.Tenants.Update(tenant);

        // حفظ التعديلات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // تحويل الكيان المحدث إلى DTO
        var responseDto = _mapper.Map<TenantResponseDto>(tenant);

        // جلب وتعبئة الإحصائيات المحدثة
        await PopulateTenantStatsAsync(responseDto, id, ct);

        // إرجاع نتيجة التحديث بنجاح
        return ServiceResult<TenantResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TenantResponseDto>> UpdateLogoAsync(Guid id, string logoUrl, CancellationToken ct = default)
    {
        // جلب سجل المستأجر من المستودع
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);

        // التحقق من وجود المستأجر
        if (tenant is null)
        {
            // إرجاع خطأ عدم العثور على المستأجر
            return ServiceResult<TenantResponseDto>.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        // تعيين رابط الشعار الجديد
        tenant.LogoUrl = logoUrl;

        // وسم السجل للتحديث
        _unitOfWork.Tenants.Update(tenant);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // تحويل الكيان إلى كائن الاستجابة
        var responseDto = _mapper.Map<TenantResponseDto>(tenant);

        // تعبئة إحصائيات المنشأة
        await PopulateTenantStatsAsync(responseDto, id, ct);

        // إرجاع نتيجة النجاح
        return ServiceResult<TenantResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // جلب سجل المستأجر المراد حذفه
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);

        // التحقق من وجود المستأجر
        if (tenant is null)
        {
            // إرجاع خطأ عدم العثور
            return ServiceResult.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        // تطبيق الحذف المنطقي (Soft Delete) بتعيين IsDeleted = true
        _unitOfWork.Tenants.SoftDelete(tenant);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        // البحث عن المستأجر في المستودع
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, ct);

        // التحقق من وجود المستأجر
        if (tenant is null)
        {
            // إرجاع خطأ عدم العثور
            return ServiceResult.Failure("المستأجر غير موجود", ErrorCodes.TenantNotFound);
        }

        // عكس حالة النشاط الحالية
        tenant.IsActive = !tenant.IsActive;

        // وسم المستأجر للتحديث
        _unitOfWork.Tenants.Update(tenant);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <summary>
    /// تعبئة الإحصائيات المرتبطة بالمستأجر مثل عدد الفروع والمستودعات والمستخدمين بتجاوز فلاتر العزل للاستعلام الشامل.
    /// </summary>
    /// <param name="dto">كائن بيانات استجابة المستأجر</param>
    /// <param name="tenantId">معرف المستأجر الفريد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    private async Task PopulateTenantStatsAsync(TenantResponseDto dto, Guid tenantId, CancellationToken ct)
    {
        // حساب إجمالي عدد الفروع غير المحذوفة التابعة لهذا المستأجر
        dto.BranchesCount = await _dbContext.Branches.IgnoreQueryFilters().CountAsync(b => b.TenantId == tenantId && !b.IsDeleted, ct);

        // حساب إجمالي عدد المستودعات غير المحذوفة التابعة لهذا المستأجر
        dto.WarehousesCount = await _dbContext.Warehouses.IgnoreQueryFilters().CountAsync(w => w.TenantId == tenantId && !w.IsDeleted, ct);

        // حساب إجمالي عدد المستخدمين المسجلين تحت مظلة هذا المستأجر
        dto.UsersCount = await _dbContext.Users.IgnoreQueryFilters().CountAsync(u => u.TenantId == tenantId, ct);
    }
}
