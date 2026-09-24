using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل بنداً واحداً تفصيلياً ضمن بنود سند مرتجع المشتريات موضحاً الصنف والباركود والكمية وسعر الشرجاع.
/// </summary>
// كلاس كيان بند سند مرتجع المشتريات
public class PurchaseReturnItem : TenantBaseEntity
{
    /// <summary>
    /// معرّف سند مرتجع المشتريات الأساسي الذي يتبع له هذا البند.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف سند المرتجع
    public Guid PurchaseReturnId { get; set; }

    /// <summary>
    /// كائن سند مرتجع المشتريات المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان سند المرتجع
    public PurchaseReturn PurchaseReturn { get; set; } = null!;

    /// <summary>
    /// معرّف الصنف والمنتج المرتجع إلى المورد.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج المرتبط بالبند المرتجع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product Product { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود أو العبوة والنكهة المحددة المرتجعة من المخزن.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية المرتجعة إلى المورد من هذا الصنف.
    /// </summary>
    // خاصية الكمية المرتجعة
    public decimal Quantity { get; set; }

    /// <summary>
    /// سعر شراء الوحدة المعتمد في المرتجع لاحتساب القيمة المستردة.
    /// </summary>
    // خاصية سعر شراء الوحدة المرتجعة
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// إجمالي قيمة البند المرتجع الصافية (الكمية × سعر الوحدة).
    /// </summary>
    // خاصية إجمالي مبلغ السطر المرتجع
    public decimal LineTotal { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات خاصة بهذا البند المرتجع (مثل وصف سبب العيب).
    /// </summary>
    // خاصية ملاحظات البند المرتجع
    public string? Notes { get; set; }
}
