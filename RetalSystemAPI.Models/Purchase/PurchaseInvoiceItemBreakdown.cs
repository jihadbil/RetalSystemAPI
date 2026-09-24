using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل تفصيل الباركود والعبوة والنكهة المستلمة للبند في فاتورة المشتريات لتغذية أرصدة المستودع بدقة واحتساب تكلفة القطعة.
/// </summary>
// كلاس كيان تفصيل عبوات بند فاتورة المشتريات
public class PurchaseInvoiceItemBreakdown : TenantBaseEntity
{
    /// <summary>
    /// معرّف بند فاتورة المشتريات الأصلي الذي يتبع له هذا التفصيل.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف بند فاتورة المشتريات
    public Guid PurchaseInvoiceItemId { get; set; }

    /// <summary>
    /// كائن بند فاتورة المشتريات المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان بند فاتورة المشتريات
    public PurchaseInvoiceItem PurchaseInvoiceItem { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود المحدد للعبوة أو النكهة المستلمة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid ProductBarCodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط بالتفصيل (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode ProductBarCode { get; set; } = null!;

    /// <summary>
    /// عدد العبوات أو الصناديق الكلية المشتراة.
    /// </summary>
    // خاصية كمية العبوات الكلية
    public decimal PackageQuantity { get; set; } = 1;

    /// <summary>
    /// عدد الوحدات أو القطع في العبوة الواحدة (معامل التعبئة).
    /// </summary>
    // خاصية عدد القطع داخل كل عبوة
    public int UnitsPerPackage { get; set; } = 1;

    /// <summary>
    /// إجمالي عدد القطع الصافية المستلمة (كمية العبوات × سعة العبوة).
    /// </summary>
    // خاصية إجمالي عدد القطع الصافية
    public decimal Quantity { get; set; }

    /// <summary>
    /// سعر تكلفة القطعة الواحدة الصافية المحسوب.
    /// </summary>
    // خاصية سعر تكلفة القطعة الفردية
    public decimal UnitPrice { get; set; }
}
