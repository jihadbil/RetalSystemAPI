using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Sales;

/// <summary>
/// يمثل بنداً واحداً من بنود فاتورة المبيعات.
/// </summary>
public class SalesInvoiceItem : TenantBaseEntity
{
    public Guid SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;

    /// <summary>
    /// الصنف المباع.
    /// </summary>
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>
    /// الباركود الخاص (النكهة/اللون) — اختياري.
    /// </summary>
    public Guid? ProductBarCodeId { get; set; }
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية المباعة.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// سعر البيع للوحدة وقت إصدار الفاتورة.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// تكلفة الوحدة وقت البيع — لحساب الأرباح بدقة.
    /// </summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// قيمة الخصم على البند.
    /// </summary>
    public decimal DiscountAmount { get; set; } = 0;

    /// <summary>
    /// الإجمالي للبند = (Quantity × UnitPrice) - DiscountAmount.
    /// </summary>
    public decimal LineTotal { get; set; }
}
