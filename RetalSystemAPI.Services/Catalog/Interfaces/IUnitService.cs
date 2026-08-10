using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Catalog.Unit;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Catalog.Interfaces;

/// <summary>
/// واجهة خدمة وحدات القياس للمنتجات.
/// </summary>
public interface IUnitService
{
    Task<ServiceResult<UnitResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<UnitResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<UnitResponseDto>> CreateAsync(CreateUnitDto dto, CancellationToken ct = default);
    Task<ServiceResult<UnitResponseDto>> UpdateAsync(Guid id, UpdateUnitDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
