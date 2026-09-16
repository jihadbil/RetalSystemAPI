using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Branch.Interfaces;

/// <summary>
/// واجهة خدمة الفروع لإدارة فروع المستأجر وعناوينها وأرقام هواتفها وحالات التفعيل.
/// </summary>
public interface IBranchService
{
    /// <summary>
    /// جلب تفاصيل فرع محدد بواسطة المعرف.
    /// </summary>
    Task<ServiceResult<BranchResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة فروع المستأجر الحالي مع أرقام هواتفها.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<BranchResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من الفروع مع الترقيم.
    /// </summary>
    Task<ServiceResult<PagedResult<BranchResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// إنشاء فرع جديد للمستأجر الحالي مع هواتفه.
    /// </summary>
    Task<ServiceResult<BranchResponseDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات فرع موجود وإدارة أرقام هواتفه.
    /// </summary>
    Task<ServiceResult<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف فرع منطقياً بعد التحقق من عدم وجود مستخدمين أو مخازن مرتبطة به.
    /// </summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تبديل حالة نشاط الفرع (تفعيل / تعطيل).
    /// </summary>
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
