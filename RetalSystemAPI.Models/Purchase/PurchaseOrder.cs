using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;


namespace RetalSystemAPI.Models.Purchase;
/// <summary>
/// يمتل الطلبية الخاصة بصاحب المحل
/// </summary>
public class PurchaseOrder : BaseEntity
{
    /// <summary>
    /// رقم الطلبية
    /// </summary>
    public string OrderNumber { get; set; } = null!;
   /// <summary>
   /// يمتل المخزن او الصالة التي تحتاج هده الطلبية
   /// </summary>
    public Guid? WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
    /// <summary>
    /// معرف الفرع الدي تنتمي اليه الطلبية
    /// </summary>
    public Guid BranchId { get; set; }
    public Branch Branch { get; set; } = null!;
    /// <summary>
    /// تاريخ انشاء الطلبية
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// تااريخ استلام الطلبية المتوقع
    /// </summary>
    public DateTime? ExpectedDate { get; set; }
    /// <summary>
    /// حالة الطلبية
    /// </summary>
    public PurchaseOrderStatus Status { get; set; }

    /// <summary>
    /// اجمالي الطلبية
    /// </summary>
    public decimal TotalAmount { get; set; }

    

    
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
}
