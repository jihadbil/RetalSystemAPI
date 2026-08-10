using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Suppliers;

namespace RetalSystemAPI.Models.Suppliers;
/// <summary>
/// يمتل الموردين الدين يتعامل معهم صاحب المحل
/// </summary>
public class Supplier : BaseEntity
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

    public ICollection<SupplierPhone> SupplierPhones { get; set; } = new List<SupplierPhone>();

    public ICollection<SupplierTransaction> SupplierTransactions { get; set; } = new List<SupplierTransaction>();


}
