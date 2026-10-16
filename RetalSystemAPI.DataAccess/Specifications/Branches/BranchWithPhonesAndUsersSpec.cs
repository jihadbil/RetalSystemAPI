using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Specifications;
/// <summary>
/// مواصفة جلب فرع محدد مع هواتفه والمستخدمين المرتبطين به للتحقق قبل الحذف ومنع حذف الفروع النشطة.
/// </summary>
internal sealed class BranchWithPhonesAndUsersSpec : BaseSpecification<Branch>
{
    /// <summary>
    /// تهيئة مواصفة استعلام الفرع بالمعرف وتضمين الهواتف والمستخدمين التابعين له.
    /// </summary>
    /// <param name = "id">المعرف الفريد للفرع</param>
    public BranchWithPhonesAndUsersSpec(Guid id) : base(b => b.Id == id)
    {
        // تضمين أرقام هواتف الفرع
        AddInclude(b => b.BranchPhones);
        // تضمين قائمة مستخدمي التطبيق المرتبطين بالفرع
        AddInclude(b => b.ApplicationUsers);
    }
}
