using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة وحدات القياس والتحقق من عدم تكرار الأسماء والتحقق من عدم وجود ارتباطات سابقة عند الحذف.
/// </summary>
public class UnitService : IUnitService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة وحدات القياس مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل النماذج</param>
    public UnitService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UnitResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام وحدة القياس بواسطة المعرف الفريد
        var unit = await _unitOfWork.Units.GetByIdAsync(id, ct);
        // التحقق من وجود الوحدة
        if (unit is null)
        {
            // إرجاع خطأ بعدم وجود وحدة القياس
            return ServiceResult<UnitResponseDto>.Failure("الوحدة غير موجودة", ErrorCodes.UnitNotFound);
        }

        // تحويل الكيان إلى كائن الاستجابة المنقول
        var result = _mapper.Map<UnitResponseDto>(unit);
        // إرجاع النتيجة الناجحة
        return ServiceResult<UnitResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<UnitResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        // جلب قائمة بكافة وحدات القياس المسجلة في النظام
        var units = await _unitOfWork.Units.GetAllAsync(ct);
        // تحويل قائمة الكيانات إلى قائمة كائنات DTO
        var result = _mapper.Map<IReadOnlyList<UnitResponseDto>>(units);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<UnitResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UnitResponseDto>> CreateAsync(CreateUnitDto dto, CancellationToken ct = default)
    {
        // التحقق من عدم وجود وحدة قياس بنفس الاسم مسبقاً
        bool nameExists = await _unitOfWork.Units.ExistsAsync(u => u.Name == dto.Name, ct);
        // في حال وجود الاسم مسبقاً
        if (nameExists)
        {
            // إرجاع خطأ تكرار اسم الوحدة
            return ServiceResult<UnitResponseDto>.Failure("اسم الوحدة موجود بالفعل", ErrorCodes.UnitNameExists);
        }

        // تحويل بيانات الإدخال إلى كيان وحدة القياس
        var unit = _mapper.Map<Unit>(dto);
        // إضافة الوحدة الجديدة إلى المستودع
        await _unitOfWork.Units.AddAsync(unit, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // تحويل الكيان المنشأ إلى كائن استجابة DTO
        var responseDto = _mapper.Map<UnitResponseDto>(unit);
        // إرجاع النتيجة الناجحة
        return ServiceResult<UnitResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<UnitResponseDto>> UpdateAsync(Guid id, UpdateUnitDto dto, CancellationToken ct = default)
    {
        // استعلام وحدة القياس المطلوب تعديلها
        var unit = await _unitOfWork.Units.GetByIdAsync(id, ct);
        // التحقق من وجود الوحدة
        if (unit is null)
        {
            // إرجاع خطأ بعدم وجود الوحدة
            return ServiceResult<UnitResponseDto>.Failure("الوحدة غير موجودة", ErrorCodes.UnitNotFound);
        }

        // التحقق من عدم استخدام الاسم الجديد لدى وحدة قياس أخرى
        bool nameExists = await _unitOfWork.Units.ExistsAsync(u => u.Name == dto.Name && u.Id != id, ct);
        // في حال تكرار الاسم
        if (nameExists)
        {
            // إرجاع خطأ تكرار الاسم لدى وحدة أخرى
            return ServiceResult<UnitResponseDto>.Failure("اسم الوحدة موجود بالفعل لدى وحدة أخرى", ErrorCodes.UnitNameExists);
        }

        // نسخ التعديلات من DTO إلى الكيان
        _mapper.Map(dto, unit);
        // تثبيت المعرف الأصلي
        unit.Id = id;

        // تحديث الكيان في المستودع
        _unitOfWork.Units.Update(unit);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // تحويل الكيان المحدث إلى DTO الاستجابة
        var responseDto = _mapper.Map<UnitResponseDto>(unit);
        // إرجاع النتيجة الناجحة
        return ServiceResult<UnitResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام وحدة القياس المراد حذفها
        var unit = await _unitOfWork.Units.GetByIdAsync(id, ct);
        // التحقق من وجود الوحدة
        if (unit is null)
        {
            // إرجاع خطأ بعدم وجود الوحدة
            return ServiceResult.Failure("الوحدة غير موجودة", ErrorCodes.UnitNotFound);
        }

        // التحقق مما إذا كانت الوحدة مستخدمة في أي منتج قائم
        bool inUse = await _unitOfWork.ProductUnits.ExistsAsync(pu => pu.UnitId == id, ct);
        // في حال وجود ارتباطات قائمة
        if (inUse)
        {
            // منع الحذف وإرجاع رسالة توضيحية
            return ServiceResult.Failure("لا يمكن حذف الوحدة لأنها مرتبطة بمنتجات قائمة", ErrorCodes.UnitInUse);
        }

        // إجراء الحذف المنطقي لوحدة القياس
        _unitOfWork.Units.SoftDelete(unit);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
