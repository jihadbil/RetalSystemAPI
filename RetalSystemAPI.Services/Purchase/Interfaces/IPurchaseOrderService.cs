using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Purchase;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Purchase.Interfaces;

/// <summary>
/// واجهة خدمة إدارة طلبية الشراء وتقارير الطلبيات.
/// </summary>
public interface IPurchaseOrderService
{
    Task<ServiceResult<PurchaseOrderResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<PurchaseOrderSummaryDto>>> GetAllAsync(Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<PurchaseOrderSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, Guid? warehouseId = null, PurchaseOrderStatus? status = null, string? search = null, CancellationToken ct = default);
    Task<ServiceResult<PurchaseOrderResponseDto>> CreateAsync(CreatePurchaseOrderDto dto, CancellationToken ct = default);
    Task<ServiceResult<PurchaseOrderResponseDto>> UpdateAsync(Guid id, UpdatePurchaseOrderDto dto, CancellationToken ct = default);
    Task<ServiceResult<PurchaseOrderResponseDto>> UpdateStatusAsync(Guid id, PurchaseOrderStatus status, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
