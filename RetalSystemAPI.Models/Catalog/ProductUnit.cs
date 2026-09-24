using System;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;

/// <summary>
/// يمثل ربط وحدات القياس والعبوات بالمنتج (قطعة، دستة، صندوق) وتحديد معامل التحويل لاحتساب التكلفة والكميات تلقائياً.
/// </summary>
// كلاس كيان وحدة قياس المنتج ومعامل التحويل
public class ProductUnit : TenantBaseEntity
{
    /// <summary>
    /// معرّف الصنف والمنتج الذي تنتمي إليه وحدة القياس.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج المرتبط بوحدة القياس (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product? Product { get; set; }

    /// <summary>
    /// معرّف وحدة القياس العامة المسجلة في النظام (قطعة، صندوق، كرتونة).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الوحدة العامة
    public Guid UnitId { get; set; }

    /// <summary>
    /// كائن وحدة القياس العامة المرتبطة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان وحدة القياس
    public Unit? Unit { get; set; }

    /// <summary>
    /// معامل التحويل إلى الوحدة الأساسية (مثال: الصندوق يحتوي على 12 قطعة، فالقيمة 12).
    /// </summary>
    // خاصية معامل التحويل للوحدة الصغرى
    public int ConversionFactor { get; set; }

    /// <summary>
    /// يحدد ما إذا كانت هذه الوحدة هي الوحدة الافتراضية للبيع والعرض.
    /// </summary>
    // خاصية مؤشر الوحدة الافتراضية للمنتج
    public bool IsDefault { get; set; } = false;    
}
