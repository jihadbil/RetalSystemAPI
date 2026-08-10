using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Auth;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Auth.Interfaces;

/// <summary>
/// واجهة خدمة المصادقة وإدارة تسجيل الدخول وحسابات المستخدمين.
/// </summary>
public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<ServiceResult> LogoutAsync(string userId, CancellationToken ct = default);
}
