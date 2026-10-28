namespace RetalSystemAPI.Models.DTOs.Auth;

/// <summary>
/// هوية الجلسة وصلاحياتها بعد التحقق من رمز الوصول على الخادم.
/// </summary>
public sealed class SessionUserDto
{
    /// <summary>معرف المستخدم الفريد في نظام الهوية</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>اسم المستخدم المسجل للدخول</summary>
    public string UserName { get; init; } = string.Empty;

    /// <summary>الاسم الكامل للمستخدم</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>البريد الإلكتروني للمستخدم</summary>
    public string? Email { get; init; }

    /// <summary>معرف المستأجر / المؤسسة التابع لها</summary>
    public Guid TenantId { get; init; }

    /// <summary>معرف الفرع الافتراضي للمستخدم</summary>
    public Guid BranchId { get; init; }

    /// <summary>قائمة الأدوار الممنوحة للمستخدم</summary>
    public List<string> Roles { get; init; } = [];

    /// <summary>قائمة الصلاحيات الفعلية المفعلة للمستخدم</summary>
    public List<string> Permissions { get; init; } = [];
}
