using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل بنداً داخل فاتورة المشتريات.
/// </summary>
public class PurchaseInvoiceItem : TenantBaseEntity
{
    public Guid PurchaseInvoiceId { get; set; }
    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductBarCodeId { get; set; }
    public ProductBarCode? ProductBarCode { get; set; }

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal LineTotal { get; set; }

    public ICollection<PurchaseInvoiceItemBreakdown> Breakdowns { get; set; } = new List<PurchaseInvoiceItemBreakdown>();
}
