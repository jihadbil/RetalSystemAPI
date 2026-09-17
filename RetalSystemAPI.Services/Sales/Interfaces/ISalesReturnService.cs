using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Sales.Interfaces;

/// <summary>
/// واجهة خدمة إدارة مرتجعات المبيعات واسترجاع البضائع لمخزون الصالة وتحديث الحسابات.
/// </summary>
public interface ISalesReturnService
{
    /// <summary>
    /// جلب تفاصيل مرتجع مبيعات محدد بالمعرف مع بنوده.
    /// </summary>
    /// <param name="id">المعرف الفريد للمرتجع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات المرتجع</returns>
    Task<ServiceResult<SalesReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب مرتجع مبيعات بواسطة رقم المرتجع.
    /// </summary>
    /// <param name="returnNumber">رقم المرتجع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المرتجع</returns>
    Task<ServiceResult<SalesReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة مرتجعات المبيعات مع فلاتر الفرع، الصالة، العميل، وسبب الإرجاع.
    /// </summary>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع أو الصالة (اختياري)</param>
    /// <param name="customerId">معرف العميل (اختياري)</param>
    /// <param name="reason">سبب الإرجاع (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات المرتجعات</returns>
    Task<ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من مرتجعات المبيعات مع الترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع أو الصالة (اختياري)</param>
    /// <param name="customerId">معرف العميل (اختياري)</param>
    /// <param name="reason">سبب الإرجاع (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على ملخصات المرتجعات وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<SalesReturnSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// إنشاء مرتجع مبيعات جديد وإعادة كميات البنود لمخزون صالة العرض آلياً.
    /// </summary>
    /// <param name="dto">بيانات إنشاء المرتجع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المرتجع المنشأ</returns>
    Task<ServiceResult<SalesReturnResponseDto>> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف سجل مرتجع مبيعات منطقياً وإلغاء أثره على المخزون.
    /// </summary>
    /// <param name="id">معرف المرتجع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
