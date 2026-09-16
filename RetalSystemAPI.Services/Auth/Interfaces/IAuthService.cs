using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models.DTOs.Auth;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Auth.Interfaces;

/// <summary>
/// واجهة خدمة المصادقة وإدارة تسجيل الدخول وإنشاء حسابات المستخدمين وتوليد رموز التوثيق (JWT Tokens).
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// تسجيل الدخول للمستخدم والتحقق من كلمة المرور وإرجاع بيانات الجلسة وتوكن JWT.
    /// </summary>
    /// <param name="dto">بيانات تسجيل الدخول (اسم المستخدم/البريد وكلمة المرور)</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المصادقة والتوكن أو رسالة خطأ</returns>
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);

    /// <summary>
    /// تسجيل مستخدم جديد وربطه بالمستأجر والفرع المحددين.
    /// </summary>
    /// <param name="dto">بيانات تسجيل المستخدم الجديد</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>بيانات المصادقة وتوكن الحساب المنشأ</returns>
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);

    /// <summary>
    /// تسجيل خروج المستخدم وإنهاء الجلسة.
    /// </summary>
    /// <param name="userId">معرف المستخدم</param>
    /// <param name="ct">رمز إلغاء العملية</param>
    Task<ServiceResult> LogoutAsync(string userId, CancellationToken ct = default);
}
