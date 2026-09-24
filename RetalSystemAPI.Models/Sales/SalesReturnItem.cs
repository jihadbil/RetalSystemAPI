using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Sales;

/// <summary>
/// يمثل بنداً واحداً تفصيلياً ضمن بنود سند مرتجع المبيعات يحدد الصنف والباركود والكمية المسترجعة وسعر الإرجاع المعتمد.
/// </summary>
// كلاس كيان بند سند مرتجع المبيعات
public class SalesReturnItem : TenantBaseEntity
{
    /// <summary>
    /// معرّف سند مرتجع المبيعات الأساسي الذي يتبع له هذا البند.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف سند مرتجع المبيعات
    public Guid SalesReturnId { get; set; }

    /// <summary>
    /// كائن سند مرتجع المبيعات المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان سند مرتجع المبيعات
    public SalesReturn SalesReturn { get; set; } = null!;

    /// <summary>
    /// معرّف الصنف والمنتج المرتجع من العميل.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج المرتجع المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product Product { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود أو العبوة المحددة المرتجعة (اختياري).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية الفعلية المرتجعة من العميل إلى المستودع.
    /// </summary>
    // خاصية الكمية المرتجعة
    public int Quantity { get; set; }

    /// <summary>
    /// سعر بيع الوحدة المعتمد للإرجاع لاحتساب القيمة المستردة.
    /// </summary>
    // خاصية سعر الوحدة المرتجعة
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// إجمالي قيمة البند المرتجع الصافية (الكمية × سعر الوحدة).
    /// </summary>
    // خاصية إجمالي مبلغ السطر المرتجع
    public decimal LineTotal { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات خاصة بهذا البند المرتجع.
    /// </summary>
    // خاصية ملاحظات البند المرتجع
    public string? Notes { get; set; }
}
