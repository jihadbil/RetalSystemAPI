using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.ProductUnit;

/// <summary>
/// ناقل بيانات استجابة وحدة قياس المنتج (Product Unit Response DTO).
/// </summary>
public class ProductUnitResponseDto : BaseDto
{
    /// <summary>معرف المنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>معرف الوحدة</summary>
    public Guid UnitId { get; set; }

    /// <summary>اسم وحدة القياس</summary>
    public string UnitName { get; set; } = null!;

    /// <summary>معامل التحويل إلى الوحدة الأساسية</summary>
    public int ConversionFactor { get; set; }

    /// <summary>هل هي الوحدة الافتراضية</summary>
    public bool IsDefault { get; set; }
}
