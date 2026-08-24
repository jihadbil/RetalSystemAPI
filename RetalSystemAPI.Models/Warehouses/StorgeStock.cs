using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouses;
/// <summary>
/// معرف المخزن اخاص بالمحل الدي يقوم صاحب المحل بحفظ مخزونه فيه تم فصله عن مخزون الصالات لان المخزن يتعمال مع 
/// كل صنف و نكهة بالباركود حتى و ان كان للصنف نفس السعر او البيع هنا يتم التعامل مع كل لون و نكهة على حدى
/// </summary>
public class StorgeStock : TenantBaseEntity
{
    /// <summary>
    /// معرف المخزن (المخزن) الدي ينتمي الي المخزون
    /// </summary>
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    /// <summary>
    /// معرف الباركود الخاص  بالنكهة و اللون الدي ينتمي الي المخزون
    /// </summary>
    public Guid ProductBarcodeId { get; set; }
  public ProductBarCode ProductBarcode { get; set; } = null!;

    /// <summary>
    /// الكمية
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// المستوى الأدنى للمخزون، وهو الحد الأدنى الذي يجب أن يكون موجودًا.
    /// </summary>
    public int MinStockLevel { get; set; }
   

  
 
}
