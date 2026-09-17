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
    /// <summary>
    /// جلب تفاصيل وحدة قياس محددة بالمعرف الفريد.
    /// </summary>
    /// <param name="id">معرف وحدة القياس</param>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>بيانات وحدة القياس المطلوبة</returns>
    Task<ServiceResult<UnitResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة وحدات القياس المعرفة في النظام.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة وحدات القياس</returns>
    Task<ServiceResult<IReadOnlyList<UnitResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// إنشاء وحدة قياس جديدة مع التحقق من عدم تكرار الاسم.
    /// </summary>
    /// <param name="dto">بيانات وحدة القياس المراد إنشاؤها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات وحدة القياس المنشأة</returns>
    Task<ServiceResult<UnitResponseDto>> CreateAsync(CreateUnitDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث اسم أو بيانات وحدة قياس موجودة.
    /// </summary>
    /// <param name="id">معرف وحدة القياس المراد تعديلها</param>
    /// <param name="dto">البيانات الجديدة للوحدة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات وحدة القياس بعد التحديث</returns>
    Task<ServiceResult<UnitResponseDto>> UpdateAsync(Guid id, UpdateUnitDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف وحدة قياس منطقياً بعد التأكد من عدم استخدامها في أي منتج.
    /// </summary>
    /// <param name="id">معرف وحدة القياس المراد حذفها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل عملية الحذف</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
