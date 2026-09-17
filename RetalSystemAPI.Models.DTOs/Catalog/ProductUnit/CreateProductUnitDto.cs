using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

/// <summary>
/// ناقل بيانات ربط وحدة قياس بالمنتج (Create Product Unit DTO).
/// يحدد معامل التحويل بين الوحدة والوحدة الأساسية للصنف.
/// </summary>
public class CreateProductUnitDto
{
    /// <summary>المعرف الفريد للمنتج</summary>
    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>المعرف الفريد لوحدة القياس المراد ربطها</summary>
    [Required(ErrorMessage = "معرف الوحدة مطلوب")]
    public Guid UnitId { get; set; }

    /// <summary>معامل التحويل (عدد الوحدات الأساسية التي تعادلها هذه الوحدة)</summary>
    [Range(1, int.MaxValue, ErrorMessage = "معامل التحويل يجب أن يكون 1 على الأقل")]
    public int ConversionFactor { get; set; }

    /// <summary>هل هي وحدة الصنف الافتراضية للبيع والعرض</summary>
    public bool IsDefault { get; set; } = false;
}
