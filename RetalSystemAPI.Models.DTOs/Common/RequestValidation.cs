using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RetalSystemAPI.Models.DTOs.Common;

/// <summary>
/// سمة التحقق من تحديد وسيلة دفع فردية فعلية واحدة (تستبعد وسيلة Multiple المخصصة لملخص الفاتورة فقط).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SinglePaymentMethodAttribute : ValidationAttribute
{
    /// <summary>
    /// تهيئة سمة التحقق من وسيلة الدفع الفردية.
    /// </summary>
    public SinglePaymentMethodAttribute() : base("اختر طريقة دفع واحدة صحيحة") { }

    /// <summary>
    /// التحقق من أن القيمة المدخلة تنتمي لوسائل الدفع المفردة الصالحة.
    /// </summary>
    public override bool IsValid(object? value) => value is null || value is RetalSystemAPI.Models.Enums.PaymentMethod method &&
        method is >= RetalSystemAPI.Models.Enums.PaymentMethod.Cash and <= RetalSystemAPI.Models.Enums.PaymentMethod.Cheque;
}

/// <summary>
/// سمة التحقق من أن المعرف الفريد Guid ليس فارغاً (Guid.Empty).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    /// <summary>
    /// تهيئة سمة التحقق من عدم فراغ المعرف.
    /// </summary>
    public NotEmptyGuidAttribute() : base("المعرف مطلوب ولا يمكن أن يكون فارغاً") { }

    /// <summary>
    /// التحقق من أن المعرف ليس فارغاً.
    /// </summary>
    public override bool IsValid(object? value) => value is Guid id && id != Guid.Empty;
}

/// <summary>
/// سمة التحقق من عدم احتواء القائمة أو المجموعة على عناصر فارغة (null items).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NoNullItemsAttribute : ValidationAttribute
{
    /// <summary>
    /// تهيئة سمة التحقق من خلو عناصر القائمة من القيم الفارغة.
    /// </summary>
    public NoNullItemsAttribute() : base("القائمة لا يمكن أن تحتوي عناصر فارغة") { }

    /// <summary>
    /// التحقق من سلامة عناصر المجموعة وعدم وجود أي عنصر فارغ.
    /// </summary>
    public override bool IsValid(object? value) => value is null ||
        value is IEnumerable items && items.Cast<object?>().All(item => item is not null);
}

/// <summary>
/// سمة التحقق من أن الكمية المدخلة تمثل عدداً صحيحاً من أصغر وحدة قياس ضمن الحدود المسموح بها.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class WholeQuantityAttribute : ValidationAttribute
{
    /// <summary>هل يُسمح بالكمية صفر</summary>
    public bool AllowZero { get; set; }

    /// <summary>
    /// تهيئة سمة التحقق من صحة الكمية الصحيحة.
    /// </summary>
    public WholeQuantityAttribute() : base("الكمية يجب أن تكون عدداً صحيحاً من أصغر وحدة، ضمن الحد المسموح") { }

    /// <summary>
    /// التحقق من أن الكمية صحيحة وموجبة ومطابقة للشروط.
    /// </summary>
    public override bool IsValid(object? value) => value is null || value is decimal quantity &&
        quantity >= (AllowZero ? 0 : 1) && quantity <= int.MaxValue && quantity == decimal.Truncate(quantity);
}

/// <summary>
/// سمة التحقق من المبالغ المالية ومطابقتها لدقة التخزين المحاسبي decimal(18,4).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MoneyAmountAttribute : ValidationAttribute
{
    /// <summary>الحد الأقصى للمبلغ المالي المدعوم في قاعدة البيانات</summary>
    public const decimal Maximum = 99999999999999.9999m;

    /// <summary>هامش التفاوت المسموح للتمثيل الثنائي للأرقام العشرية من عملاء JSON</summary>
    private const decimal RepresentationTolerance = 0.000000000001m;

    /// <summary>
    /// تقريب وتطبيع المبلغ المالي لأربع منازل عشرية محاسبية.
    /// </summary>
    /// <param name="amount">المبلغ الأصلي.</param>
    /// <returns>المبلغ مقرباً لأربع خانات عشرية.</returns>
    public static decimal Normalize(decimal amount) => decimal.Round(amount, 4);

    /// <summary>
    /// تهيئة سمة التحقق من المبالغ المالية.
    /// </summary>
    public MoneyAmountAttribute() : base("المبلغ يتجاوز الحد المسموح أو يحتوي أكثر من أربع منازل عشرية") { }

    /// <summary>
    /// التحقق من صحة المبلغ وعدم تجاوزه للحد الأقصى أو احتوائه على منازل عشرية إضافية غير مقبولة.
    /// </summary>
    public override bool IsValid(object? value) => value is null || value is decimal amount &&
        amount >= 0 && amount <= Maximum && decimal.Abs(Normalize(amount) - amount) <= RepresentationTolerance;
}

