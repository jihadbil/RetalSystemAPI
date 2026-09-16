using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Desktop.Helpers;

/// <summary>Optional contact email: blank values are accepted, entered addresses are validated.</summary>
public sealed class OptionalEmailAttribute : ValidationAttribute
{
    public OptionalEmailAttribute() : base("أدخل بريدًا إلكترونيًا صحيحًا أو اترك الحقل فارغًا.") { }

    public override bool IsValid(object? value) =>
        value is null || value is string text &&
        (string.IsNullOrWhiteSpace(text) || new EmailAddressAttribute().IsValid(text));
}
