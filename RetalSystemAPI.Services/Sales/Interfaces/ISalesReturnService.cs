using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Sales;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Sales.Interfaces;

/// <summary>
/// واجهة خدمة إدارة مرتجعات المبيعات واسترجاع البضائع للمخازن.
/// </summary>
public interface ISalesReturnService
{
    Task<ServiceResult<SalesReturnResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<SalesReturnResponseDto>> GetByReturnNumberAsync(string returnNumber, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<SalesReturnSummaryDto>>> GetAllAsync(
        Guid? branchId = null,
        Guid? warehouseId = null,
        Guid? customerId = null,
        SalesReturnReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);
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
    Task<ServiceResult<SalesReturnResponseDto>> CreateAsync(CreateSalesReturnDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
