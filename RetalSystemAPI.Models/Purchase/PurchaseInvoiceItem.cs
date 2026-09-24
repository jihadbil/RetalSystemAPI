using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل بنداً تفصيلياً واحداً من بنود فاتورة المشتريات متضمناً الكمية، سعر شراء الوحدة، الخصم، والإجمالي.
/// </summary>
// كلاس كيان بند فاتورة المشتريات
public class PurchaseInvoiceItem : TenantBaseEntity
{
    /// <summary>
    /// معرّف فاتورة المشتريات الأساسية التي يتبع لها هذا البند.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف فاتورة المشتريات
    public Guid PurchaseInvoiceId { get; set; }

    /// <summary>
    /// كائن فاتورة المشتريات المرتبطة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان فاتورة المشتريات
    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

    /// <summary>
    /// معرّف الصنف والمنتج المشترى.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product Product { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود المحدد للعبوة المشتراة في حال ربطها بباركود معين.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية المشتراة من هذا البند.
    /// </summary>
    // خاصية الكمية المشتراة للبند
    public decimal Quantity { get; set; }

    /// <summary>
    /// سعر شراء الوحدة الواحدة من المورد قبل الخصم.
    /// </summary>
    // خاصية سعر شراء الوحدة
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// قيمة الخصم المالي الممنوح على إجمالي هذا السطر.
    /// </summary>
    // خاصية مبلغ الخصم على البند
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// إجمالي قيمة البند الصافية بعد خصم السطر (الكمية × السعر - الخصم).
    /// </summary>
    // خاصية إجمالي السطر الصافي
    public decimal LineTotal { get; set; }

    /// <summary>
    /// تفاصيل تحليل وتفكيك العبوات (Breakdowns) في حال تم شراء كراتين وتوزيعها إلى قطع (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لتفاصيل تفكيك عبوات البند
    public ICollection<PurchaseInvoiceItemBreakdown> Breakdowns { get; set; } = new List<PurchaseInvoiceItemBreakdown>();
}
