using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Filters;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Users;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Users.Interfaces;

namespace RetalSystemAPI.Controllers.Users;

/// <summary>
/// متحكم إدارة المستخدمين وأدوارهم ومصفوفة الصلاحيات في النظام.
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
    [HasPermission(Permissions.Users.View)]
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
    [HasPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetById([FromRoute] string id, CancellationToken ct = default)
    {
        var result = await _userService.GetUserByIdAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء مستخدم جديد.
    /// </summary>
    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
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
    [HasPermission(Permissions.Users.Edit)]
    public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateUserDto dto, CancellationToken ct = default)
    {
        var result = await _userService.UpdateUserAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف مستخدم.
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission(Permissions.Users.Delete)]
    public async Task<IActionResult> Delete([FromRoute] string id, CancellationToken ct = default)
    {
        var result = await _userService.DeleteUserAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إعادة تعيين كلمة المرور للمستخدم.
    /// </summary>
    [HttpPost("{id}/reset-password")]
    [HasPermission(Permissions.Users.ResetPassword)]
    public async Task<IActionResult> ResetPassword([FromRoute] string id, [FromBody] ResetPasswordDto dto, CancellationToken ct = default)
    {
        var result = await _userService.ResetPasswordAsync(id, dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على جميع الأدوار المتاحة في النظام.
    /// </summary>
    [HttpGet("roles")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetRoles(CancellationToken ct = default)
    {
        var result = await _userService.GetRolesAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء دور جديد في النظام.
    /// </summary>
    [HttpPost("roles")]
    [HasPermission(Permissions.Roles.Manage)]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request, CancellationToken ct = default)
    {
        var result = await _userService.CreateRoleAsync(request.Name, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<RoleDto>.Ok(result.Data!, "تم إنشاء الدور بنجاح"));
        }
        return ToActionResult(result);
    }

    /// <summary>
    /// حذف دور من النظام.
    /// </summary>
    [HttpDelete("roles/{id}")]
    [HasPermission(Permissions.Roles.Manage)]
    public async Task<IActionResult> DeleteRole([FromRoute] string id, CancellationToken ct = default)
    {
        var result = await _userService.DeleteRoleAsync(id, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على جميع أذونات وصلاحيات الشاشات والعمليات المعرفة في النظام.
    /// </summary>
    [HttpGet("permissions")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetAllPermissions(CancellationToken ct = default)
    {
        var result = await _userService.GetAllPermissionsAsync(ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على مصفوفة صلاحيات دور محدد.
    /// </summary>
    [HttpGet("roles/{roleId}/permissions")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetRolePermissions([FromRoute] string roleId, CancellationToken ct = default)
    {
        var result = await _userService.GetRolePermissionsAsync(roleId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث وحفظ مصفوفة صلاحيات دور محدد.
    /// </summary>
    [HttpPut("roles/{roleId}/permissions")]
    [HasPermission(Permissions.Roles.Manage)]
    public async Task<IActionResult> UpdateRolePermissions([FromRoute] string roleId, [FromBody] UpdateRolePermissionsDto dto, CancellationToken ct = default)
    {
        dto.RoleId = roleId;
        var result = await _userService.UpdateRolePermissionsAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// الحصول على الصلاحيات المباشرة والفعالة لمستخدم محدد.
    /// </summary>
    [HttpGet("{userId}/permissions")]
    [HasPermission(Permissions.Roles.View)]
    public async Task<IActionResult> GetUserPermissions([FromRoute] string userId, CancellationToken ct = default)
    {
        var result = await _userService.GetUserPermissionsAsync(userId, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// تحديث الصلاحيات المباشرة لمستخدم محدد.
    /// </summary>
    [HttpPut("{userId}/permissions")]
    [HasPermission(Permissions.Roles.Manage)]
    public async Task<IActionResult> UpdateUserPermissions([FromRoute] string userId, [FromBody] UpdateUserPermissionsDto dto, CancellationToken ct = default)
    {
        dto.UserId = userId;
        var result = await _userService.UpdateUserPermissionsAsync(dto, ct);
        return ToActionResult(result);
    }
}

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
}
