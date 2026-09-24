namespace RetalSystemAPI.Models.Common;

/// <summary>
/// ثوابت وقواعد التحقق من صحة المدخلات (Validation Constants) المستخدمة عبر نماذج النظام.
/// </summary>
// كلاس ثابت يحتوي على تعابير المطابقة القياسية ورسائل التحقق من صحة البيانات
public static class ValidationConstants
{
    /// <summary>
    /// التعبير النمطي (Regex) للتحقق من صحة أرقام الهواتف الليبية (09XXXXXXXX أو +2189XXXXXXXX).
    /// </summary>
    // نمط التعبير المنتظم لأرقام الهواتف النقالة في ليبيا
    public const string LibyanPhonePattern = @"^(09\d{8}|\+2189\d{8})$";

    /// <summary>
    /// رسالة الخطأ المعيارية عند إدخال رقم هاتف ليبي غير مطابق للنمط المعتمد.
    /// </summary>
    // رسالة الخطأ التوضيحية عند فشل التحقق من رقم الهاتف
    public const string LibyanPhoneError = "رقم الهاتف غير صحيح. يجب أن يبدأ بـ 09 أو +2189";
}
