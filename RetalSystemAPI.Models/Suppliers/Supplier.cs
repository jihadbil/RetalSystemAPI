using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Suppliers;
/// <summary>
/// يمتل الموردين الدين يتعامل معهم صاحب المحل
/// </summary>
public class Supplier : TenantBaseEntity
{
    /// <summary>
    /// اسم المورد
    /// </summary>
    public string Name { get; set; } = null!;
    /// <summary>
    /// عنوان المورد
    /// </summary>
    public string? Address { get; set; }
    /// <summary>
    /// الرصيد المبدئي للمورد عند بداية التعامل معه
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// حالة نشاط المورد
    /// </summary>
    public bool IsActive { get; set; } = true;

    public ICollection<SupplierPhone> SupplierPhones { get; set; } = new List<SupplierPhone>();




}
