using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses;

public class WarehouseSummaryDto : BaseDto
{
    public string Name { get; set; } = null!;
    public WarehouseType Type { get; set; }
    public string TypeName { get; set; } = null!;
    public bool IsActive { get; set; }
    public string BranchName { get; set; } = null!;
}