/// <summary>
/// سمة التحقق من صلاحية معرف المنطقة الزمنية لنظام التشغيل (Time Zone ID).
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TimeZoneIdAttribute : ValidationAttribute
{
    /// <summary>
    /// تهيئة سمة التحقق من المنطقة الزمنية.
    /// </summary>
    public TimeZoneIdAttribute() : base("معرف المنطقة الزمنية غير صالح") { }

    /// <summary>
    /// التحقق من وجود المنطقة الزمنية المسماة على النظام.
    /// </summary>
    public override bool IsValid(object? value) => value is null ||
        value is string id && !string.IsNullOrWhiteSpace(id) && TimeZoneInfo.TryFindSystemTimeZoneById(id, out _);
}

/// <summary>
/// فئة مساعدة لفحص والتحقق من صحة نواقل البيانات (DTOs) بشكل تكراري متداخل حتى خارج إطار MVC.
/// </summary>
public static class DtoValidation
{
    /// <summary>
    /// فحص كائن ناقل البيانات واستخراج أول رسالة خطأ إن وجدت.
    /// </summary>
    /// <param name="request">كائن الطلب المراد التحقق منه.</param>
    /// <returns>رسالة الخطأ بالعربية، أو null إذا كان الطلب سليماً تماماً.</returns>
    public static string? GetError(object? request) => Validate(request, new HashSet<object>(ReferenceEqualityComparer.Instance));

    /// <summary>
    /// التحقق التكراري العميق من كائن الطلب وخصائصه وقوائمه المتداخلة مع منع الحلقات التكرارية.
    /// </summary>
    /// <param name="request">كائن الطلب المستهدف.</param>
    /// <param name="visited">مجموعة تتبع الكائنات المفحوصة لمنع الدوران اللانهائي.</param>
    /// <returns>أول رسالة خطأ مواجهة، أو null إن تم الفحص بنجاح.</returns>
    private static string? Validate(object? request, HashSet<object> visited)
    {
        // التحقق من وجود كائن الطلب وعدم تمرير قيمة فارغة
        if (request is null) return "بيانات الطلب مطلوبة";

        // منع الدوران اللانهائي في الكائنات المتداخلة إذا تم فحصها مسبقاً
        if (!visited.Add(request)) return null;

        // التحقق القياسي من سمات التحقق DataAnnotations المعرفة على مستوى الكائن
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), errors, true))
            return errors[0].ErrorMessage ?? "بيانات الطلب غير صالحة";

        // فحص الخصائص التفصيلية والتحقق من الحقول غير القابلة للقيمة الفارغة وتطبيع المبالغ
        var nullability = new NullabilityInfoContext();
        foreach (var property in request.GetType().GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
        {
            // استخراج القيمة الحالية للخاصية
            var value = property.GetValue(request);

            // التحقق من الحقول التي لا تقبل فارغاً وفق نظام C# Nullable Context
            if (value is null && !property.PropertyType.IsValueType && nullability.Create(property).ReadState == NullabilityState.NotNull)
                return $"{property.Name}: الحقل مطلوب";

            // تطبيع وتقريب المبالغ المالية تلقائياً لتوافق دقة التخزين المحاسبي
            if (value is decimal amount && property.CanWrite && property.GetCustomAttribute<MoneyAmountAttribute>() is not null)
                property.SetValue(request, MoneyAmountAttribute.Normalize(amount));

            // الفحص التكراري لمجموعات وقوائم الكائنات المتداخلة
            if (value is IEnumerable collection and not string)
            {
                foreach (var item in collection)
                {
                    // التحقق من خلو عناصر القائمة من القيم الفارغة
                    if (item is null) return "القائمة لا يمكن أن تحتوي عناصر فارغة";

                    // التحقق التكراري من عناصر نواقل البيانات المتداخلة
                    if (item.GetType().Namespace?.StartsWith("RetalSystemAPI.Models.DTOs.", StringComparison.Ordinal) == true &&
                        Validate(item, visited) is { } error) return error;
                }
            }
            // الفحص التكراري للكائنات الفردية المتداخلة
            else if (value is not null && value.GetType().Namespace?.StartsWith("RetalSystemAPI.Models.DTOs.", StringComparison.Ordinal) == true &&
                     value.GetType().IsClass && Validate(value, visited) is { } error) return error;
        }

        // إرجاع خلو الطلب من أية أخطاء
        return null;
    }
}
