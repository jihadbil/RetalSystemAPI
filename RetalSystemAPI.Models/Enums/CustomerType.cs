namespace RetalSystemAPI.Models.Enums;

/// <summary>
/// يحدد تصنيف وطبيعة تعامل العميل التجاري في النظام.
/// </summary>
// تعريف تعداد أنواع وتصنيفات العملاء
public enum CustomerType
{
    /// <summary>
    /// عميل قطاعي (تجزئة / أفراد).
    /// </summary>
    // عميل تجزئة قطاعي
    Retail = 1,

    /// <summary>
    /// عميل جملة (تجار ومحلات تجارية).
    /// </summary>
    // عميل جملة
    Wholesale = 2,

    /// <summary>
    /// شركات ومؤسسات وهيئات اعتبارية وحكومية.
    /// </summary>
    // عميل شركات وجهات اعتبارية
    Corporate = 3
}
