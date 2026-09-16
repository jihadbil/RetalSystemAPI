using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Branch;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Branch.Interfaces;

namespace RetalSystemAPI.Controllers.Branches;

/// <summary>
/// متحكم إدارة الفروع وأرقام هواتفها الخاصة بالمستأجر.
/// </summary>
[Authorize]
[Route("api/branches")]
public class BranchesController : BaseApiController
{
    private readonly IBranchService _branchService;

    public BranchesController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    /// <summary>
    /// الحصول على جميع الفروع.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Branches.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _branchService.GetAllAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالفروع.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.Branches.View)]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _branchService.GetPagedAsync(pageNumber, pageSize, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل فرع محدد شاملاً أرقام الهواتف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Branches.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _branchService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء فرع جديد.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Branches.Create)]
    public async Task<IActionResult> Create([FromBody] CreateBranchDto dto, CancellationToken ct)
    {
        var result = await _branchService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<BranchResponseDto>.Ok(result.Data!, "تم إنشاء الفرع بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات فرع وهواتفه.
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Branches.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateBranchDto dto, CancellationToken ct)
    {
        var result = await _branchService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف فرع (حذف منطقي).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Branches.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _branchService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة النشاط للفرع (تفعيل / تعطيل).
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Branches.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _branchService.ToggleActiveStatusAsync(id, ct);
        return ToActionResult(result);
    }
}
