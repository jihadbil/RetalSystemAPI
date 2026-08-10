using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouse;

public class ShowroomStock:BaseEntity
{
   
    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public decimal Quantity { get; set; }
    public int MinStockLevel { get; set; }
    

    
  
}
