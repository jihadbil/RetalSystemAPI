using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Branch.Interfaces;

/// <summary>
/// واجهة خدمة الفروع لإدارة فروع المنشأة والمستأجر وعناوينها وأرقام هواتفها وحالات التفعيل.
/// </summary>
public interface IBranchService
{
    /// <summary>
    /// جلب تفاصيل فرع محدد بواسطة المعرف الفريد متضمناً قائمة أرقام هواتفه.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة كائن استجابة الفرع أو رمز خطأ</returns>
    Task<ServiceResult<BranchResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة فروع المستأجر الحالي مع أرقام هواتفها دون تجزئة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بفروع المنشأة</returns>
    Task<ServiceResult<IReadOnlyList<BranchResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من الفروع مع الترقيم والفرز بالاسم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوبة (يبدأ من 1)</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على عناصر الصفحة وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<BranchResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// إنشاء فرع جديد للمستأجر الحالي مع أرقام هواتفه والتحقق من عدم تكرار الاسم.
    /// </summary>
    /// <param name="dto">بيانات إنشاء الفرع الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفرع المنشأ بعد الحفظ</returns>
    Task<ServiceResult<BranchResponseDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات فرع موجود وإعادة بناء قائمة أرقام هواتفه بعد التحقق من عدم تعارض الاسم.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع المراد تعديله</param>
    /// <param name="dto">البيانات الجديدة للفرع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات الفرع المحدثة</returns>
    Task<ServiceResult<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف فرع منطقياً بعد التحقق من عدم وجود مستخدمين أو سجلات نشطة مرتبطة به.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تبديل حالة نشاط الفرع بين التفعيل والتعطيل.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
