using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Sales.Interfaces;

/// <summary>
/// واجهة خدمة إدارة مرتجعات المبيعات واسترجاع البضائع لمخزون الصالة وتحديث الحسابات.
/// </summary>
public interface ISalesReturnService
{
    /// <summary>جلب تفاصيل مرتجع مبيعات محدد بالمعرف مع بنوده</summary>
    Task<ServiceResult<SalesReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب مرتجع مبيعات بواسطة رقم المرتجع</summary>
    Task<ServiceResult<SalesReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>جلب قائمة بكافة مرتجعات المبيعات مع فلاتر الفرع، الصالة، العميل، وسبب الإرجاع</summary>
    Task<ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>جلب صفحة بيانات مجزأة من مرتجعات المبيعات مع الترقيم</summary>
    Task<ServiceResult<PagedResult<SalesReturnSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>إنشاء مرتجع مبيعات جديد وإعادة كميات البنود لمخزون صالة العرض آلياً</summary>
    Task<ServiceResult<SalesReturnResponseDto>> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default);

    /// <summary>حذف سجل مرتجع مبيعات منطقياً</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
