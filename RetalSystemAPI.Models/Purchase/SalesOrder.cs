using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Entities.Customers;

namespace RetalSystemAPI.Models.Entities.Orders;

public class SalesOrder : BaseEntity
{
    public string OrderNumber { get; set; } = null!;
    public int? CustomerId { get; set; }
    public int? WarehouseId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDate { get; set; }
    public SalesOrderStatus Status { get; set; }
    public int? ConvertedSalesInvoiceId { get; set; }
    public decimal TotalAmount { get; set; }

    public Customer? Customer { get; set; }
    public Warehouse.Warehouse? Warehouse { get; set; }
    public SalesInvoice? ConvertedSalesInvoice { get; set; }
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
}
