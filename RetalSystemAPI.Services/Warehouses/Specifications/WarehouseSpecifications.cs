using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات المخازن وصالات العرض مع تضمين الفرع والفلترة بالنوع وترتيب النتائج بالاسم.
/// </summary>
public class WarehouseWithDetailsSpec : BaseSpecification<Warehouse>
{
    /// <summary>جلب كافة المستودعات والصالات مع الفرع مرتبة بالاسم</summary>
    public WarehouseWithDetailsSpec()
    {
        AddInclude(w => w.Branch);
        ApplyOrderBy(w => w.Name);
    }

    /// <summary>جلب مستودع محدد بالمعرف مع الفرع</summary>
    /// <param name="id">معرف المستودع</param>
    public WarehouseWithDetailsSpec(Guid id) : base(w => w.Id == id)
    {
        AddInclude(w => w.Branch);
    }

    /// <summary>فلترة المستودعات بالفرع والنوع (صالة / مخزن)</summary>
    /// <param name="branchId">معرف الفرع</param>
    /// <param name="type">نوع المستودع</param>
    public WarehouseWithDetailsSpec(Guid? branchId, WarehouseType? type)
        : base(w => (!branchId.HasValue || w.BranchId == branchId.Value) &&
                    (!type.HasValue || w.Type == type.Value))
    {
        AddInclude(w => w.Branch);
        ApplyOrderBy(w => w.Name);
    }
}
