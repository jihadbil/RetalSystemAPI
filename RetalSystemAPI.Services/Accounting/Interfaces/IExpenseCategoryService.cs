using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Accounting.ExpenseCategories;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Interfaces;

/// <summary>
/// واجهة خدمة إدارة تصنيفات وشجرات المصروفات التشغيلية وتنظيم بنود الإنفاق.
/// </summary>
public interface IExpenseCategoryService
{
    /// <summary>
    /// جلب بيانات تصنيف مصروفات محدد بواسطة المعرف الفريد.
    /// </summary>
    /// <param name="id">معرف تصنيف المصروفات</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات تصنيف المصروفات أو خطأ في حال عدم وجوده</returns>
    // جلب تصنيف مصروفات بواسطة معرفه
    Task<ServiceResult<ExpenseCategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة تصنيفات المصروفات المتاحة لدى المستأجر.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>قائمة بجميع تصنيفات المصروفات</returns>
    // جلب كافة تصنيفات المصروفات
    Task<ServiceResult<IReadOnlyList<ExpenseCategoryResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة لتصنيفات المصروفات مع إمكانية الفلترة حسب النشاط والاسم.
    /// </summary>
    /// <param name="isActive">فلترة حسب حالة النشاط (اختياري)</param>
    /// <param name="searchTerm">كلمة البحث في الاسم أو الوصف (اختياري)</param>
    /// <param name="pageNumber">رقم الصفحة المطلوب</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>صفحة نتائج مجزأة لتصنيفات المصروفات</returns>
    // استرجاع تصنيفات المصروفات في صفحة مجزأة ومفلترة
    Task<ServiceResult<PagedResult<ExpenseCategoryResponseDto>>> GetPagedAsync(
        bool? isActive,
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default);

    /// <summary>
    /// إنشاء تصنيف مصروفات تشغيلية جديد والتحقق من عدم تكرار الاسم.
    /// </summary>
    /// <param name="dto">بيانات تصنيف المصروفات الجديد</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات التصنيف بعد إنشائه</returns>
    // إنشاء تصنيف مصروفات جديد
    Task<ServiceResult<ExpenseCategoryResponseDto>> CreateAsync(CreateExpenseCategoryDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات تصنيف مصروفات موجود مسبقاً.
    /// </summary>
    /// <param name="id">معرف تصنيف المصروفات المراد تعديله</param>
    /// <param name="dto">البيانات الجديدة المراد تحديثها</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات التصنيف بعد التحديث</returns>
    // تحديث بيانات تصنيف مصروفات
    Task<ServiceResult<ExpenseCategoryResponseDto>> UpdateAsync(Guid id, UpdateExpenseCategoryDto dto, CancellationToken ct = default);

    /// <summary>
    /// عكس وتبديل حالة نشاط تصنيف المصروفات بين التفعيل والتعطيل.
    /// </summary>
    /// <param name="id">معرف تصنيف المصروفات</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات التصنيف مع الحالة الجديدة</returns>
    // عكس حالة نشاط التصنيف
    Task<ServiceResult<ExpenseCategoryResponseDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// حذف ناعم لتصنيف المصروفات بشرط عدم احتوائه على أي سندات مصروفات مسجلة عليه.
    /// </summary>
    /// <param name="id">معرف تصنيف المصروفات المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>قيمة منطقية تدل على نجاح الحذف</returns>
    // حذف ناعم لتصنيف المصروفات بشرط عدم وجود سندات مرتبطة
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
}
