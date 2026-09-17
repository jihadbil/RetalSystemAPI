using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة المستودعات، صالات العرض، وتصنيفاتها وحالات تشغيلها.
/// </summary>
public interface IWarehouseService
{
    /// <summary>
    /// جلب تفاصيل مستودع أو صالة محددة بالمعرف الفريد.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات المستودع</returns>
    Task<ServiceResult<WarehouseResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة المستودعات والصالات مع إمكانية الفلترة بالفرع والنوع (صالة عرض / مخزن تخزين).
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="type">نوع المستودع (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات المستودعات</returns>
    Task<ServiceResult<IReadOnlyList<WarehouseSummaryDto>>> GetAllAsync(Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من المستودعات مع الترقيم والفلترة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة (يبدأ من 1)</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="branchId">معرف الفرع (اختياري)</param>
    /// <param name="type">نوع المستودع (اختياري)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على ملخصات المستودعات وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<WarehouseSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default);

    /// <summary>
    /// إنشاء مستودع أو صالة جديدة للمستأجر مع توليد أسطر المخزون الافتتاحية تلقائياً.
    /// </summary>
    /// <param name="dto">بيانات إنشاء المستودع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المستودع المنشأ</returns>
    Task<ServiceResult<WarehouseResponseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات المستودع أو الصالة.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع المراد تعديله</param>
    /// <param name="dto">بيانات التحديث</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المستودع المحدثة</returns>
    Task<ServiceResult<WarehouseResponseDto>> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف مستودع منطقياً بعد التحقق من خلوه تماماً من الأرصدة المخزنية.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تبديل حالة نشاط المستودع بين التفعيل والتعطيل.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
