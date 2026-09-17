using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة التسويات الجردية ومعالجة فوارق الجرد (زيادة/عجز/تلف/انتهاء صلاحية) وتعديل أرصدة المخازن الفعلية.
/// </summary>
public interface IStockAdjustmentService
{
    /// <summary>
    /// جلب تفاصيل تسوية جردية محددة بالمعرف مع كافة بنود التعديل وتفاصيل المستودع.
    /// </summary>
    /// <param name="id">المعرف الفريد لسند التسوية</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات سند التسوية الجردية</returns>
    Task<ServiceResult<StockAdjustmentResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب تسوية جردية بواسطة رقم سند التسوية.
    /// </summary>
    /// <param name="adjustmentNumber">رقم سند التسوية الجردية</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات سند التسوية</returns>
    Task<ServiceResult<StockAdjustmentResponseDto>> GetByAdjustmentNumberAsync(string adjustmentNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة التسويات الجردية مع إمكانية الفلترة بالمستودع وسبب التسوية والفترة الزمنية.
    /// </summary>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="reason">سبب التسوية الجردية (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات التسويات الجردية</returns>
    Task<ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>> GetAllAsync(
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من التسويات الجردية مع الترقيم والفلترة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="reason">سبب التسوية الجردية (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على ملخصات التسويات وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<StockAdjustmentSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// إنشاء واعتماد تسوية جردية وتعديل كميات المخزون في المستودع المحدد تلقائياً وفق الفوارق الفعلية.
    /// </summary>
    /// <param name="dto">بيانات سند التسوية</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات سند التسوية المعتمد</returns>
    Task<ServiceResult<StockAdjustmentResponseDto>> CreateAsync(CreateStockAdjustmentDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف سجل تسوية جردية منطقياً.
    /// </summary>
    /// <param name="id">معرف سند التسوية</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
