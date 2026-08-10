using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RetalSystemAPI.Controllers.Base;
using RetalSystemAPI.Models.DTOs.Auth;
using RetalSystemAPI.Responses;
using RetalSystemAPI.Services.Auth.Interfaces;

namespace RetalSystemAPI.Controllers.Auth;

/// <summary>
/// متحكم عمليات المصادقة وإدارة الحسابات وتدفق تسجيل الدخول والخروج.
/// </summary>
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// تسجيل الدخول للمستخدم والحصول على رمز التوثيق JWT.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(dto, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// إنشاء حساب مستخدم جديد.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(dto, ct);
        if (result.IsSuccess)
        {
            return StatusCode(201, ApiResponse<AuthResponseDto>.Ok(result.Data!, "تم إنشاء الحساب بنجاح"));
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// تسجيل الخروج للمستخدم وإبطال جلسة العمل الحالية.
    /// </summary>
    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Fail("المستخدم غير مصرح له", "UNAUTHORIZED"));
        }

        var result = await _authService.LogoutAsync(userId, ct);
        return ToActionResult(result);
    }
}
