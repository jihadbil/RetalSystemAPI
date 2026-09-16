namespace RetalSystemAPI.DataAccess.Services;

/// <summary>
/// واجهة توفر معرف المستخدم الحالي (UserId) المستخرج من سياق الطلب لتدقيق العمليات (Audit Logging).
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// المعرف الفريد للمستخدم الحالي المنفذ للعملية، أو null للعمليات غير المصادق عليها.
    /// </summary>
    string? UserId { get; }
}
