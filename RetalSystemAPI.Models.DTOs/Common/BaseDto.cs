namespace RetalSystemAPI.Models.DTOs.Common;

/// <summary>
/// الناقل الأساسي المشترك بين جميع نواقل البيانات (DTOs) في النظام.
/// يحتوي فقط على الحقول المطلوبة التي يحتاجها العميل.
/// </summary>
public abstract class BaseDto
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
