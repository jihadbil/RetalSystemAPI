using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Warehouses;


namespace RetalSystemAPI.Models.Branchs;
/// <summary>
/// يمثل الكيان الخاص بالفروع في النظام، ويحتوي على الخصائص المتعلقة بالفرع مثل الاسم، العنوان، رقم الهاتف، وحالة النشاط.
/// </summary>
public class Branch : BaseEntity
{
    /// <summary>
    /// اسم الفرع
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// عنوان الفرع
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// حالة نشاط الفرع
    /// </summary>
    public bool IsActive { get; set; } = true;

    public ICollection<BranchPhone> BranchPhones { get; set; } = new List<BranchPhone>();

    public ICollection<ApplicationUser> ApplicationUser { get; set; }=new List<ApplicationUser>();
    public ICollection<Warehouse> Warehouses { get; set; } = new List<Warehouse>(); 

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    public ICollection<ShowroomStock> ShowroomStocks { get; set; } = new List<ShowroomStock>();

}
