using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;
using RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة رصيد المخزون (مخازن وصالات عرض).
/// </summary>
public interface IStockService
{
    // ── Storge Stock (مخزون المخازن بالباركود) ───────────
    Task<ServiceResult<StorgeStockResponseDto>> GetStorgeStockAsync(Guid warehouseId, Guid productBarcodeId, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetStorgeStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
    Task<ServiceResult<StorgeStockResponseDto>> SetStorgeStockAsync(SetStorgeStockDto dto, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<StorgeStockResponseDto>>> GetLowStorgeStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default);

    // ── Showroom Stock (مخزون صالة العرض بالمنتج) ──────────
    Task<ServiceResult<ShowroomStockResponseDto>> GetShowroomStockAsync(Guid warehouseId, Guid productId, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetShowroomStocksByWarehouseAsync(Guid warehouseId, CancellationToken ct = default);
    Task<ServiceResult<ShowroomStockResponseDto>> SetShowroomStockAsync(SetShowroomStockDto dto, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<ShowroomStockResponseDto>>> GetLowShowroomStockAlertsAsync(Guid? warehouseId = null, CancellationToken ct = default);
}
