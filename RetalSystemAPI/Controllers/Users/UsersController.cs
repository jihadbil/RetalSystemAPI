using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Users.Interfaces;

namespace RetalSystemAPI.Controllers.Users;

/// <summary>
/// متحكم إدارة المستخدمين وأدوارهم في النظام.
/// </summary>
[Authorize]
[Route("api/users")]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// الحصول على قائمة صفحية بالمستخدمين مع إمكانية البحث والتصفية بحسب الفرع.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? branchId = null,
        CancellationToken ct = default)
    {
        var result = await _userService.GetPagedUsersAsync(pageNumber, pageSize, search, branchId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على تفاصيل مستخدم محدد.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken ct = default)
    {
        var result = await _userService.GetUserByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء مستخدم جديد.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto, CancellationToken ct = default)
    {
        var result = await _userService.CreateUserAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<UserSummaryDto>.Ok(result.Data!, "تم إنشاء حساب المستخدم بنجاح"));
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// تعديل بيانات مستخدم.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateUserDto dto, CancellationToken ct = default)
    {
        var result = await _userService.UpdateUserAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مستخدم.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken ct = default)
    {
        var result = await _userService.DeleteUserAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إعادة تعيين كلمة المرور للمستخدم.
    /// </summary>
    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword([FromRoute] string id, [FromBody] ResetPasswordDto dto, CancellationToken ct = default)
    {
        var result = await _userService.ResetPasswordAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على جميع الأدوار المتاحة في النظام.
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct = default)
    {
        var result = await _userService.GetRolesAsync(ct);
        return ToActionResult(result);
    }
}
