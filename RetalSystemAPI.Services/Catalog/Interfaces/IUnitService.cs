using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة وحدات القياس العامة في النظام (مثل: حبة، كرتون، باكت، كجم).
/// </summary>
public interface IUnitService
{
    /// <summary>جلب تفاصيل وحدة قياس محددة بالمعرف</summary>
    Task<ServiceResult<UnitResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة وحدات القياس المعرفة في النظام</summary>
    Task<ServiceResult<IReadOnlyList<UnitResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>إنشاء وحدة قياس جديدة</summary>
    Task<ServiceResult<UnitResponseDto>> CreateAsync(CreateUnitDto dto, CancellationToken ct = default);

    /// <summary>تحديث اسم أو بيانات وحدة قياس</summary>
    Task<ServiceResult<UnitResponseDto>> UpdateAsync(Guid id, UpdateUnitDto dto, CancellationToken ct = default);

    /// <summary>حذف وحدة قياس منطقياً بعد التأكد من عدم استخدامها في أي منتج</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
