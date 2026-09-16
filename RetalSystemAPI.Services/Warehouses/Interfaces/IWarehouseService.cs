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
    /// <summary>جلب تفاصيل مستودع أو صالة محددة بالمعرف</summary>
    Task<ServiceResult<WarehouseResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة المستودعات والصالات مع إمكانية الفلترة بالفرع والنوع (صالة عرض / مخزن تخزين)</summary>
    Task<ServiceResult<IReadOnlyList<WarehouseSummaryDto>>> GetAllAsync(Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من المستودعات مع الترقيم</summary>
    Task<ServiceResult<PagedResult<WarehouseSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default);

    /// <summary>إنشاء مستودع أو صالة جديدة للمستأجر</summary>
    Task<ServiceResult<WarehouseResponseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات المستودع أو الصالة</summary>
    Task<ServiceResult<WarehouseResponseDto>> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken ct = default);

    /// <summary>حذف مستودع منطقياً بعد التحقق من خلوه من الأرصدة</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>تبديل حالة نشاط المستودع</summary>
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
