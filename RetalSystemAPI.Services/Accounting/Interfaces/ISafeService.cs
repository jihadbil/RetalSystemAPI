using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Accounting.Safes;
using RetalSystemAPI.Models.DTOs.Accounting.SafeTransactions;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Interfaces;

/// <summary>
/// واجهة خدمة إدارة الخزائن المالية والحسابات المصرفية وأدراج النقدية وحركاتها والتحويلات المالية بينها.
/// </summary>
public interface ISafeService
{
    /// <summary>
    /// جلب بيانات خزينة مالية أو حساب مصرفي بواسطة المعرف الفريد مع تفاصيل الفرع.
    /// </summary>
    /// <param name="id">معرف الخزينة المراد جلبها</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات الخزينة المفصلة أو خطأ في حال عدم وجودها</returns>
    // جلب بيانات الخزينة بالتفصيل حسب المعرف
    Task<ServiceResult<SafeResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة الخزائن والحسابات المصرفية المسجلة لدى المستأجر.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>قائمة ملخصات الخزائن النشطة والمسجلة</returns>
    // جلب كافة الخزائن والحسابات المصرفية المتاحة
    Task<ServiceResult<IReadOnlyList<SafeSummaryDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة للخزائن مع إمكانية الفلترة حسب الفرع وحالة النشاط وكلمة البحث.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="isActive">حالة النشاط (اختياري)</param>
    /// <param name="searchTerm">كلمة البحث في الاسم والكود (اختياري)</param>
    /// <param name="pageNumber">رقم الصفحة المطلوب</param>
    /// <param name="pageSize">عدد العناصر في الصفحة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>صفحة النتائج المجزأة وإجمالي عدد العناصر</returns>
    // جلب صفحة بيانات مجزأة للخزائن وفق معايير الفلترة المحددة
    Task<ServiceResult<PagedResult<SafeSummaryDto>>> GetPagedAsync(
        Guid? branchId,
        bool? isActive,
        string? searchTerm,
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken ct = default);

    /// <summary>
    /// إنشاء خزينة مالية أو حساب مصرفي أو درج نقدية جديد مع تعيين الرصيد الافتتاحي.
    /// </summary>
    /// <param name="dto">بيانات إنشاء الخزينة (الاسم، الفرع، النوع، الرصيد الافتتاحي)</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات الخزينة المنشأة بنجاح</returns>
    // إنشاء خزينة جديدة وتسجيل حركتها الافتتاحية
    Task<ServiceResult<SafeResponseDto>> CreateAsync(CreateSafeDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات خزينة مالية محددة والتحقق من عدم وجود قيود تمنع التعديل.
    /// </summary>
    /// <param name="id">معرف الخزينة المراد تعديلها</param>
    /// <param name="dto">بيانات التحديث الجديدة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات الخزينة المحدثة</returns>
    // تحديث بيانات الخزينة وتدقيق ارتباطاتها
    Task<ServiceResult<SafeResponseDto>> UpdateAsync(Guid id, UpdateSafeDto dto, CancellationToken ct = default);

    /// <summary>
    /// عكس وتغيير حالة نشاط الخزينة (تفعيل أو تعطيل) مع التأكد من عدم ارتباطها بورديات مفتوحة.
    /// </summary>
    /// <param name="id">معرف الخزينة</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات الخزينة بعد تعديل حالة النشاط</returns>
    // تبديل حالة نشاط الخزينة بين التفعيل والتعطيل
    Task<ServiceResult<SafeResponseDto>> ToggleActiveAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// حذف ناعم لخزينة بشرط عدم وجود حركات مالية مسجلة عليها وعدم وجود رصيد معلق.
    /// </summary>
    /// <param name="id">معرف الخزينة المراد حذفها</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>قيمة منطقية تشير إلى نجاح عملية الحذف</returns>
    // حذف الخزينة ناعماً بشرط خلوها من الحركات المالية
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تسجيل حركة مالية يدوية في الخزينة (إيداع أو سحب) مع تحديث الرصيد ذرّياً وتسجيل قيد المعاملة.
    /// </summary>
    /// <param name="dto">بيانات الحركة المالية اليدوية (النوع، المبلغ، الوصف، الخزينة)</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات حركة الخزينة المسجلة</returns>
    // تسجيل حركة إيداع أو سحب نقدي يدوي وتحديث الرصيد مباشرة
    Task<ServiceResult<SafeTransactionResponseDto>> AddTransactionAsync(CreateSafeTransactionDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحويل أموال بين خزينتين أو حسابين مصرفيين ضمن معاملة ذرية متزامنة تضمن توازن الطرفين.
    /// </summary>
    /// <param name="dto">بيانات التحويل المالي (الخزينة المصدر، الخزينة الوجهة، المبلغ، البيان)</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>قيمة منطقية تشير لنجاح عملية التحويل المتزامنة</returns>
    // تنفيذ تحويل مالي متزامن بين خزينتين داخل نفس المعاملة
    Task<ServiceResult<bool>> TransferFundsAsync(TransferFundsDto dto, CancellationToken ct = default);

    /// <summary>
    /// جلب سجل حركات الخزائن مفلترة حسب الفرع أو الخزينة أو التاريخ أو النوع مع التقسيم لصفحات.
    /// </summary>
    /// <param name="filter">معايير فلترة حركات الخزائن</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>صفحة حركات الخزائن وإجمالي عدد السجلات</returns>
    // استرجاع سجل الحركات النقدية وفق معايير الفلترة
    Task<ServiceResult<PagedResult<SafeTransactionResponseDto>>> GetTransactionsAsync(SafeTransactionFilterDto filter, CancellationToken ct = default);
}
