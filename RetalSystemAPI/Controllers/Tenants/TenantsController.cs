using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Tenant;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Tenant.Interfaces;

namespace RetalSystemAPI.Controllers.Tenants;

/// <summary>
/// متحكم إدارة المستأجرين (Tenants) على مستوى النظام.
/// </summary>
[Authorize]
[Route("api/tenants")]
public class TenantsController : BaseApiController
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// الحصول على بيانات المستأجر الحالي (البروفايل).
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId) || tenantId == Guid.Empty)
        {
            return Unauthorized(ApiResponse<TenantResponseDto>.Fail("لم يتم العثور على معرف المستأجر في رمز التوثيق", "UNAUTHORIZED"));
        }

        var result = await _tenantService.GetByIdAsync(tenantId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات المستأجر الحالي (البروفايل).
    /// </summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTenantDto dto, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId) || tenantId == Guid.Empty)
        {
            return Unauthorized(ApiResponse<TenantResponseDto>.Fail("لم يتم العثور على معرف المستأجر في رمز التوثيق", "UNAUTHORIZED"));
        }

        var result = await _tenantService.UpdateAsync(tenantId, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة بكل المستأجرين.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _tenantService.GetAllAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالمستأجرين.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _tenantService.GetPagedAsync(pageNumber, pageSize, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل مستأجر محدد بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _tenantService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء مستأجر جديد.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantDto dto, CancellationToken ct)
    {
        var result = await _tenantService.CreateAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<TenantResponseDto>.Ok(result.Data!, "تم إنشاء المستأجر بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات مستأجر موجود.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTenantDto dto, CancellationToken ct)
    {
        var result = await _tenantService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مستأجر.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _tenantService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة النشاط للمستأجر (تفعيل / تعطيل).
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _tenantService.ToggleActiveStatusAsync(id, ct);
        return ToActionResult(result);
    }
}
