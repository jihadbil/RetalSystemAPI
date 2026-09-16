using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Purchase.Interfaces;

/// <summary>
/// واجهة خدمة إدارة مرتجعات المشتريات وخصم البضائع المرتجعة من المخازن.
/// </summary>
public interface IPurchaseReturnService
{
    /// <summary>جلب تفاصيل مرتجع المشتريات بالمعرف</summary>
    Task<ServiceResult<PurchaseReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>جلب تفاصيل مرتجع المشتريات برقم المرتجع</summary>
    Task<ServiceResult<PurchaseReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);

    /// <summary>جلب كافة مرتجعات المشتريات مع الفلترة</summary>
    Task<ServiceResult<IReadOnlyList<PurchaseReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>جلب قائمة صفحية لمرتجعات المشتريات</summary>
    Task<ServiceResult<PagedResult<PurchaseReturnSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? supplierId = null,
        PurchaseReturnReason? reason = null,
        PaymentMethod? paymentMethod = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    /// <summary>تسجيل وإصدار مرتجع مشتريات جديد وخصم البضاعة من المخزن</summary>
    Task<ServiceResult<PurchaseReturnResponseDto>> CreateAsync(CreatePurchaseReturnDto dto, CancellationToken ct = default);

    /// <summary>حذف مرتجع مشتريات وإعادة البضاعة للمخزن</summary>
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
