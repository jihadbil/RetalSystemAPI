namespace RetalSystemAPI.Services.Common.Interfaces;

/// <summary>
/// واجهة التحقق من صلاحيات المستخدم الحالي داخل طبقة الخدمات (سلوك مطابق لفلتر HasPermissionAttribute).
/// </summary>
public interface ICurrentPermissionService
{
    /// <summary>
    /// التحقق من امتلاك المستخدم الحالي للصلاحية المحددة — يرجع true لمسؤولي النظام تلقائياً.
    /// </summary>
    /// <param name="permission">رمز الصلاحية المطلوب التحقق منها</param>
    bool HasPermission(string permission);
}
