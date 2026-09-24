using System;

namespace RetalSystemAPI.Models.Common;

/// <summary>
/// ثوابت ودوال ضبط ومعايرة تقسيم الصفحات (Pagination) المشتركة بين طبقات الـ API والخدمات والمستودعات لمنع القراءة غير المحدودة للبيانات.
/// </summary>
// كلاس ثابت لإدارة حدود وضوابط تقسيم صفحات البيانات
public static class Pagination
{
    /// <summary>
    /// حجم الصفحة الافتراضي عند عدم تحديده في طلب العميل (20 عنصراً).
    /// </summary>
    // ثابت حجم الصفحة الافتراضي
    public const int DefaultPageSize = 20;

    /// <summary>
    /// الحد الأقصى المسموح به لحجم الصفحة الواحدة لمنع استنزاف موارد الخادم (100 عنصر).
    /// </summary>
    // ثابت أقصى حجم مسموح به للصفحة
    public const int MaxPageSize = 100;

    /// <summary>
    /// الحد الأقصى المسموح به لرقم الصفحة لمنع طلب صفحات خارج النطاق المنطقي (1,000,000).
    /// </summary>
    // ثابت أقصى رقم صفحة مسموح به
    public const int MaxPageNumber = 1_000_000;

    /// <summary>
    /// ضبط ومعايرة رقم الصفحة المدخل لضمان بقائه في النطاق الصالح بين 1 وأقصى رقم صفحة.
    /// </summary>
    /// <param name="pageNumber">رقم الصفحة المطلوب من العميل</param>
    /// <returns>رقم الصفحة بعد التحقق والضبط</returns>
    // دالة معايرة رقم الصفحة لضمان كونه ضمن الحدود المسموحة
    public static int NormalizePageNumber(int pageNumber) => Math.Clamp(pageNumber, 1, MaxPageNumber);

    /// <summary>
    /// ضبط ومعايرة حجم الصفحة لضمان تطبيق القيمة الافتراضية إذا كان الحجم صفرًا أو سالبًا، وعدم تجاوز الحد الأقصى.
    /// </summary>
    /// <param name="pageSize">حجم الصفحة المطلوب من العميل</param>
    /// <returns>حجم الصفحة المقنن والمعتمد</returns>
    // دالة معايرة وضبط حجم الصفحة بين 1 والحد الأقصى
    public static int NormalizePageSize(int pageSize) =>
        // في حال كان الحجم أقل من أو يساوي صفر يتم استخدام الافتراضي مع تقييد القيمة بالحد الأقصى
        Math.Clamp(pageSize <= 0 ? DefaultPageSize : pageSize, 1, MaxPageSize);

    /// <summary>
    /// معايرة متزامنة لرقم الصفحة وحجمها معاً وتعديل المتغيرات مباشرة عبر المرجع (ref).
    /// </summary>
    /// <param name="pageNumber">مرجع رقم الصفحة للتعديل المباشر</param>
    /// <param name="pageSize">مرجع حجم الصفحة للتعديل المباشر</param>
    // دالة شاملة لضبط رقم الصفحة وحجمها معاً عبر التمرير بالمرجع
    public static void Normalize(ref int pageNumber, ref int pageSize)
    {
        // ضبط رقم الصفحة وضمان صحته
        pageNumber = NormalizePageNumber(pageNumber);

        // ضبط حجم الصفحة وضمان عدم تجاوزه الحد الأقصى
        pageSize = NormalizePageSize(pageSize);
    }
}
