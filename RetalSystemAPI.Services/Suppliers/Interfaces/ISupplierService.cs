using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Suppliers.Interfaces;

/// <summary>
/// واجهة خدمة إدارة الموردين، تفاصيل العناوين، وسجلات أرقام الهواتف والتواصل.
/// </summary>
public interface ISupplierService
{
    /// <summary>
    /// جلب تفاصيل مورد محدد بواسطة المعرف مع هواتفه.
    /// </summary>
    /// <param name="id">المعرف الفريد للمورد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المورد المسجل</returns>
    Task<ServiceResult<SupplierResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة الموردين التابعين للمستأجر الحالي.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات الموردين</returns>
    Task<ServiceResult<IReadOnlyList<SupplierSummaryDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من الموردين مع إمكانية البحث بالاسم أو العنوان أو الهاتف والترقيم.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوبة (يبدأ من 1)</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="search">نص البحث الاختياري</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على ملخصات الموردين وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, string? search = null, CancellationToken ct = default);

    /// <summary>
    /// إنشاء مورد جديد مع التحقق من عدم تكرار الاسم وحفظ هواتفه.
    /// </summary>
    /// <param name="dto">بيانات المورد الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المورد المنشأ</returns>
    Task<ServiceResult<SupplierResponseDto>> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات مورد موجود وإعادة بناء قائمة هواتفه.
    /// </summary>
    /// <param name="id">المعرف الفريد للمورد المراد تعديله</param>
    /// <param name="dto">بيانات التحديث</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المورد المحدثة</returns>
    Task<ServiceResult<SupplierResponseDto>> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف مورد منطقياً (Soft Delete).
    /// </summary>
    /// <param name="id">المعرف الفريد للمورد المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// إضافة رقم هاتف جديد للمورد.
    /// </summary>
    /// <param name="supplierId">معرف المورد</param>
    /// <param name="dto">بيانات الهاتف الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المورد بعد الإضافة</returns>
    Task<ServiceResult<SupplierResponseDto>> AddPhoneAsync(Guid supplierId, SupplierPhoneDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف رقم هاتف محدد للمورد.
    /// </summary>
    /// <param name="supplierId">معرف المورد</param>
    /// <param name="phoneId">معرف رقم الهاتف المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeletePhoneAsync(Guid supplierId, Guid phoneId, CancellationToken ct = default);
}
