namespace RetalSystemAPI.Services.Common.Models;

/// <summary>
/// نموذج نتائج مجزأة للصفحات (Pagination) مدعوم بملخص إحصائي أو مالي شامل لكافة الصفحات.
/// </summary>
/// <typeparam name="T">نوع البيانات الخاص بعناصر الصفحة</typeparam>
/// <typeparam name="TSummary">نوع البيانات الخاص بكائن الملخص الإجمالي</typeparam>
public class PagedResult<T, TSummary> : PagedResult<T>
{
    /// <summary>
    /// كائن الملخص الإحصائي أو المالي الشامل (مثل إجمالي المبيعات، إجمالي الخصومات، إلخ).
    /// </summary>
    public required TSummary Summary { get; init; }

    /// <summary>
    /// إنشاء كائن نتائج مجزأة يحتوي على العناصر ورقم الصفحة مع ضبط معايير الترقيم وحساب الملخص.
    /// </summary>
    /// <param name="items">قائمة عناصر الصفحة الحالية</param>
    /// <param name="totalCount">إجمالي عدد السجلات في قاعدة البيانات</param>
    /// <param name="pageNumber">رقم الصفحة المطلوبة</param>
    /// <param name="pageSize">حجم الصفحة</param>
    /// <param name="summary">كائن الملخص الشامل</param>
    /// <returns>كائن النتائج المجزأة الجاهز للاستجابة</returns>
    public static PagedResult<T, TSummary> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        TSummary summary)
    {
        // ضبط وتطبيع رقم وحجم الصفحة ضمن الحدود المسموح بها في النظام
        RetalSystemAPI.Models.Common.Pagination.Normalize(ref pageNumber, ref pageSize);

        // إنشاء كائن النتائج المجزأة وتعيين الخصائص المحسوبة والملخص
        return new()
        {
            // تعيين عناصر الصفحة الحالية
            Items = items,
            // تعيين إجمالي عدد السجلات
            TotalCount = totalCount,
            // تعيين رقم الصفحة المعدل
            PageNumber = pageNumber,
            // تعيين حجم الصفحة المعدل
            PageSize = pageSize,
            // تعيين كائن الملخص
            Summary = summary
        };
    }
}
