using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Purchase;


namespace RetalSystemAPI.Models.Warehouses;
/// <summary>
///يمتل المخازن و صالات العرض الخاصة بالمحل
/// </summary>
public class Warehouse : BaseEntity
{
    /// <summary>
    /// معرف الفرع الدي ينتمي اليه المخزن او الصالة
    /// </summary>
    public int BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    /// <summary>
    /// اسم المخزن او الصالة
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// نوع اما امخزن او صالة عرض
    /// </summary>
    public WarehouseType Type { get; set; }

    /// <summary>
    /// فعال او لا 
    /// </summary>
    public bool IsActive { get; set; } = true;


    public ICollection<ShowroomStock> ShowroomStocks { get; set; } = new List<ShowroomStock>();
    public ICollection<StorgeStock> WarehouseStocks { get; set; } = new List<StorgeStock>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();

}
