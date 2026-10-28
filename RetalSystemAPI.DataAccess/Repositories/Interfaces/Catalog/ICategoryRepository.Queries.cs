using RetalSystemAPI.DataAccess.Queries;
using RetalSystemAPI.DataAccess.QueryResults;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// استعلامات التصنيفات الشجرية والبحث والتحقق من التكرار والتهيئة لعمليات الاستيراد والتصدير.
/// </summary>
public partial interface ICategoryRepository
{
    /// <summary>
    /// استرجاع تفاصيل التصنيف مع التصنيف الأب والتصنيفات الفرعية كقراءة فقط دون تتبع.
    /// </summary>
    /// <param name="id">معرف التصنيف.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>بيانات التصنيف أو null.</returns>
    Task<Category?> GetDetailsAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بكافة التصنيفات مع بياناتها التفصيلية كقراءة فقط.
    /// </summary>
    /// <param name="request">معايير الاستعلام.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة التصنيفات.</returns>
    Task<IReadOnlyList<Category>> ListAsync(CategoryQuery request, CancellationToken ct = default);

    /// <summary>
    /// استرجاع التصنيفات الجذرية الرئيسية التي ليس لها تصنيف أب (Root Categories).
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة التصنيفات الرئيسية.</returns>
    Task<IReadOnlyList<Category>> GetRootsAsync(CancellationToken ct = default);

    /// <summary>
    /// استرجاع التصنيفات الفرعية المباشرة التابعة لتصنيف أب محدد.
    /// </summary>
    /// <param name="parentId">معرف التصنيف الأب.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة التصنيفات الفرعية.</returns>
    Task<IReadOnlyList<Category>> GetChildrenAsync(Guid parentId, CancellationToken ct = default);

    /// <summary>
    /// البحث في التصنيفات وتصفحها بنظام الصفحات.
    /// </summary>
    /// <param name="request">معايير البحث والصفحات.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>صفحة نتائج التصنيفات.</returns>
    Task<PageResult<Category>> SearchAsync(CategoryQuery request, CancellationToken ct = default);

    /// <summary>
    /// فحص تكرار اسم التصنيف للمستأجر الحالي قبل الإضافة.
    /// </summary>
    /// <param name="name">اسم التصنيف المراد فحصه.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إذا كان الاسم مستخدماً، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, CancellationToken ct = default);

    /// <summary>
    /// فحص تكرار اسم التصنيف مع استثناء معرف تصنيف معين عند التعديل.
    /// </summary>
    /// <param name="name">اسم التصنيف المحدث.</param>
    /// <param name="id">معرف التصنيف المستثنى.</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>True إن وجد تصنيف آخر بنفس الاسم، وإلا False.</returns>
    Task<bool> NameExistsAsync(string name, Guid id, CancellationToken ct = default);

    /// <summary>
    /// استرجاع قائمة مختصرة ومحسنة لمعرفات وأسماء التصنيفات للقوائم المنسدلة (Lookup).
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة صفوف CategoryLookupRow.</returns>
    Task<IReadOnlyList<CategoryLookupRow>> GetLookupAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب أول تصنيف مسجل في النظام كعينة فحص سريعة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>تصنيف عينة أو null.</returns>
    Task<Category?> GetSampleAsync(CancellationToken ct = default);

    /// <summary>
    /// استرجاع أسماء كافة التصنيفات المسجلة للمستأجر كقراءة خفيفة للذاكرة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة أسماء التصنيفات.</returns>
    Task<IReadOnlyList<CategoryNamesRow>> GetNamesAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب التصنيفات المرشحة للتحديث أثناء استيراد ملفات الإكسل بدفعة واحدة ككيانات متتبعة للتعديل.
    /// </summary>
    /// <param name="keys">مفاتيح الاستيراد (المعرفات والأسماء المطابقة).</param>
    /// <param name="ct">رمز إلغاء العملية.</param>
    /// <returns>قائمة التصنيفات المتتبعة الجاهزة للتحديث.</returns>
    Task<IReadOnlyList<Category>> GetImportCandidatesForUpdateAsync(ImportKeys keys, CancellationToken ct = default);
}
