using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;

/// <summary>
/// يمثل وحدات القياس والعبوات المعرفة في النظام (قطعة، صندوق، دستة، كيس، لتر، كجم).
/// </summary>
// كلاس كيان وحدة القياس العامة
public class Unit : TenantBaseEntity
{
    /// <summary>
    /// اسم وحدة القياس (مثال: حبة، صندوق، باكت).
    /// </summary>
    // خاصية اسم وحدة القياس الإلزامي
    public required string Name { get; set; }

    /// <summary>
    /// وصف وإيضاح تفاصيل وحدة القياس واستخدامها.
    /// </summary>
    // خاصية وصف وحدة القياس
    public string? Description { get; set; }

    /// <summary>
    /// سعة أو عدد القطع القياسي المضمن في العبوة الواحدة بشكل عام (الافتراضي 1).
    /// </summary>
    // خاصية سعة العبوة القياسية
    public int UnitPackage { get; set; } = 1;

    /// <summary>
    /// قائمة سجلات ربط هذه الوحدة بالمنتجات المختلفة ومعاملات تحويلها (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة وحدات المنتجات المرتبطة
    public ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();
}
