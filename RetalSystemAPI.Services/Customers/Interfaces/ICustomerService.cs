using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Customers;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Customers.Interfaces;

/// <summary>
/// واجهة خدمة إدارة العملاء وحساباتهم وحدود الائتمان وسجل الهواتف وحالة النشاط.
/// </summary>
public interface ICustomerService
{
    /// <summary>
    /// جلب تفاصيل عميل محدد بالمعرف مع هواتفه.
    /// </summary>
    /// <param name="id">المعرف الفريد للعميل</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة الخدمة متضمنة كائن استجابة العميل أو رمز خطأ</returns>
    Task<ServiceResult<CustomerResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بالعملاء مع إمكانية الفلترة حسب النوع وحالة النشاط والبحث بالاسم أو الكود أو الهاتف.
    /// </summary>
    /// <param name="type">نوع العميل الاختياري</param>
    /// <param name="isActive">حالة النشاط الاختيارية</param>
    /// <param name="search">نص البحث الاختياري</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>قائمة ملخصات العملاء المطابقين</returns>
    Task<ServiceResult<IReadOnlyList<CustomerSummaryDto>>> GetAllAsync(CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);

    /// <summary>
    /// جلب صفحة بيانات مجزأة من العملاء مع الترقيم والفلترة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوبة (يبدأ من 1)</param>
    /// <param name="pageSize">عدد العناصر في كل صفحة</param>
    /// <param name="type">نوع العميل الاختياري</param>
    /// <param name="isActive">حالة النشاط الاختيارية</param>
    /// <param name="search">نص البحث الاختياري</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة مجزأة تحتوي على ملخصات العملاء وإجمالي العدد</returns>
    Task<ServiceResult<PagedResult<CustomerSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);

    /// <summary>
    /// إنشاء عميل جديد مع التحقق من عدم تكرار الكود وحفظ أرقام هواتفه.
    /// </summary>
    /// <param name="dto">بيانات العميل الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات العميل المنشأ</returns>
    Task<ServiceResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default);

    /// <summary>
    /// تحديث بيانات عميل موجود وإعادة تعيين أرقام هواتفه.
    /// </summary>
    /// <param name="id">المعرف الفريد للعميل المراد تعديله</param>
    /// <param name="dto">بيانات التحديث</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات العميل المحدثة</returns>
    Task<ServiceResult<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف عميل منطقياً (Soft Delete).
    /// </summary>
    /// <param name="id">المعرف الفريد للعميل المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// تبديل حالة نشاط العميل بين التفعيل والتعطيل.
    /// </summary>
    /// <param name="id">المعرف الفريد للعميل</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات العميل بعد تبديل الحالة</returns>
    Task<ServiceResult<CustomerResponseDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// إضافة رقم هاتف جديد للعميل مع ضبط الرقم الافتراضي.
    /// </summary>
    /// <param name="customerId">معرف العميل</param>
    /// <param name="dto">بيانات الهاتف الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات العميل المحدثة مع الهواتف</returns>
    Task<ServiceResult<CustomerResponseDto>> AddPhoneAsync(Guid customerId, CustomerPhoneDto dto, CancellationToken ct = default);

    /// <summary>
    /// حذف رقم هاتف محدد للعميل.
    /// </summary>
    /// <param name="customerId">معرف العميل</param>
    /// <param name="phoneId">معرف الهاتف المراد حذفه</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>نتيجة إتمام العملية</returns>
    Task<ServiceResult> DeletePhoneAsync(Guid customerId, Guid phoneId, CancellationToken ct = default);
}
