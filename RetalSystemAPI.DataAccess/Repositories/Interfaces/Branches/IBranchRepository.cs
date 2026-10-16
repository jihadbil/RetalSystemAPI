using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة مستودع بيانات الفروع (Branches) وإدارة هواتفها التابعة كوحدة واحدة.
/// </summary>
public partial interface IBranchRepository : IRepository<Branch>
{
    /// <summary>إضافة رقم هاتف جديد للفرع</summary>
    Task AddPhoneAsync(BranchPhone phone, CancellationToken ct = default);
    /// <summary>إضافة مجموعة أرقام هواتف للفرع</summary>
    Task AddPhonesAsync(IEnumerable<BranchPhone> phones, CancellationToken ct = default);
    /// <summary>حذف رقم هاتف فرع محلياً من السياق</summary>
    void RemovePhone(BranchPhone phone);
    /// <summary>حذف نهائي لرقم هاتف فرع من قاعدة البيانات</summary>
    void HardDeletePhone(BranchPhone phone);
    /// <summary>جلب أرقام هواتف الفرع المحددة</summary>
    Task<IReadOnlyList<BranchPhone>> GetPhonesAsync(Guid branchId, CancellationToken ct = default);
}
