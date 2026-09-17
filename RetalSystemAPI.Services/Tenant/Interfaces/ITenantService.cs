using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Tenant.Interfaces;

/// <summary>
/// واجهة خدمة إدارة المستأجرين (Tenants) على مستوى المنظومة، وتتضمن العمليات الأساسية من إنشاء وتعديل وحذف واستعلام وتغيير الحالة.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// جلب بيانات مستأجر محدد بواسطة معرفه الفريد مع تضمين إحصائيات الفروع والمستودعات والمستخدمين.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة بيانات المستأجر أو رمز خطأ</returns>
    Task<ServiceResult<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة المستأجرين المسجلين في النظام دون تجزئة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة بيانات المستأجرين</returns>
    Task<ServiceResult<IReadOnlyList<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من المستأجرين مع ترقيم الصفحات والفرز بحسب تاريخ الإنشاء.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوبة (يبدأ من 1)</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على المستأجرين وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<TenantResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// إنشاء مستأجر جديد في النظام مع التحقق من عدم تكرار الاسم.
    /// </summary>
    /// <param name="dto">بيانات المستأجر الجديد المراد إنشاؤه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المستأجر المنشأ بعد الحفظ</returns>
    Task<ServiceResult<TenantResponseDto>> CreateAsync(CreateTenantDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات مستأجر موجود والتحقق من عدم تعارض الاسم مع مستأجرين آخرين.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر المراد تعديله</param>
    /// <param name="dto">البيانات الجديدة المراد تحديثها</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المستأجر المحدثة مع الإحصائيات</returns>
    Task<ServiceResult<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف مستأجر منطقياً (Soft Delete) عبر تعيين علامة الحذف وتاريخه.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر المطلوب حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة نجاح أو فشل العملية</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تحديث رابط أو مسار شعار المستأجر.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر</param>
    /// <param name="logoUrl">الرابط الجديد لشعار المستأجر</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المستأجر المحدثة</returns>
    Task<ServiceResult<TenantResponseDto>> UpdateLogoAsync(Guid id, string logoUrl, CancellationToken ct = default);

    /// <summary>
    /// تبديل حالة نشاط حساب المستأجر بين التفعيل والتعطيل.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستأجر</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام تغيير الحالة</returns>
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
