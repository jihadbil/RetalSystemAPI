using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.DataAccess.Queries;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Accounting.ExpenseCategories;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Implementations;
/// <summary>
/// تنفيذ خدمة إدارة تصنيفات وشجرات المصروفات التشغيلية والتحقق من عدم تكرار الأسماء والتحكم بالنشاط.
/// </summary>
public class ExpenseCategoryService : IExpenseCategoryService
{
    // وحدة العمل للوصول إلى مستودعات بيانات الحسابات
    private readonly IUnitOfWork _unitOfWork;
    // محول الكيانات لتحويل النماذج إلى كائنات DTO
    private readonly IMapper _mapper;
    /// <summary>
    /// تهيئة خدمة تصنيفات المصروفات وحقن وحدة العمل ومحول الكيانات.
    /// </summary>
    /// <param name = "unitOfWork">وحدة العمل</param>
    /// <param name = "mapper">محول الكيانات</param>
    public ExpenseCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc/>
    /// <summary>
    /// جلب بيانات تصنيف مصروفات محدد بواسطة المعرف مع تفاصيل السندات المرتبطة.
    /// </summary>
    public async Task<ServiceResult<ExpenseCategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // جلب تصنيف المصروفات من المستودع
        var category = await _unitOfWork.Accounting.GetExpenseCategoryWithDetailsAsync(id, ct);
        // إذا لم يوجد التصنيف
        if (category == null)
            // إرجاع خطأ عدم وجود التصنيف
            return ServiceResult<ExpenseCategoryResponseDto>.Failure("تصنيف المصروفات غير موجود", ErrorCodes.ExpenseCategoryNotFound);
        // تحويل الكيان إلى DTO وإرجاعه بنجاح
        return ServiceResult<ExpenseCategoryResponseDto>.Success(_mapper.Map<ExpenseCategoryResponseDto>(category));
    }

    /// <inheritdoc/>
    /// <summary>
    /// جلب قائمة بكافة تصنيفات المصروفات المسجلة لدى المستأجر.
    /// </summary>
    public async Task<ServiceResult<IReadOnlyList<ExpenseCategoryResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        // جلب كافة التصنيفات من قاعدة البيانات
        var categories = await _unitOfWork.Accounting.GetAllExpenseCategoriesAsync(ct);
        // تحويل القائمة إلى كائنات DTO وإرجاعها بنجاح
        return ServiceResult<IReadOnlyList<ExpenseCategoryResponseDto>>.Success(_mapper.Map<IReadOnlyList<ExpenseCategoryResponseDto>>(categories));
    }

    /// <inheritdoc/>
    /// <summary>
    /// استرجاع صفحة مجزأة ومفلترة من تصنيفات المصروفات.
    /// </summary>
    public async Task<ServiceResult<PagedResult<ExpenseCategoryResponseDto>>> GetPagedAsync(bool? isActive, string? searchTerm, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        // تصحيح رقم الصفحة في حال كان أقل من 1
        if (pageNumber < 1)
            pageNumber = 1;
        // تصحيح حجم الصفحة في حال كان أقل من 1
        if (pageSize < 1)
            pageSize = 10;
        // تنفيذ استعلام جلب التصنيفات المجزأة
        var(items, totalCount) = await _unitOfWork.Accounting.SearchExpenseCategoriesAsync(new ExpenseCategoryQuery { IsActive = isActive, SearchTerm = searchTerm, PageNumber = pageNumber, PageSize = pageSize }, ct);
        // تحويل الكيانات المقروءة إلى DTO
        var dtos = _mapper.Map<IReadOnlyList<ExpenseCategoryResponseDto>>(items);
        // بناء كائن النتائج المجزأة
        var pagedResult = PagedResult<ExpenseCategoryResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);
        // إرجاع النتيجة بنجاح
        return ServiceResult<PagedResult<ExpenseCategoryResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc/>
    /// <summary>
    /// إنشاء تصنيف مصروفات تشغيلية جديد بعد التحقق من عدم تكرار الاسم.
    /// </summary>
    public async Task<ServiceResult<ExpenseCategoryResponseDto>> CreateAsync(CreateExpenseCategoryDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ExpenseCategoryResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // التحقق من عدم تكرار اسم التصنيف لدى المستأجر
        bool nameExists = await _unitOfWork.Accounting.ExpenseCategoryNameExistsAsync(dto.Name.Trim(), null, ct);
        // إذا كان الاسم موجوداً مسبقاً
        if (nameExists)
            // إرجاع خطأ تكرار الاسم
            return ServiceResult<ExpenseCategoryResponseDto>.Failure("اسم تصنيف المصروفات مسجل مسبقاً", ErrorCodes.ExpenseCategoryNameExists);
        // تحويل كائن DTO إلى كيان تصنيف المصروفات
        var category = _mapper.Map<ExpenseCategory>(dto);
        // إضافة الكيان لمستودع البيانات
        await _unitOfWork.Accounting.AddExpenseCategoryAsync(category, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // جلب التصنيف المنشأ مع تفاصيله
        var created = await _unitOfWork.Accounting.GetExpenseCategoryWithDetailsAsync(category.Id, ct) ?? category;
        // إرجاع كائن الاستجابة بنجاح
        return ServiceResult<ExpenseCategoryResponseDto>.Success(_mapper.Map<ExpenseCategoryResponseDto>(created));
    }

    /// <inheritdoc/>
    /// <summary>
    /// تحديث بيانات تصنيف المصروفات والتحقق من عدم تكرار الاسم في حال تعديله.
    /// </summary>
    public async Task<ServiceResult<ExpenseCategoryResponseDto>> UpdateAsync(Guid id, UpdateExpenseCategoryDto dto, CancellationToken ct = default)
    {
        if (DtoValidation.GetError(dto) is { } validationError)
            return ServiceResult<ExpenseCategoryResponseDto>.Failure(validationError, ErrorCodes.ValidationError);

        // جلب التصنيف من المستودع بالمعرف
        var category = await _unitOfWork.Accounting.GetExpenseCategoryByIdAsync(id, ct);
        // في حال عدم العثور عليه
        if (category == null)
            // إرجاع خطأ عدم وجود التصنيف
            return ServiceResult<ExpenseCategoryResponseDto>.Failure("تصنيف المصروفات غير موجود", ErrorCodes.ExpenseCategoryNotFound);
        // التحقق من عدم تكرار الاسم الجديد في حال تغييره
        if (!string.Equals(category.Name, dto.Name.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            // فحص تكرار الاسم الجديد في المستودع
            bool nameExists = await _unitOfWork.Accounting.ExpenseCategoryNameExistsAsync(dto.Name.Trim(), id, ct);
            // إذا كان مكرراً
            if (nameExists)
                // إرجاع خطأ تكرار الاسم
                return ServiceResult<ExpenseCategoryResponseDto>.Failure("اسم تصنيف المصروفات مسجل مسبقاً", ErrorCodes.ExpenseCategoryNameExists);
        }

        // تطبيق التعديلات من DTO إلى الكيان
        _mapper.Map(dto, category);
        // إشعار المستودع بالتحديث
        _unitOfWork.Accounting.UpdateExpenseCategory(category);
        // حفظ التغييرات
        await _unitOfWork.SaveChangesAsync(ct);
        // جلب التصنيف المحدث مع التفاصيل
        var updated = await _unitOfWork.Accounting.GetExpenseCategoryWithDetailsAsync(id, ct) ?? category;
        // إرجاع النتيجة بنجاح
        return ServiceResult<ExpenseCategoryResponseDto>.Success(_mapper.Map<ExpenseCategoryResponseDto>(updated));
    }

    /// <inheritdoc/>
    /// <summary>
    /// تبديل وعكس حالة نشاط تصنيف المصروفات بين التفعيل والتعطيل.
    /// </summary>
    public async Task<ServiceResult<ExpenseCategoryResponseDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default)
    {
        // جلب التصنيف بالمعرف
        var category = await _unitOfWork.Accounting.GetExpenseCategoryByIdAsync(id, ct);
        // إذا لم يوجد التصنيف
        if (category == null)
            // إرجاع خطأ عدم وجود التصنيف
            return ServiceResult<ExpenseCategoryResponseDto>.Failure("تصنيف المصروفات غير موجود", ErrorCodes.ExpenseCategoryNotFound);
        // عكس حالة النشاط
        category.IsActive = !category.IsActive;
        // تحديث الكيان في المستودع
        _unitOfWork.Accounting.UpdateExpenseCategory(category);
        // حفظ التغييرات
        await _unitOfWork.SaveChangesAsync(ct);
        // جلب التصنيف بعد تعديل حالته
        var updated = await _unitOfWork.Accounting.GetExpenseCategoryWithDetailsAsync(id, ct) ?? category;
        // إرجاع النتيجة بنجاح
        return ServiceResult<ExpenseCategoryResponseDto>.Success(_mapper.Map<ExpenseCategoryResponseDto>(updated));
    }

    /// <inheritdoc/>
    /// <summary>
    /// حذف ناعم لتصنيف المصروفات بشرط عدم ارتباطه بأي سندات مصروفات مسجلة عليه.
    /// </summary>
    public async Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // جلب التصنيف بالمعرف
        var category = await _unitOfWork.Accounting.GetExpenseCategoryByIdAsync(id, ct);
        // إذا لم يوجد
        if (category == null)
            // إرجاع خطأ عدم العثور عليه
            return ServiceResult<bool>.Failure("تصنيف المصروفات غير موجود", ErrorCodes.ExpenseCategoryNotFound);
        // التحقق من عدم وجود سندات مصروفات مرتبطة بالتصنيف
        bool hasExpenses = await _unitOfWork.Accounting.ExpenseCategoryHasExpensesAsync(id, ct);
        // إذا كانت هناك سندات مرتبطة
        if (hasExpenses)
            // إرجاع خطأ منع الحذف لوجود سندات مسجلة
            return ServiceResult<bool>.Failure("لا يمكن حذف تصنيف المصروفات لاحتوائه على سندات مصروفات مسجلة", ErrorCodes.ExpenseCategoryHasExpenses);
        // تطبيق الحذف الناعم على الكيان
        _unitOfWork.Accounting.SoftDeleteExpenseCategory(category);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);
        // إرجاع نجاح عملية الحذف
        return ServiceResult<bool>.Success(true);
    }
}
