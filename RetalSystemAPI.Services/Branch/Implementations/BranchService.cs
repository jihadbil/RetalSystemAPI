using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Services.Branch.Interfaces;
using RetalSystemAPI.Services.Common.Models;
using BranchEntity = RetalSystemAPI.Models.Branchs.Branch;

namespace RetalSystemAPI.Services.Branch.Implementations;
/// <summary>
/// تنفيذ خدمة الفروع لإدارة فروع المستأجر وتفاصيلها وأرقام هواتفها مع تطبيق قواعد العمل والتحقق من التكرار.
/// </summary>
public class BranchService : IBranchService
{
    // وحدة العمل للوصول إلى مستودعات الفروع وهواتفها وحفظ التغييرات
    private readonly IUnitOfWork _unitOfWork;
    // محول الكيانات لتحويل النماذج بين كيانات قاعدة البيانات ونماذج DTOs
    private readonly IMapper _mapper;
    /// <summary>
    /// تهيئة خدمة الفروع مع حقن وحدة العمل والمحول AutoMapper.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل للمستودعات</param>
    /// <param name = "mapper">محول الكيانات</param>
    public BranchService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل المحقون
        _unitOfWork = unitOfWork;
        // تعيين مرجع المحول المحقون
        _mapper = mapper;
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BranchResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام الفرع بالمعرف وتضمين أرقام هواتفه عبر المواصفة المخصصة
        var branch = await _unitOfWork.Branches.GetDetailsAsync(id, ct);
        // التحقق من وجود الفرع
        if (branch is null)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<BranchResponseDto>.Failure("الفرع غير موجود", ErrorCodes.BranchNotFound);
        }

        // تحويل كيان الفرع إلى كائن استجابة DTO
        var result = _mapper.Map<BranchResponseDto>(branch);
        // إرجاع نتيجة النجاح مع البيانات
        return ServiceResult<BranchResponseDto>.Success(result);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<IReadOnlyList<BranchResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        // استرجاع كافة فروع المستأجر مع هواتفها مرتبة بالاسم عبر المواصفة
        var branches = await _unitOfWork.Branches.ListAsync(new BranchQuery { }, ct);
        // تحويل قائمة الكيانات إلى قائمة كائنات الاستجابة DTO
        var result = _mapper.Map<IReadOnlyList<BranchResponseDto>>(branches);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<BranchResponseDto>>.Success(result);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<PagedResult<BranchResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        // تجهيز مواصفة استعلام الفروع مع تضمين الهواتف
        // تنفيذ استعلام الصفحة المجزأة مع إجمالي العدد
        var(items, totalCount) = await _unitOfWork.Branches.SearchAsync(new BranchQuery { PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<BranchResponseDto>>(items);
        // إنشاء كائن النتيجة المجزأة الموحد مع تفاصيل الصفحات
        var pagedResult = PagedResult<BranchResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة الناجحة
        return ServiceResult<PagedResult<BranchResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BranchResponseDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<BranchResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        if (await ValidatePhonesAsync(dto.Phones, null, ct) is { } phoneError)
            return ServiceResult<BranchResponseDto>.Failure(phoneError, ErrorCodes.BranchPhoneExists);

        // التحقق من عدم تكرار اسم الفرع للمستأجر الحالي
        bool nameExists = await _unitOfWork.Branches.NameExistsAsync(dto.Name, ct);
        // في حال وجود فرع مسجل بنفس الاسم
        if (nameExists)
        {
            // إرجاع رسالة خطأ تكرار الاسم
            return ServiceResult<BranchResponseDto>.Failure("اسم الفرع موجود بالفعل", ErrorCodes.BranchNameExists);
        }

        // تحويل بيانات DTO إلى كيان الفرع
        var branch = _mapper.Map<BranchEntity>(dto);
        // التحقق من وجود أرقام هواتف مرفقة مع الفرع الجديد
        if (dto.Phones != null && dto.Phones.Any())
        {
            // تحويل قائمة الهواتف من DTO إلى كيانات هواتف تابعة للفرع
            branch.BranchPhones = dto.Phones.Select(p => new BranchPhone { // تعيين اسم الهاتف أو التسمية الافتراضية "الرئيسي"
 Name = string.IsNullOrWhiteSpace(p.Name) ? "الرئيسي" : p.Name, // تعيين رقم الهاتف
 PhoneNumber = p.PhoneNumber }).ToList();
        }

        // إضافة كيان الفرع وهواتفه إلى المستودع
        await _unitOfWork.Branches.AddAsync(branch, ct);
        // حفظ التغييرات في قاعدة البيانات
        if (await SaveChangesAsync(ct) is { } failure)
            return ServiceResult<BranchResponseDto>.Failure(failure.ErrorMessage!, failure.ErrorCode);
        // إعادة جلب الفرع المحفوظ مع أرقام هواتفه للتأكد من اكتمال البيانات
        var createdBranch = await _unitOfWork.Branches.GetDetailsAsync(branch.Id, ct) ?? branch;
        // تحويل الكيان المحفوظ إلى كائن استجابة DTO
        var responseDto = _mapper.Map<BranchResponseDto>(createdBranch);
        // إرجاع نتيجة نجاح إنشاء الفرع
        return ServiceResult<BranchResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<BranchResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // البحث عن الفرع الحالي مع هواتفه
        var branch = await _unitOfWork.Branches.GetWithPhonesForUpdateAsync(id, ct);
        // التحقق من وجود الفرع
        if (branch is null)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult<BranchResponseDto>.Failure("الفرع غير موجود", ErrorCodes.BranchNotFound);
        }

        // التحقق من عدم استخدام الاسم الجديد لدى فرع آخر
        bool nameExists = await _unitOfWork.Branches.NameExistsAsync(dto.Name, id, ct);
        // في حال وجود تعارض في الاسم
        if (nameExists)
        {
            // إرجاع خطأ تكرار اسم الفرع
            return ServiceResult<BranchResponseDto>.Failure("اسم الفرع موجود بالفعل لدى فرع آخر", ErrorCodes.BranchNameExists);
        }

        if (await ValidatePhonesAsync(dto.Phones, id, ct) is { } phoneError)
            return ServiceResult<BranchResponseDto>.Failure(phoneError, ErrorCodes.BranchPhoneExists);

        // تحديث اسم الفرع
        branch.Name = dto.Name;
        // تحديث عنوان الفرع
        branch.Address = dto.Address;
        // تحديث حالة نشاط الفرع
        branch.IsActive = dto.IsActive;
        // Preserve unchanged numbers and IDs; retire removed numbers in the same save.
        var pendingPhones = dto.Phones.ToDictionary(p => p.PhoneNumber, StringComparer.Ordinal);
        foreach (var phone in branch.BranchPhones.Where(p => !p.IsDeleted).ToList())
        {
            if (pendingPhones.Remove(phone.PhoneNumber, out var incoming))
                phone.Name = string.IsNullOrWhiteSpace(incoming.Name) ? "الرئيسي" : incoming.Name;
            else
                _unitOfWork.Branches.SoftDeletePhone(phone);
        }

        // التحقق من وجود أرقام هواتف جديدة في الطلب
        if (pendingPhones.Count > 0)
        {
            // إنشاء كيانات الهواتف الجديدة وربطها بالفرع
            var newPhones = pendingPhones.Values.Select(p => new BranchPhone { // تعيين معرف الفرع
 BranchId = id, // تعيين اسم جهة الاتصال أو المسمى
 Name = string.IsNullOrWhiteSpace(p.Name) ? "الرئيسي" : p.Name, // تعيين رقم الهاتف
 PhoneNumber = p.PhoneNumber }).ToList();
            // إضافة الهواتف الجديدة كمجموعة إلى المستودع
            await _unitOfWork.Branches.AddPhonesAsync(newPhones, ct);
        }

        // وسم الفرع للتحديث في المستودع
        _unitOfWork.Branches.Update(branch);
        // حفظ كافة التغييرات في قاعدة البيانات
        if (await SaveChangesAsync(ct) is { } failure)
            return ServiceResult<BranchResponseDto>.Failure(failure.ErrorMessage!, failure.ErrorCode);
        // إعادة جلب الفرع المحدث مع هواتفه الجديدة
        var updatedBranch = await _unitOfWork.Branches.GetDetailsAsync(id, ct) ?? branch;
        // تحويل الكيان إلى DTO
        var responseDto = _mapper.Map<BranchResponseDto>(updatedBranch);
        // إرجاع نتيجة نجاح التعديل
        return ServiceResult<BranchResponseDto>.Success(responseDto);
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // جلب الفرع مع هواتفه والمستخدمين المرتبطين به للتحقق
        var branch = await _unitOfWork.Branches.GetWithPhonesForUpdateAsync(id, ct);
        // التحقق من وجود الفرع
        if (branch is null)
        {
            // إرجاع خطأ عدم العثور على الفرع
            return ServiceResult.Failure("الفرع غير موجود", ErrorCodes.BranchNotFound);
        }

        // التحقق من عدم ارتباط مستخدمين نشطين بهذا الفرع لمنع كسر التكامل المرجعي
        if (await _unitOfWork.Branches.HasUsersAsync(id, ct))
        {
            // إرجاع خطأ يوضح وجود مستخدمين مرتبطين
            return ServiceResult.Failure("لا يمكن حذف الفرع لوجود مستخدمين نشطين مرتبطين به", ErrorCodes.BranchHasUsers);
        }

        if (await _unitOfWork.Branches.HasDependenciesAsync(id, ct))
            return ServiceResult.Failure("لا يمكن حذف الفرع لوجود مستودعات أو معاملات مرتبطة به؛ يمكن تعطيله بدلاً من حذفه.", ErrorCodes.BranchHasDependencies);

        foreach (var phone in branch.BranchPhones.Where(p => !p.IsDeleted).ToList())
            _unitOfWork.Branches.SoftDeletePhone(phone);

        // تطبيق الحذف المنطقي للفرع
        _unitOfWork.Branches.SoftDelete(branch);
        // حفظ التغييرات في قاعدة البيانات
        if (await SaveChangesAsync(ct) is { } failure) return failure;
        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc/>
    public async Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        // جلب الفرع بالمعرف
        var branch = await _unitOfWork.Branches.GetByIdForUpdateAsync(id, ct);
        // التحقق من وجود الفرع
        if (branch is null)
        {
            // إرجاع خطأ عدم وجود الفرع
            return ServiceResult.Failure("الفرع غير موجود", ErrorCodes.BranchNotFound);
        }

        // عكس حالة النشاط الحالية (تفعيل / تعطيل)
        branch.IsActive = !branch.IsActive;
        // وسم الفرع للتحديث
        _unitOfWork.Branches.Update(branch);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نجاح العملية
        return ServiceResult.Success();
    }

    private async Task<string?> ValidatePhonesAsync(IReadOnlyCollection<BranchPhoneDto> phones, Guid? branchId, CancellationToken ct)
    {
        var numbers = phones.Select(p => p.PhoneNumber).ToArray();
        return numbers.Distinct(StringComparer.Ordinal).Count() != numbers.Length ||
            await _unitOfWork.Branches.PhonesExistAsync(numbers, branchId, ct)
            ? "رقم هاتف الفرع مكرر أو مستخدم لدى فرع آخر" : null;
    }

    private async Task<ServiceResult?> SaveChangesAsync(CancellationToken ct)
    {
        try { await _unitOfWork.SaveChangesAsync(ct); return null; }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Failure("تغيرت بيانات الفرع؛ يرجى إعادة تحميلها والمحاولة مجدداً", ErrorCodes.ConcurrencyError);
        }
        catch (DbUpdateException ex) when (SqlConstraintErrors.IsUniqueViolation(ex, "IX_Branches_TenantId_Name"))
        {
            return ServiceResult.Failure("اسم الفرع موجود بالفعل", ErrorCodes.BranchNameExists);
        }
        catch (DbUpdateException ex) when (SqlConstraintErrors.IsUniqueViolation(ex, "IX_BranchPhones_TenantId_PhoneNumber"))
        {
            return ServiceResult.Failure("رقم هاتف الفرع مستخدم بالفعل", ErrorCodes.BranchPhoneExists);
        }
        catch (DbUpdateException ex) when (SqlConstraintErrors.IsBranchInUse(ex))
        {
            return ServiceResult.Failure("لا يمكن حذف الفرع لوجود بيانات مرتبطة به", ErrorCodes.BranchHasDependencies);
        }
    }
}
