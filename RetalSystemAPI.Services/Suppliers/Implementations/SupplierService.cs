using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Services.Common.Models;
using RetalSystemAPI.Services.Suppliers.Interfaces;
using RetalSystemAPI.Services.Suppliers.Specifications;

namespace RetalSystemAPI.Services.Suppliers.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة الموردين، حساباتهم، وجهات الاتصال وأرقام الهواتف التابعة لهم.
/// </summary>
public class SupplierService : ISupplierService
{
    // وحدة العمل للوصول إلى مستودعات الموردين وهواتفهم وحفظ التعديلات
    private readonly IUnitOfWork _unitOfWork;

    // محول النماذج للتحويل التلقائي بين الكيانات وDTOs
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة الموردين مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name="mapper">محول الكيانات</param>
    public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;

        // تعيين مرجع المحول
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SupplierResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // البحث عن المورد بالمعرف مع تضمين أرقام هواتفه
        var supplier = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(id), ct);

        // التحقق من وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم العثور على المورد
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // تحويل الكيان إلى كائن استجابة DTO
        var dto = _mapper.Map<SupplierResponseDto>(supplier);

        // إرجاع النتيجة بنجاح
        return ServiceResult<SupplierResponseDto>.Success(dto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<SupplierSummaryDto>>> GetAllAsync(CancellationToken ct = default)
    {
        // استرجاع كافة الموردين مع هواتفهم مرتبين بالاسم
        var suppliers = await _unitOfWork.Suppliers.FindAsync(new SupplierWithDetailsSpec(), ct);

        // تحويل قائمة الكيانات إلى قائمة ملخصات DTOs
        var dtos = _mapper.Map<IReadOnlyList<SupplierSummaryDto>>(suppliers);

        // إرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<SupplierSummaryDto>>.Success(dtos);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, string? search = null, CancellationToken ct = default)
    {
        // تجهيز مواصفة الاستعلام المخصصة بالبحث والترتيب
        var spec = new SupplierWithDetailsSpec(search);

        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var (items, totalCount) = await _unitOfWork.Suppliers.GetPagedAsync(spec, pageNumber, pageSize, ct);

        // تحويل قائمة العناصر المسترجعة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<SupplierSummaryDto>>(items);

        // بناء كائن النتيجة المجزأة الموحد
        var pagedResult = PagedResult<SupplierSummaryDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<SupplierSummaryDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SupplierResponseDto>> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default)
    {
        // التحقق من عدم تكرار اسم المورد للمستأجر الحالي
        bool exists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Name == dto.Name, ct);

        // في حال وجود مورد آخر مسجل بنفس الاسم
        if (exists)
        {
            // إرجاع خطأ تكرار اسم المورد
            return ServiceResult<SupplierResponseDto>.Failure("اسم المورد موجود بالفعل", ErrorCodes.SupplierNameExists);
        }

        // تحويل كائن DTO إلى كيان المورد
        var supplier = _mapper.Map<Supplier>(dto);

        // التحقق من وجود هواتف مرفقة مع المورد الجديد
        if (dto.Phones != null && dto.Phones.Any())
        {
            // بناء كيانات هواتف المورد
            supplier.SupplierPhones = dto.Phones.Select(p => new SupplierPhone
            {
                // تعيين رقم الهاتف
                PhoneNumber = p.PhoneNumber,
                // تعيين اسم جهة الاتصال
                Name = p.Name
            }).ToList();
        }

        // إضافة كيان المورد وهواتفه إلى المستودع
        await _unitOfWork.Suppliers.AddAsync(supplier, ct);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب المورد المحفوظ مع هواتفه
        var created = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(supplier.Id), ct) ?? supplier;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SupplierResponseDto>(created);

        // إرجاع نتيجة النجاح
        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SupplierResponseDto>> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken ct = default)
    {
        // البحث عن المورد الحالي مع هواتفه
        var supplier = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(id), ct);

        // التحقق من وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم وجود المورد
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // التحقق من عدم تعارض الاسم الجديد مع مورد آخر
        bool nameExists = await _unitOfWork.Suppliers.ExistsAsync(s => s.Name == dto.Name && s.Id != id, ct);

        // في حال وجود مورد آخر بنفس الاسم
        if (nameExists)
        {
            // إرجاع خطأ تكرار الاسم
            return ServiceResult<SupplierResponseDto>.Failure("اسم المورد مستخدم بالفعل لمورد آخر", ErrorCodes.SupplierNameExists);
        }

        // نقل البيانات المحدثة من DTO إلى الكيان
        _mapper.Map(dto, supplier);

        // ضمان ثبات معرف المورد الأصلي
        supplier.Id = id;

        // وسم الكيان للتحديث في المستودع
        _unitOfWork.Suppliers.Update(supplier);

        // حفظ التعديلات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب المورد المحدث مع هواتفه
        var updated = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(id), ct) ?? supplier;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SupplierResponseDto>(updated);

        // إرجاع نتيجة النجاح
        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // جلب سجل المورد بالمعرف
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, ct);

        // التحقق من وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم العثور على المورد
            return ServiceResult.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // تطبيق الحذف المنطقي للمورد
        _unitOfWork.Suppliers.SoftDelete(supplier);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult<SupplierResponseDto>> AddPhoneAsync(Guid supplierId, SupplierPhoneDto dto, CancellationToken ct = default)
    {
        // البحث عن المورد بالمعرف
        var supplier = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(supplierId), ct);

        // التحقق من وجود المورد
        if (supplier is null)
        {
            // إرجاع خطأ عدم وجود المورد
            return ServiceResult<SupplierResponseDto>.Failure("المورد غير موجود", ErrorCodes.SupplierNotFound);
        }

        // إنشاء كيان رقم الهاتف الجديد
        var phone = new SupplierPhone
        {
            // تعيين معرف المورد
            SupplierId = supplierId,
            // تعيين رقم الهاتف
            PhoneNumber = dto.PhoneNumber,
            // تعيين اسم جهة الاتصال
            Name = dto.Name
        };

        // إضافة الهاتف إلى المستودع
        await _unitOfWork.SupplierPhones.AddAsync(phone, ct);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب المورد مع قائمة الهواتف المحدثة
        var updated = await _unitOfWork.Suppliers.FirstOrDefaultAsync(new SupplierWithDetailsSpec(supplierId), ct) ?? supplier;

        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<SupplierResponseDto>(updated);

        // إرجاع نتيجة النجاح
        return ServiceResult<SupplierResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeletePhoneAsync(Guid supplierId, Guid phoneId, CancellationToken ct = default)
    {
        // جلب سجل الهاتف بالمعرف
        var phone = await _unitOfWork.SupplierPhones.GetByIdAsync(phoneId, ct);

        // التحقق من وجود الهاتف وارتباطه بنفس المورد المحدد
        if (phone is null || phone.SupplierId != supplierId)
        {
            // إرجاع خطأ عدم وجود الهاتف أو عدم التبعية
            return ServiceResult.Failure("رقم الهاتف غير موجود أو لا ينتمي لهذا المورد", ErrorCodes.NotFound);
        }

        // تطبيق الحذف المنطقي للهاتف
        _unitOfWork.SupplierPhones.SoftDelete(phone);

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }
}
