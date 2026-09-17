using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Services.Catalog.Interfaces;
using RetalSystemAPI.Services.Catalog.Specifications;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة التصنيفات والهيكل الشجري الهرمي للأصناف والمنتجات وفحص العلاقات الدائرية.
/// </summary>
public class CategoryService : ICategoryService
{
    // وحدة العمل للوصول إلى مستودعات البيانات
    private readonly IUnitOfWork _unitOfWork;
    // محول البيانات لتحويل الكيانات إلى كائنات نقل البيانات والعكس
    private readonly IMapper _mapper;

    /// <summary>
    /// تهيئة خدمة التصنيفات مع حقن وحدة العمل وAutoMapper.
    /// </summary>
    /// <param name="unitOfWork">وحدة العمل لإدارة التفاعل مع قاعدة البيانات</param>
    /// <param name="mapper">خدمة تحويل النماذج</param>
    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        // تعيين مرجع وحدة العمل
        _unitOfWork = unitOfWork;
        // تعيين مرجع محول الكيانات
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام التصنيف بالمعرف مع تفاصيل الأصناف والتصنيفات الفرعية والتصنيف الأب
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(new CategoryWithDetailsSpec(id), ct);
        // التحقق من وجود التصنيف
        if (category is null)
        {
            // إرجاع خطأ بعدم العثور على التصنيف
            return ServiceResult<CategoryResponseDto>.Failure("التصنيف غير موجود", ErrorCodes.CategoryNotFound);
        }

