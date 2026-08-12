using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Suppliers;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Suppliers.Interfaces;

/// <summary>
/// واجهة خدمة إدارة الموردين وأرقام هواتفهم.
/// </summary>
public interface ISupplierService
{
    Task<ServiceResult<SupplierResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<SupplierSummaryDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<PagedResult<SupplierSummaryDto>>> GetPagedAsync(int pageNumber, int pageSize, string? search = null, CancellationToken ct = default);
    Task<ServiceResult<SupplierResponseDto>> CreateAsync(CreateSupplierDto dto, CancellationToken ct = default);
    Task<ServiceResult<SupplierResponseDto>> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<SupplierResponseDto>> AddPhoneAsync(Guid supplierId, SupplierPhoneDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeletePhoneAsync(Guid supplierId, Guid phoneId, CancellationToken ct = default);
}
