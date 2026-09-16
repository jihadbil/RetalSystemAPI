using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل تفصيل الباركود/النكهة المستلمة للبند في فاتورة المشتريات لتغذية أرصدة المستودع بدقة.
/// </summary>
public class PurchaseInvoiceItemBreakdown : TenantBaseEntity
{
    public Guid PurchaseInvoiceItemId { get; set; }
    public PurchaseInvoiceItem PurchaseInvoiceItem { get; set; } = null!;

    public Guid ProductBarCodeId { get; set; }
    public ProductBarCode ProductBarCode { get; set; } = null!;

    public decimal PackageQuantity { get; set; } = 1;
    public int UnitsPerPackage { get; set; } = 1;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}
