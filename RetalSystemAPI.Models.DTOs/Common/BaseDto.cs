namespace RetalSystemAPI.Models.DTOs.Common;

/// <summary>
/// الناقل الأساسي المشترك بين جميع نواقل البيانات (Base DTO) في النظام.
/// يوفر الحقول الأساسية الثابتة للهوية وتتبع الإنشاء والتعديل.
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// المعرف الفريد للكيان (Primary Key).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// تاريخ وتوقيت إنشاء السجل بالتوقيت العالمي المنسق (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ وتوقيت آخر تعديل على السجل بالتوقيت العالمي (UTC)، ويكون فارغاً إذا لم يُعدل.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
