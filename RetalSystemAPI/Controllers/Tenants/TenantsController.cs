using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
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
    private readonly RetalSystemAPI.Services.FileUpload.Interfaces.IFileUploadService _fileUploadService;

    public TenantsController(
        ITenantService tenantService,
        RetalSystemAPI.Services.FileUpload.Interfaces.IFileUploadService fileUploadService)
    {
        _tenantService = tenantService;
        _fileUploadService = fileUploadService;
    }

    /// <summary>
    /// الحصول على بيانات المستأجر الحالي (البروفايل).
    /// </summary>
    [HttpGet("me")]
    [HasPermission(Permissions.Tenants.View)]
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
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateTenantDto dto, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId) || tenantId == Guid.Empty)
        {
            return Unauthorized(ApiResponse<TenantResponseDto>.Fail("لم يتم العثور على معرف المستأجر في رمز التوثيق", "UNAUTHORIZED"));
        }

        dto.Id = tenantId;
        var result = await _tenantService.UpdateAsync(tenantId, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// رفع وتحديث شعار المستأجر الحالي.
    /// </summary>
    [HttpPost("me/logo")]
    [Consumes("multipart/form-data")]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> UploadLogo(Microsoft.AspNetCore.Http.IFormFile file, CancellationToken ct)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId) || tenantId == Guid.Empty)
        {
            return Unauthorized(ApiResponse<TenantResponseDto>.Fail("لم يتم العثور على معرف المستأجر في رمز التوثيق", "UNAUTHORIZED"));
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(ApiResponse<TenantResponseDto>.Fail("ملف الشعار مطلوب", "VALIDATION_ERROR"));
        }

        if (!_fileUploadService.IsValidImageExtension(file.FileName))
        {
            return BadRequest(ApiResponse<TenantResponseDto>.Fail("نوع الملف غير مسموح به، يرجى رفع صورة صالحة (.jpg, .png, .webp)", "INVALID_FILE_TYPE"));
        }

        if (!_fileUploadService.IsWithinSizeLimit(file.Length))
        {
            return BadRequest(ApiResponse<TenantResponseDto>.Fail("حجم الصورة يتجاوز الحد المسموح به (5 ميجابايت)", "FILE_TOO_LARGE"));
        }

        var uploadResult = await _fileUploadService.UploadAsync(file, "tenants/logos", ct);
        if (!uploadResult.IsSuccess)
        {
            return ToActionResult(uploadResult);
        }

        var updateResult = await _tenantService.UpdateLogoAsync(tenantId, uploadResult.Data!, ct);
        return ToActionResult(updateResult);
    }

    /// <summary>
    /// الحصول على قائمة بكل المستأجرين.
    /// </summary>
    [HttpGet]
    [HasPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _tenantService.GetAllAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالمستأجرين.
    /// </summary>
    [HttpGet("paged")]
    [HasPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetPaged([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await _tenantService.GetPagedAsync(pageNumber, pageSize, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل مستأجر محدد بالمعرف.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Tenants.View)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _tenantService.GetByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء مستأجر جديد.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Tenants.Edit)]
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
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateTenantDto dto, CancellationToken ct)
    {
        var result = await _tenantService.UpdateAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مستأجر.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _tenantService.DeleteAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تبديل حالة النشاط للمستأجر (تفعيل / تعطيل).
    /// </summary>
    [HttpPatch("{id:guid}/toggle-active")]
    [HasPermission(Permissions.Tenants.Edit)]
    public async Task<IActionResult> ToggleActiveStatus([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _tenantService.ToggleActiveStatusAsync(id, ct);
        return ToActionResult(result);
    }
}
