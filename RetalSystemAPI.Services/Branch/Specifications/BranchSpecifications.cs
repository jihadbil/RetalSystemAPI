using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.Services.Branch.Specifications;

/// <summary>
/// مواصفة جلب الفروع مع أرقام هواتفها التابعة وترتيبها بالاسم.
/// </summary>
public class BranchWithPhonesSpec : BaseSpecification<RetalSystemAPI.Models.Branchs.Branch>
{
    /// <summary>
    /// جلب جميع الفروع مع هواتفها مرتبة بالاسم.
    /// </summary>
    public BranchWithPhonesSpec()
    {
        AddInclude(b => b.BranchPhones);
        ApplyOrderBy(b => b.Name);
    }

    /// <summary>
    /// جلب فرع محدد بالمعرف مع هواتفه.
    /// </summary>
    /// <param name="id">معرف الفرع</param>
    public BranchWithPhonesSpec(Guid id) : base(b => b.Id == id)
    {
        AddInclude(b => b.BranchPhones);
    }
}

/// <summary>
/// مواصفة جلب فرع محدد مع هواتفه والمستخدمين المرتبطين به للتحقق قبل الحذف.
/// </summary>
public class BranchWithPhonesAndUsersSpec : BaseSpecification<RetalSystemAPI.Models.Branchs.Branch>
{
    /// <summary>
    /// تهيئة مواصفة الفرع بالمعرف وتضمين الهواتف والمستخدمين.
    /// </summary>
    /// <param name="id">معرف الفرع</param>
    public BranchWithPhonesAndUsersSpec(Guid id) : base(b => b.Id == id)
    {
        AddInclude(b => b.BranchPhones);
        AddInclude(b => b.ApplicationUsers);
    }
}
