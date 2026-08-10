using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Branch.Interfaces;

/// <summary>
/// واجهة خدمة الفروع لتنفيذ عمليات إدارة فروع المستأجر.
/// </summary>
public interface IBranchService
{
    Task<ServiceResult<BranchResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<BranchResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<PagedResult<BranchResponseDto>>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<ServiceResult<BranchResponseDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default);
    Task<ServiceResult<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
