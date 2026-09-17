using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.Services.Branch.Specifications;

/// <summary>
/// مواصفة جلب الفروع مع أرقام هواتفها التابعة وترتيبها تصاعدياً بالاسم.
/// </summary>
public class BranchWithPhonesSpec : BaseSpecification<RetalSystemAPI.Models.Branchs.Branch>
{
    /// <summary>
    /// جلب جميع الفروع التابعة للمستأجر مع تضمين أرقام هواتفها وترتيبها تصاعدياً حسب اسم الفرع.
    /// </summary>
    public BranchWithPhonesSpec()
    {
        // تضمين جدول الهواتف المرتبطة بالفرع
        AddInclude(b => b.BranchPhones);

        // تطبيق الترتيب التصاعدي بالاسم
        ApplyOrderBy(b => b.Name);
    }

    /// <summary>
    /// جلب فرع محدد بواسطة المعرف الفريد مع تضمين كافة هواتفه المسجلة.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع</param>
    public BranchWithPhonesSpec(Guid id) : base(b => b.Id == id)
    {
        // تضمين الهواتف التابعة للفرع المحدد
        AddInclude(b => b.BranchPhones);
    }
}

/// <summary>
/// مواصفة جلب فرع محدد مع هواتفه والمستخدمين المرتبطين به للتحقق قبل الحذف ومنع حذف الفروع النشطة.
/// </summary>
public class BranchWithPhonesAndUsersSpec : BaseSpecification<RetalSystemAPI.Models.Branchs.Branch>
{
    /// <summary>
    /// تهيئة مواصفة استعلام الفرع بالمعرف وتضمين الهواتف والمستخدمين التابعين له.
    /// </summary>
    /// <param name="id">المعرف الفريد للفرع</param>
    public BranchWithPhonesAndUsersSpec(Guid id) : base(b => b.Id == id)
    {
        // تضمين أرقام هواتف الفرع
        AddInclude(b => b.BranchPhones);

        // تضمين قائمة مستخدمي التطبيق المرتبطين بالفرع
        AddInclude(b => b.ApplicationUsers);
    }
}
