using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Sales;

/// <summary>
/// يمثل بنداً واحداً تفصيلياً ضمن بنود فاتورة المبيعات يحدد الصنف، الباركود، الكمية، سعر البيع، والتكلفة لحساب هامش الربح.
/// </summary>
// كلاس كيان بند فاتورة المبيعات
public class SalesInvoiceItem : TenantBaseEntity
{
    /// <summary>
    /// معرّف فاتورة المبيعات الأساسية التي يتبع لها هذا البند.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف فاتورة المبيعات
    public Guid SalesInvoiceId { get; set; }

    /// <summary>
    /// كائن فاتورة المبيعات المرتبطة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان فاتورة المبيعات
    public SalesInvoice SalesInvoice { get; set; } = null!;

    /// <summary>
    /// معرّف الصنف والمنتج المباع.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج المباع المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product Product { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود المحدد للعبوة أو النكهة المباعة (اختياري).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية المباعة من هذا الصنف.
    /// </summary>
    // خاصية الكمية المباعة للبند
    public int Quantity { get; set; }

    /// <summary>
    /// سعر بيع الوحدة الواحدة وقت إصدار الفاتورة.
    /// </summary>
    // خاصية سعر بيع الوحدة
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// تكلفة الوحدة الواحدة وقت البيع لاحتساب مجمل الربح بدقة.
    /// </summary>
    // خاصية سعر تكلفة الوحدة وقت البيع
    public decimal UnitCost { get; set; }

    /// <summary>
    /// قيمة الخصم المالي الممنوح على إجمالي هذا السطر.
    /// </summary>
    // خاصية مبلغ الخصم على البند
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// إجمالي قيمة البند الصافية بعد الخصم = (Quantity × UnitPrice) - DiscountAmount.
    /// </summary>
    // خاصية إجمالي السطر الصافي
    public decimal LineTotal { get; set; }
}
