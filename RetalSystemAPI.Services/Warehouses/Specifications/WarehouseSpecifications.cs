using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات المخازن وصالات العرض مع تضمين الفرع والفلترة بالنوع وترتيب النتائج تصاعدياً بالاسم.
/// </summary>
public class WarehouseWithDetailsSpec : BaseSpecification<Warehouse>
{
    /// <summary>
    /// جلب كافة المستودعات وصالات العرض التابعة للمستأجر مع الفرع وترتيبها تصاعدياً بالاسم.
    /// </summary>
    public WarehouseWithDetailsSpec()
    {
        // تضمين بيانات الفرع التابع له المستودع
        AddInclude(w => w.Branch);

        // ترتيب النتائج تصاعدياً باسم المستودع
        ApplyOrderBy(w => w.Name);
    }

    /// <summary>
    /// جلب مستودع أو صالة عرض محددة بواسطة المعرف الفريد مع تضمين بيانات الفرع.
    /// </summary>
    /// <param name="id">المعرف الفريد للمستودع</param>
    public WarehouseWithDetailsSpec(Guid id) : base(w => w.Id == id)
    {
        // تضمين بيانات الفرع للمستودع المحدد
        AddInclude(w => w.Branch);
    }

    /// <summary>
    /// فلترة المستودعات حسب الفرع والنوع (صالة عرض أو مخزن تخزين) مع الترتيب بالاسم.
    /// </summary>
    /// <param name="branchId">معرف الفرع للفلترة (اختياري)</param>
    /// <param name="type">نوع المستودع (صالة عرض / مخزن تخزين) (اختياري)</param>
    public WarehouseWithDetailsSpec(Guid? branchId, WarehouseType? type)
        : base(w => (!branchId.HasValue || w.BranchId == branchId.Value) &&
                    (!type.HasValue || w.Type == type.Value))
    {
        // تضمين بيانات الفرع
        AddInclude(w => w.Branch);

        // الترتيب تصاعدياً باسم المستودع
        ApplyOrderBy(w => w.Name);
    }
}
