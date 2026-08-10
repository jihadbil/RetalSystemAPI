using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Entities.Catalog;

namespace RetalSystemAPI.Models.Entities.Orders;

public class SalesOrderItem : BaseEntity
{
    public int SalesOrderId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    public SalesOrder SalesOrder { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
