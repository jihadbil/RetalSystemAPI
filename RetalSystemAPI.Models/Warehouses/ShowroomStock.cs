using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouses;
/// <summary>
/// يمتل صالات العرض الخاصة بصاحب المحل تم فصلها عن المخزن لان التعامل مع الصنف في العرض يتم بمعرف الصنف و ليس بالكود لانه لا يهم اللون هنا لان سعر البيع و التكلفة هو نفسه
/// </summary>
public class ShowroomStock:BaseEntity
{
   /// <summary>
   /// معرف المخزن(الصالة) التي تحوي الصنف
   /// </summary>
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;
    /// <summary>
    /// معرف الصنف
    /// </summary>
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    /// <summary>
    /// الكمية
    /// </summary>
    public decimal Quantity { get; set; }
    /// <summary>
    /// اقل قيمة مسموح بها للصنف عندها يتم طلب تلقائي
    /// </summary>
    public int MinStockLevel { get; set; }
    

    
  
}
