namespace RetalSystemAPI.Models.DTOs.Auth;

/// <summary>
/// ناقل بيانات استجابة المصادقة وتسجيل الدخول (Auth Response DTO).
/// يتضمن توكن الوصول JWT، توكن التحديث، وبيانات المستخدم والمستأجر والصلاحيات الممنوحة.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// رمز الوصول المشفر (JWT Bearer Token).
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// رمز تحديث التوكن (Refresh Token) لتجديد الجلسة دون إعادة إدخال بيانات الاعتماد.
    /// </summary>
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// تاريخ ووقت انتهاء صلاحية رمز الوصول (UTC).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// المعرف الفريد للمستخدم (User ID).
    /// </summary>
    public string UserId { get; set; } = null!;

    /// <summary>
    /// اسم المستخدم المسجل في النظام.
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// المعرف الفريد للمستأجر (الشركة/المؤسسة) التابع له المستخدم.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// المعرف الفريد للفرع الافتراضي للمستخدم.
    /// </summary>
    public Guid BranchId { get; set; }

    /// <summary>
    /// قائمة الأدوار الممنوحة للمستخدم (Roles).
    /// </summary>
    public System.Collections.Generic.List<string> Roles { get; set; } = new();

    /// <summary>
    /// قائمة الأذونات والصلاحيات الفعلية المتاحة للمستخدم (Permissions).
    /// </summary>
    public System.Collections.Generic.List<string> Permissions { get; set; } = new();
}
