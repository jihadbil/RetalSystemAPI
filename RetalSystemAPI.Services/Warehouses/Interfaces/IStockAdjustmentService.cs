using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة التسويات الجردية ومعالجة الفوارق وتحديث المخزون الفعلي.
/// </summary>
public interface IStockAdjustmentService
{
    Task<ServiceResult<StockAdjustmentResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<StockAdjustmentResponseDto>> GetByAdjustmentNumberAsync(string adjustmentNumber, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<StockAdjustmentSummaryDto>>> GetAllAsync(
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);
    Task<ServiceResult<PagedResult<StockAdjustmentSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? warehouseId = null,
        StockAdjustmentReason? reason = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);
    Task<ServiceResult<StockAdjustmentResponseDto>> CreateAsync(CreateStockAdjustmentDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
