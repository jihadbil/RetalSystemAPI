using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace RetalSystemAPI.Models.DTOs.Common;

[AttributeUsage(AttributeTargets.Property)]
public sealed class NotEmptyGuidAttribute : ValidationAttribute
{
    public NotEmptyGuidAttribute() : base("المعرف مطلوب ولا يمكن أن يكون فارغاً") { }
    public override bool IsValid(object? value) => value is Guid id && id != Guid.Empty;
}

/// <summary>Rejects null elements without changing whether the collection itself is optional.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NoNullItemsAttribute : ValidationAttribute
{
    public NoNullItemsAttribute() : base("القائمة لا يمكن أن تحتوي عناصر فارغة") { }
    public override bool IsValid(object? value) => value is null ||
        value is IEnumerable items && items.Cast<object?>().All(item => item is not null);
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class WholeQuantityAttribute : ValidationAttribute
{
    public bool AllowZero { get; set; }
    public WholeQuantityAttribute() : base("الكمية يجب أن تكون عدداً صحيحاً من أصغر وحدة، ضمن الحد المسموح") { }
    public override bool IsValid(object? value) => value is null || value is decimal quantity &&
        quantity >= (AllowZero ? 0 : 1) && quantity <= int.MaxValue && quantity == decimal.Truncate(quantity);
}

/// <summary>Matches the decimal(18,4) columns used for monetary amounts.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MoneyAmountAttribute : ValidationAttribute
{
    public const decimal Maximum = 99999999999999.9999m;
    // JSON clients use binary floating point. Accept its insignificant representation noise,
    // while rejecting a meaningful fifth decimal place; services normalize accepted values.
    private const decimal RepresentationTolerance = 0.000000000001m;
    public static decimal Normalize(decimal amount) => decimal.Round(amount, 4);
    public MoneyAmountAttribute() : base("المبلغ يتجاوز الحد المسموح أو يحتوي أكثر من أربع منازل عشرية") { }
    public override bool IsValid(object? value) => value is null || value is decimal amount &&
        amount >= 0 && amount <= Maximum && decimal.Abs(Normalize(amount) - amount) <= RepresentationTolerance;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class TimeZoneIdAttribute : ValidationAttribute
{
    public TimeZoneIdAttribute() : base("معرف المنطقة الزمنية غير صالح") { }
    public override bool IsValid(object? value) => value is null ||
        value is string id && !string.IsNullOrWhiteSpace(id) && TimeZoneInfo.TryFindSystemTimeZoneById(id, out _);
}

/// <summary>Applies request annotations recursively for callers outside the MVC pipeline as well.</summary>
public static class DtoValidation
{
    public static string? GetError(object? request) => Validate(request, new HashSet<object>(ReferenceEqualityComparer.Instance));

    private static string? Validate(object? request, HashSet<object> visited)
    {
        if (request is null) return "بيانات الطلب مطلوبة";
        if (!visited.Add(request)) return null;
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(request, new ValidationContext(request), errors, true))
            return errors[0].ErrorMessage ?? "بيانات الطلب غير صالحة";

        var nullability = new NullabilityInfoContext();
        foreach (var property in request.GetType().GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
        {
            var value = property.GetValue(request);
            if (value is null && !property.PropertyType.IsValueType && nullability.Create(property).ReadState == NullabilityState.NotNull)
                return $"{property.Name}: الحقل مطلوب";
            if (value is decimal amount && property.CanWrite && property.GetCustomAttribute<MoneyAmountAttribute>() is not null)
                property.SetValue(request, MoneyAmountAttribute.Normalize(amount));
            if (value is IEnumerable collection and not string)
            {
                foreach (var item in collection)
                {
                    if (item is null) return "القائمة لا يمكن أن تحتوي عناصر فارغة";
                    if (item.GetType().Namespace?.StartsWith("RetalSystemAPI.Models.DTOs.", StringComparison.Ordinal) == true &&
                        Validate(item, visited) is { } error) return error;
                }
            }
            else if (value is not null && value.GetType().Namespace?.StartsWith("RetalSystemAPI.Models.DTOs.", StringComparison.Ordinal) == true &&
                     value.GetType().IsClass && Validate(value, visited) is { } error) return error;
        }
        return null;
    }
}
