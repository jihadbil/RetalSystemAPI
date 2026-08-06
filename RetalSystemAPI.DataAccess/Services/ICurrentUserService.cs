namespace RetalSystemAPI.DataAccess.Services;

/// <summary>
/// واجهة توفر معرف المستخدم الحالي المستخرج من سياق الطلب.
/// </summary>
public interface ICurrentUserService
{
    string? UserId { get; }
}
