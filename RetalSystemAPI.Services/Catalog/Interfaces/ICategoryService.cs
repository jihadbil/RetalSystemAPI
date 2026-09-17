using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Category;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة التصنيفات لإدارة شجرة التصنيفات الرئيسية والفرعية للأصناف ومنع الحلقات الدائرية.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// جلب تفاصيل تصنيف محدد بواسطة المعرف مع تفاصيل الأصناف والتصنيفات الفرعية.
    /// </summary>
    /// <param name="id">معرف التصنيف الفريد</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>بيانات التصنيف التفصيلية</returns>
    Task<ServiceResult<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة التصنيفات المعرفة في النظام مرتبة حسب ترتيب العرض.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بكافة التصنيفات</returns>
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب التصنيفات الجذرية الرئيسية (التي ليس لها تصنيف أب).
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة التصنيفات الجذرية</returns>
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetRootCategoriesAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب التصنيفات الفرعية التابعة لتصنيف أب معين.
    /// </summary>
    /// <param name="parentId">معرف التصنيف الأب</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة التصنيفات الفرعية التابعة</returns>
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من التصنيفات مع دعم الترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة الحالية</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>صفحة نتائج تحتوي على عناصر التصنيفات وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<CategoryResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// إنشاء تصنيف جديد مع التحقق من عدم تكرار الاسم والتحقق من وجود التصنيف الأب إن حُدد.
    /// </summary>
    /// <param name="dto">بيانات التصنيف الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات التصنيف المنشأ</returns>
    Task<ServiceResult<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات تصنيف مع منع المراجع الدائرية (Circular References) وتكرار الاسم.
    /// </summary>
    /// <param name="id">معرف التصنيف المراد تعديله</param>
    /// <param name="dto">البيانات الجديدة للتصنيف</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات التصنيف المحدثة</returns>
    Task<ServiceResult<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف تصنيف منطقياً بعد التأكد من خلوه من الأصناف والتصنيفات الفرعية المرتبطة.
    /// </summary>
    /// <param name="id">معرف التصنيف المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تبديل حالة نشاط التصنيف بين التفعيل والتعطيل.
    /// </summary>
    /// <param name="id">معرف التصنيف</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل العملية</returns>
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
