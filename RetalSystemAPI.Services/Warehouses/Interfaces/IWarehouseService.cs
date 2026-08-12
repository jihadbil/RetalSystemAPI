using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Warehouses;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Warehouses.Interfaces;

/// <summary>
/// واجهة خدمة إدارة المخازن وصالات العرض.
/// </summary>
public interface IWarehouseService
{
    Task<ServiceResult<WarehouseResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<WarehouseSummaryDto>>> GetAllAsync(Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<WarehouseSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, Guid? branchId = null, WarehouseType? type = null, CancellationToken ct = default);
    Task<ServiceResult<WarehouseResponseDto>> CreateAsync(CreateWarehouseDto dto, CancellationToken ct = default);
    Task<ServiceResult<WarehouseResponseDto>> UpdateAsync(Guid id, UpdateWarehouseDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
