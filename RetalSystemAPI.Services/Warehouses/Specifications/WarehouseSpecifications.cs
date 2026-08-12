using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات المخازن وصالات العرض.
/// </summary>
public class WarehouseWithDetailsSpec : BaseSpecification<Warehouse>
{
    public WarehouseWithDetailsSpec()
    {
        AddInclude(w => w.Branch);
        ApplyOrderBy(w => w.Name);
    }

    public WarehouseWithDetailsSpec(Guid id) : base(w => w.Id == id)
    {
        AddInclude(w => w.Branch);
    }

    public WarehouseWithDetailsSpec(Guid? branchId, WarehouseType? type)
        : base(w => (!branchId.HasValue || w.BranchId == branchId.Value) &&
                    (!type.HasValue || w.Type == type.Value))
    {
        AddInclude(w => w.Branch);
        ApplyOrderBy(w => w.Name);
    }
}
