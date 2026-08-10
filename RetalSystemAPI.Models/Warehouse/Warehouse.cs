using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Branchs;


namespace RetalSystemAPI.Models.Warehouse;

public class Warehouse : BaseEntity
{
    public int BranchId { get; set; }
    public string Name { get; set; } = null!;
    public WarehouseType Type { get; set; }
    public bool IsActive { get; set; } = true;

    public Branch Branch { get; set; } = null!;
    public ICollection<ShowroomStock> ShowroomStocks { get; set; } = new List<ShowroomStock>();
    public ICollection<StorgeStock> WarehouseStocks { get; set; } = new List<StorgeStock>();


}
