using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Accounting.Expenses;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Interfaces;

/// <summary>
/// واجهة خدمة إدارة سندات المصروفات وسجلات الإنفاق التشغيلي وتأثيرها المالي على الخزائن وأدراج النقدية.
/// </summary>
public interface IExpenseService
{
    /// <summary>
    /// جلب بيانات سند مصروف محدد بواسطة المعرف الفريد مع كافة تفاصيل التصنيف والخزينة.
    /// </summary>
    /// <param name="id">معرف سند المصروف</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات سند المصروف المفصلة</returns>
    // جلب سند المصروف مع بيانات التصنيف والخزينة
    Task<ServiceResult<ExpenseResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة سندات مصروفات مجزأة مع الفلترة المتقدمة حسب الفرع، التصنيف، الخزينة، التاريخ وطريقة الدفع.
    /// </summary>
    /// <param name="filter">معايير فلترة المصروفات</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>قائمة مجزأة بملخصات المصروفات مع العدد الإجمالي</returns>
    // استرجاع سندات المصروفات وفق معايير الفلترة
    Task<ServiceResult<PagedResult<ExpenseSummaryDto>>> GetPagedAsync(ExpenseFilterDto filter, CancellationToken ct = default);

    /// <summary>
    /// تسجيل سند مصروف تشغيلي جديد وخصم قيمته من رصيد الخزينة أو درج النقدية المعتمد آلياً.
    /// </summary>
    /// <param name="dto">بيانات سند المصروف (المبلغ، التصنيف، الخزينة، الوصف، الفرع)</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات سند المصروف المنشأ</returns>
    // إنشاء سند مصروف جديد وخصم المبلغ من الخزينة المحددة
    Task<ServiceResult<ExpenseResponseDto>> CreateAsync(CreateExpenseDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث البيانات الوصفية لسند المصروف (الوصف، الملاحظات) دون المساس بالقيم المالية المؤثرة على الخزينة.
    /// </summary>
    /// <param name="id">معرف سند المصروف</param>
    /// <param name="dto">البيانات الوصفية الجديدة المراد تحديثها</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>بيانات سند المصروف بعد التحديث</returns>
    // تحديث البيانات الوصفية لسند المصروف
    Task<ServiceResult<ExpenseResponseDto>> UpdateAsync(Guid id, UpdateExpenseDto dto, CancellationToken ct = default);

    /// <summary>
    /// إلغاء وحذف ناعم لسند المصروف مع استرداد وإعادة المبلغ للخزينة إن كان قد خُصم منها.
    /// </summary>
    /// <param name="id">معرف سند المصروف المراد إلغاؤه</param>
    /// <param name="ct">رمز إلغاء العملية الإشعارية</param>
    /// <returns>قيمة منطقية تشير لنجاح عملية الإلغاء واسترداد الرصيد</returns>
    // حذف سند المصروف ناعماً وعكس حركته المالية في الخزينة
    Task<ServiceResult<bool>> DeleteAsync(Guid id, CancellationToken ct = default);
}
