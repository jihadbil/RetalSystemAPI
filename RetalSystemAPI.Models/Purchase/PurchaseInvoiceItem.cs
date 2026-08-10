using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Entities.Catalog;

namespace RetalSystemAPI.Models.Purchase;

public class PurchaseInvoiceItem : BaseEntity
{
    public int PurchaseInvoiceId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal LineTotal { get; set; }

    public PurchaseInvoice Invoice { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
