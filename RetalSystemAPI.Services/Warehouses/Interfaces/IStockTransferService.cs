using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة عمليات التحويل المخزني ونقل البضائع بين المخازن وصالات العرض.
/// </summary>
public interface IStockTransferService
{
    Task<ServiceResult<StockTransferResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<StockTransferResponseDto>> GetByTransferNumberAsync(string transferNumber, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<StockTransferSummaryDto>>> GetAllAsync(
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);
    Task<ServiceResult<PagedResult<StockTransferSummaryDto>>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Guid? fromWarehouseId = null,
        Guid? toWarehouseId = null,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);
    Task<ServiceResult<StockTransferResponseDto>> CreateAsync(CreateStockTransferDto dto, CancellationToken ct = default);
    Task<ServiceResult<StockTransferResponseDto>> UpdateAsync(Guid id, UpdateStockTransferDto dto, CancellationToken ct = default);
    Task<ServiceResult<StockTransferResponseDto>> UpdateStatusAsync(Guid id, StockTransferStatus status, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