        // تحويل الكيان إلى كائن الاستجابة
        var result = _mapper.Map<CategoryResponseDto>(category);
        // إرجاع النتيجة الناجحة
        return ServiceResult<CategoryResponseDto>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetAllAsync(CancellationToken ct = default)
    {
        // استعلام كافة التصنيفات مع تفاصيلها وترتيبها
        var categories = await _unitOfWork.Categories.FindAsync(new CategoryWithDetailsSpec(), ct);
        // تحويل قائمة الكيانات إلى DTOs
        var result = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<CategoryResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetRootCategoriesAsync(CancellationToken ct = default)
    {
        // استعلام التصنيفات الجذرية التي ليس لها أب
        var categories = await _unitOfWork.Categories.FindAsync(new RootCategoriesSpec(), ct);
        // تحويل الكيانات إلى كائنات الاستجابة
        var result = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<CategoryResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default)
    {
        // استعلام التصنيفات الفرعية التابعة لمعرف التصنيف الأب المحدد
        var categories = await _unitOfWork.Categories.FindAsync(new SubCategoriesSpec(parentId), ct);
        // تحويل النتائج إلى كائنات نقل البيانات
        var result = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(categories);
        // إرجاع النتيجة الناجحة
        return ServiceResult<IReadOnlyList<CategoryResponseDto>>.Success(result);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PagedResult<CategoryResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        // بناء مواصفة استعلام تفاصيل التصنيفات
        var spec = new CategoryWithDetailsSpec();
        // تنفيذ الاستعلام المصفح لجلب عناصر الصفحة وإجمالي العدد
        var (items, totalCount) = await _unitOfWork.Categories.GetPagedAsync(spec, pageNumber, pageSize, ct);

        // تحويل عناصر الصفحة إلى DTOs
        var dtos = _mapper.Map<IReadOnlyList<CategoryResponseDto>>(items);
        // إنشاء نتيجة التصفح مع حساب الصفحات
        var pagedResult = PagedResult<CategoryResponseDto>.Create(dtos, totalCount, pageNumber, pageSize);

        // إرجاع النتيجة الناجحة المصفحة
        return ServiceResult<PagedResult<CategoryResponseDto>>.Success(pagedResult);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default)
    {
        // التحقق من عدم تكرار اسم التصنيف
        bool nameExists = await _unitOfWork.Categories.ExistsAsync(c => c.Name == dto.Name, ct);
        // في حال وجود الاسم مسبقاً
        if (nameExists)
        {
            // إرجاع خطأ تكرار اسم التصنيف
            return ServiceResult<CategoryResponseDto>.Failure("اسم التصنيف موجود بالفعل", ErrorCodes.CategoryNameExists);
        }

        // التحقق من صحة وجود التصنيف الأب إن تم تحديده
        if (dto.ParentCategoryId.HasValue)
        {
            // الاستعلام عن وجود التصنيف الأب
            bool parentExists = await _unitOfWork.Categories.ExistsAsync(c => c.Id == dto.ParentCategoryId.Value, ct);
            // في حال عدم وجوده
            if (!parentExists)
            {
                // إرجاع خطأ بعدم وجود التصنيف الأب
                return ServiceResult<CategoryResponseDto>.Failure("التصنيف الأب المحدد غير موجود", ErrorCodes.CategoryNotFound);
            }
        }

        // تحويل بيانات الإدخال إلى كيان التصنيف
        var category = _mapper.Map<Category>(dto);
        // إضافة التصنيف الجديد إلى المستودع
        await _unitOfWork.Categories.AddAsync(category, ct);
        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب التصنيف المنشأ بالتفاصيل الكاملة
        var createdCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(new CategoryWithDetailsSpec(category.Id), ct) ?? category;
        // تحويل الكيان إلى كائن الاستجابة
        var responseDto = _mapper.Map<CategoryResponseDto>(createdCategory);

        // إرجاع النتيجة الناجحة
        return ServiceResult<CategoryResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default)
    {
        // استعلام التصنيف المراد تعديله
        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);
        // التحقق من وجود التصنيف
        if (category is null)
        {
            // إرجاع خطأ بعدم وجود التصنيف
            return ServiceResult<CategoryResponseDto>.Failure("التصنيف غير موجود", ErrorCodes.CategoryNotFound);
        }

        // التحقق من عدم تكرار الاسم لدى تصنيف آخر
        bool nameExists = await _unitOfWork.Categories.ExistsAsync(c => c.Name == dto.Name && c.Id != id, ct);
        // في حال تكرار الاسم
        if (nameExists)
        {
            // إرجاع خطأ تكرار اسم التصنيف
            return ServiceResult<CategoryResponseDto>.Failure("اسم التصنيف موجود بالفعل لدى تصنيف آخر", ErrorCodes.CategoryNameExists);
        }

        // التحقق من العلاقات الدائرية عند ربط تصنيف أب
        if (dto.ParentCategoryId.HasValue)
        {
            // منع اختيار التصنيف نفسه كأب لنفسه
            if (dto.ParentCategoryId.Value == id)
            {
                // إرجاع خطأ ربط النفس كأب
                return ServiceResult<CategoryResponseDto>.Failure("لا يمكن اختيار التصنيف نفسه كتصنيف أب", ErrorCodes.CategoryCircularRef);
            }

            // فحص السلسلة الهرمية لمنع تشكل حلقة دائرية
            bool isCircular = await IsCircularReferenceAsync(id, dto.ParentCategoryId.Value, ct);
            // في حال وجود علاقة دائرية
            if (isCircular)
            {
                // إرجاع خطأ الحلقة الدائرية
                return ServiceResult<CategoryResponseDto>.Failure("لا يمكن الربط كتصنيف أب لأنه سيتسبب بحلقة دائرية", ErrorCodes.CategoryCircularRef);
            }
        }

        // تحديث اسم التصنيف
        category.Name = dto.Name;
        // تحديث معرف التصنيف الأب
        category.ParentCategoryId = dto.ParentCategoryId;
        // تحديث حالة النشاط
        category.IsActive = dto.IsActive;
        // تحديث ترتيب العرض
        category.SortOrder = dto.SortOrder;

        // حفظ التغييرات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إعادة جلب التصنيف المحدث مع علاقاته
        var updatedCategory = await _unitOfWork.Categories.FirstOrDefaultAsync(new CategoryWithDetailsSpec(id), ct) ?? category;
        // تحويل الكيان إلى DTO الاستجابة
        var responseDto = _mapper.Map<CategoryResponseDto>(updatedCategory);

        // إرجاع النتيجة الناجحة
        return ServiceResult<CategoryResponseDto>.Success(responseDto);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام التصنيف مع تفاصيله من أصناف وتصنيفات فرعية
        var category = await _unitOfWork.Categories.FirstOrDefaultAsync(new CategoryWithDetailsSpec(id), ct);
        // التحقق من وجود التصنيف
        if (category is null)
        {
            // إرجاع خطأ بعدم وجود التصنيف
            return ServiceResult.Failure("التصنيف غير موجود", ErrorCodes.CategoryNotFound);
        }

        // منع الحذف في حال وجود أصناف أو تصنيفات فرعية تابعة له
        if (category.Products.Any() || category.SubCategories.Any())
        {
            // إرجاع رسالة خطأ تمنع الحذف
            return ServiceResult.Failure("لا يمكن حذف التصنيف لاحتوائه على منتجات أو تصنيفات فرعية مرتبطة", ErrorCodes.CategoryHasProducts);
        }

        // إجراء الحذف المنطقي للتصنيف
        _unitOfWork.Categories.SoftDelete(category);
        // حفظ التعديلات في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    public async Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default)
    {
        // استعلام التصنيف بالمعرف
        var category = await _unitOfWork.Categories.GetByIdAsync(id, ct);
        // التحقق من وجود التصنيف
        if (category is null)
        {
            // إرجاع خطأ بعدم وجود التصنيف
            return ServiceResult.Failure("التصنيف غير موجود", ErrorCodes.CategoryNotFound);
        }

        // عكس حالة النشاط الحالية (مفعل <-> معطل)
        category.IsActive = !category.IsActive;
        // تعليم الكيان كمحدث
        _unitOfWork.Categories.Update(category);
        // حفظ التغيير في قاعدة البيانات
        await _unitOfWork.SaveChangesAsync(ct);

        // إرجاع نتيجة النجاح
        return ServiceResult.Success();
    }

    /// <summary>
    /// فحص ومنع تشكيل مراجع دائرية في الشجرة الهرمية للتصنيفات بالتتبع الصاعد للآباء.
    /// </summary>
    /// <param name="categoryId">معرف التصنيف الحالي المراد تعديله</param>
    /// <param name="targetParentId">معرف التصنيف الأب المرشح للارتباط به</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>صحيح إذا كانت العلاقة تشكل حلقة دائرية، وإلا خطأ</returns>
    private async Task<bool> IsCircularReferenceAsync(Guid categoryId, Guid targetParentId, CancellationToken ct)
    {
        // تتبع معرف الأب الصاعد
        Guid? currentParentId = targetParentId;
        // الصعود في شجرة الآباء حتى الوصول للجذر
        while (currentParentId.HasValue)
        {
            // إذا تطابق أحد الآباء الصاعدين مع التصنيف نفسه فهذا يعني حلقة دائرية
            if (currentParentId.Value == categoryId)
            {
                // إرجاع صحيح دلالة على وجود حلقة دائرية
                return true;
            }

            // جلب بيانات التصنيف الأب في المستوى الحالي
            var parent = await _unitOfWork.Categories.GetByIdAsync(currentParentId.Value, ct);
            // في حال عدم وجود الأب يتم إنهاء الحلقة
            if (parent is null)
            {
                // الخروج من حلقة التتبع
                break;
            }

            // الانتقال للأب التالي في المستوى الأعلى
            currentParentId = parent.ParentCategoryId;
        }

        // إرجاع خطأ لعدم وجود أي حلقة دائرية
        return false;
    }
}
