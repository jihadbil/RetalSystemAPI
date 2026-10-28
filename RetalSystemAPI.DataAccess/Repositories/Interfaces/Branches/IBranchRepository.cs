using RetalSystemAPI.Models.Branchs;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة مستودع بيانات الفروع (Branches) وإدارة هواتفها التابعة كوحدة واحدة.
/// </summary>
public partial interface IBranchRepository : IRepository<Branch>
{
    /// <summary>
    /// إضافة رقم هاتف جديد للفرع.
    /// </summary>
    /// <param name="phone">كيان رقم الهاتف المراد إضافته.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddPhoneAsync(BranchPhone phone, CancellationToken ct = default);

    /// <summary>
    /// إضافة مجموعة أرقام هواتف للفرع دفعة واحدة.
    /// </summary>
    /// <param name="phones">قائمة أرقام الهواتف المراد إضافتها.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    Task AddPhonesAsync(IEnumerable<BranchPhone> phones, CancellationToken ct = default);

    /// <summary>
    /// حذف رقم هاتف فرع محلياً من السياق (فيزيائياً).
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه.</param>
    void RemovePhone(BranchPhone phone);

    /// <summary>
    /// حذف نهائي وفوري لرقم هاتف فرع من قاعدة البيانات.
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه نهائياً.</param>
    void HardDeletePhone(BranchPhone phone);

    /// <summary>
    /// تطبيق الحذف المنطقي على رقم هاتف الفرع بتعيين IsDeleted = true.
    /// </summary>
    /// <param name="phone">كيان الهاتف المراد حذفه منطقياً.</param>
    void SoftDeletePhone(BranchPhone phone);

    /// <summary>
    /// جلب أرقام هواتف الفرع المحددة كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="branchId">معرف الفرع.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أرقام هواتف الفرع.</returns>
    Task<IReadOnlyList<BranchPhone>> GetPhonesAsync(Guid branchId, CancellationToken ct = default);
}
