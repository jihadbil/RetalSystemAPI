using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة التصنيفات لإدارة شجرة التصنيفات الرئيسية والفرعية للأصناف.
/// </summary>
public interface ICategoryService
{
    /// <summary>جلب تفاصيل تصنيف محدد بواسطة المعرف</summary>
    Task<ServiceResult<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب كافة التصنيفات</summary>
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>جلب التصنيفات الجذرية الرئيسية (التي ليس لها أب)</summary>
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetRootCategoriesAsync(CancellationToken ct = default);

    /// <summary>جلب التصنيفات الفرعية التابعة لتصنيف أب معين</summary>
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من التصنيفات</summary>
    Task<ServiceResult<PagedResult<CategoryResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>إنشاء تصنيف جديد مع التحقق من عدم تكرار الاسم</summary>
    Task<ServiceResult<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات تصنيف مع منع المراجع الدائرية (Circular References)</summary>
    Task<ServiceResult<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default);

    /// <summary>حذف تصنيف منطقياً بعد التأكد من خلوه من الأصناف والتصنيفات الفرعية</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>تبديل حالة نشاط التصنيف</summary>
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
