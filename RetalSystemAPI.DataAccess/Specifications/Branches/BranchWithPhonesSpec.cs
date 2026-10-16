using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب الفروع مع أرقام هواتفها التابعة وترتيبها تصاعدياً بالاسم.
/// </summary>
internal sealed class BranchWithPhonesSpec : BaseSpecification<Branch>
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
    /// <param name = "id">المعرف الفريد للفرع</param>
    public BranchWithPhonesSpec(Guid id) : base(b => b.Id == id)
    {
        // تضمين الهواتف التابعة للفرع المحدد
        AddInclude(b => b.BranchPhones);
    }
}
