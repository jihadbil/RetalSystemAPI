using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات وحدات القياس والتحقق من الأسماء والتجهيز للاستيراد.
/// </summary>
public partial interface IUnitRepository
{
    /// <summary>
    /// استرجاع قائمة مختصرة لمعرفات وأسماء الوحدات لملء القوائم المنسدلة (Lookup) كقراءة فقط.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة صفوف UnitLookupRow.</returns>
    Task<IReadOnlyList<UnitLookupRow>> GetLookupAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب الوحدات المرشحة للتحديث أثناء استيراد ملفات الإكسل بدفعة واحدة ككيانات متتبعة للتعديل.
    /// </summary>
    /// <param name="keys">مفاتيح الاستيراد (المعرفات والأسماء المطابقة).</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة الوحدات المتتبعة الجاهزة للتحديث.</returns>
    Task<IReadOnlyList<Unit>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default);

    /// <summary>
    /// جلب كافة كيانات الوحدات كقراءة فقط بدون تتبع مرتبة بالمعرف.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة كيانات الوحدات.</returns>
    Task<IReadOnlyList<Unit>> ListLookupAsync(CancellationToken ct = default);

    /// <summary>
    /// فحص تكرار اسم وحدة القياس للمستأجر الحالي قبل الإضافة.
    /// </summary>
    /// <param name="name">اسم الوحدة المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم مستخدماً، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// فحص تكرار اسم وحدة القياس مع استثناء سجل وحدة معينة عند التعديل.
    /// </summary>
    /// <param name="name">اسم الوحدة المحدث.</param>
    /// <param name="id">معرف الوحدة المستثنى.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد اسم مكرر لوحدة أخرى، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);
}
