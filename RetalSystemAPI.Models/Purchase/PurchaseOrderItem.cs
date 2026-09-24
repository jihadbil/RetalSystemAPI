using System;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل بنداً واحداً تفصيلياً ضمن بنود أمر الشراء يحدد الباركود والكمية المطلوبة وسعر التكلفة التقديري.
/// </summary>
// كلاس كيان بند أمر الشراء
public class PurchaseOrderItem : TenantBaseEntity
{
    /// <summary>
    /// معرّف أمر الشراء الأساسي الذي يتبع له هذا البند.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف أمر الشراء
    public Guid PurchaseOrderId { get; set; }

    /// <summary>
    /// كائن أمر الشراء المرتبط بهذا البند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان أمر الشراء
    public PurchaseOrder PurchaseOrder { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود المحدد للعبوة أو النكهة أو المقاس المطلوب شراؤه.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid ProductBarCodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode ProductBarCode { get; set; } = null!;

    /// <summary>
    /// الكمية المطلوب شراؤها وتوريدها من هذا الصنف.
    /// </summary>
    // خاصية الكمية المطلوبة للبند
    public decimal Quantity { get; set; }

    /// <summary>
    /// سعر شراء الوحدة التقديري استناداً إلى آخر سعر شراء معتمد.
    /// </summary>
    // خاصية سعر شراء الوحدة التقديري
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// إجمالي القيمة التقديرية للبند (الكمية × سعر الوحدة).
    /// </summary>
    // خاصية إجمالي مبلغ السطر
    public decimal LineTotal { get; set; }
}
