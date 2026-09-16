using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Tenant.Interfaces;

/// <summary>
/// واجهة خدمة إدارة المستأجرين (Tenants) على مستوى النظام، تفعيلهم، وتعديل بيانات اشتراكاتهم.
/// </summary>
public interface ITenantService
{
    /// <summary>جلب بيانات مستأجر محدد بالمعرف</summary>
    Task<ServiceResult<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة المستأجرين المسجلين في النظام</summary>
    Task<ServiceResult<IReadOnlyList<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من المستأجرين مع الترقيم</summary>
    Task<ServiceResult<PagedResult<TenantResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>إنشاء مستأجر جديد في النظام</summary>
    Task<ServiceResult<TenantResponseDto>> CreateAsync(CreateTenantDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات مستأجر موجود</summary>
    Task<ServiceResult<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto dto, CancellationToken ct = default);

    /// <summary>حذف مستأجر منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>تحديث رابط شعار المستأجر</summary>
    Task<ServiceResult<TenantResponseDto>> UpdateLogoAsync(Guid id, string logoUrl, CancellationToken ct = default);

    /// <summary>تبديل حالة نشاط حساب المستأجر (تفعيل/تعطيل)</summary>
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
