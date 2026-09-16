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
    /// <summary>جلب تفاصيل عميل محدد بالمعرف مع هواتفه</summary>
    Task<ServiceResult<CustomerResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب قائمة بالعملاء مع إمكانية الفلترة حسب النوع وحالة النشاط والبحث بالاسم أو الكود أو الهاتف</summary>
    Task<ServiceResult<IReadOnlyList<CustomerSummaryDto>>> GetAllAsync(CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من العملاء مع الترقيم</summary>
    Task<ServiceResult<PagedResult<CustomerSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, CustomerType? type = null, bool? isActive = null, string? search = null, CancellationToken ct = default);

    /// <summary>إنشاء عميل جديد مع التحقق من عدم تكرار الكود</summary>
    Task<ServiceResult<CustomerResponseDto>> CreateAsync(CreateCustomerDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات عميل موجود</summary>
    Task<ServiceResult<CustomerResponseDto>> UpdateAsync(Guid id, UpdateCustomerDto dto, CancellationToken ct = default);

    /// <summary>حذف عميل منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>تبديل حالة نشاط العميل</summary>
    Task<ServiceResult<CustomerResponseDto>> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);

    /// <summary>إضافة رقم هاتف جديد للعميل</summary>
    Task<ServiceResult<CustomerResponseDto>> AddPhoneAsync(Guid customerId, CustomerPhoneDto dto, CancellationToken ct = default);

    /// <summary>حذف رقم هاتف للعميل</summary>
    Task<ServiceResult> DeletePhoneAsync(Guid customerId, Guid phoneId, CancellationToken ct = default);
}
