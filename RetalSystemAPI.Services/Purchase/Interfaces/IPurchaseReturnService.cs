using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Purchase.Interfaces;

/// <summary>
/// واجهة خدمة إدارة مرتجعات المشتريات وخصم البضائع المرتجعة من المخازن.
/// </summary>
public interface IPurchaseReturnService
{
    /// <summary>
    /// جلب تفاصيل مرتجع المشتريات بالمعرف الفريد مع تفاصيل البنود والمورد والمستودع.
    /// </summary>
    /// <param name="id">معرف سجل المرتجع</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>تفاصيل مرتجع المشتريات</returns>
    Task<ServiceResult<PurchaseReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب تفاصيل مرتجع المشتريات برقم المرتجع.
    /// </summary>
    /// <param name="returnNumber">رقم إشعار المرتجع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>تفاصيل مرتجع المشتريات</returns>
    Task<ServiceResult<PurchaseReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة مرتجعات المشتريات مع إمكانية الفلترة المتقدمة.
    /// </summary>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="supplierId">معرف المورد (اختياري)</param>
    /// <param name="reason">سبب الإرجاع (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بملخصات مرتجعات المشتريات</returns>
    Task<ServiceResult<IReadOnlyList<PurchaseReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة صفحية لمرتجعات المشتريات مع الترقيم والبحث والفلترة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة الحالية</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="warehouseId">معرف المستودع (اختياري)</param>
    /// <param name="supplierId">معرف المورد (اختياري)</param>
    /// <param name="reason">سبب الإرجاع (اختياري)</param>
    /// <param name="paymentMethod">طريقة الدفع (اختياري)</param>
    /// <param name="fromDate">تاريخ البداية (اختياري)</param>
    /// <param name="toDate">تاريخ النهاية (اختياري)</param>
    /// <param name="search">نص البحث (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>صفحة نتائج تحتوي على عناصر المرتجعات وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<PurchaseReturnSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>
    /// تسجيل وإصدار مرتجع مشتريات جديد وخصم البضاعة من المخزن بدقة وفق النكهات والباركودات.
    /// </summary>
    /// <param name="dto">بيانات تسجيل المرتجع وبنوده</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات إشعار المرتجع المنشأ</returns>
    Task<ServiceResult<PurchaseReturnResponseDto>> CreateAsync(CreatePurchaseReturnDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف مرتجع مشتريات وإعادة البضاعة للمخزن.
    /// </summary>
    /// <param name="id">معرف سجل المرتجع المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
