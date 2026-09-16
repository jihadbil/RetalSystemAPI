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
    /// <summary>جلب تفاصيل مورد محدد بواسطة المعرف مع هواتفه</summary>
    Task<ServiceResult<SupplierResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة الموردين</summary>
    Task<ServiceResult<IReadOnlyList<SupplierSummaryDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من الموردين مع إمكانية البحث بالاسم أو العنوان أو الهاتف</summary>
    Task<ServiceResult<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, string? search = null, CancellationToken ct = default);

    /// <summary>إنشاء مورد جديد مع التحقق من عدم تكرار الاسم</summary>
    Task<ServiceResult<SupplierResponseDto>> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default);

    /// <summary>تحديث بيانات مورد موجود</summary>
    Task<ServiceResult<SupplierResponseDto>> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken ct = default);

    /// <summary>حذف مورد منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>إضافة رقم هاتف جديد للمورد</summary>
    Task<ServiceResult<SupplierResponseDto>> AddPhoneAsync(Guid supplierId, SupplierPhoneDto dto, CancellationToken ct = default);

    /// <summary>حذف رقم هاتف للمورد</summary>
    Task<ServiceResult> DeletePhoneAsync(Guid supplierId, Guid phoneId, CancellationToken ct = default);
}
