namespace RetalSystemAPI.Models.Common;

/// <summary>
/// ثوابت التحقق من الصحة المستخدمة عبر نماذج النظام.
/// </summary>
public static class ValidationConstants
{
    public const string LibyanPhonePattern = @"^(09\d{8}|\+2189\d{8})$";
    public const string LibyanPhoneError = "رقم الهاتف غير صحيح. يجب أن يبدأ بـ 09 أو +2189";
}
