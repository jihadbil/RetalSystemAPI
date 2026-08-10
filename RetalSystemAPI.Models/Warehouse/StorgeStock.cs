using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouse;

public class StorgeStock:BaseEntity
{

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    public int ProductBarcodeId { get; set; }
  public ProductBarCode ProductBarcode { get; set; } = null!;
    public int ProductId { get; set; }
   public Product Product { get; set; } = null!;
    public decimal Quantity { get; set; }
    public int MinStockLevel { get; set; }
   

  
 
}
