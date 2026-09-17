using System;
using System.Collections.Generic;

namespace RetalSystemAPI.Services.Common.Models;

/// <summary>
/// حاوية موحدة لنتائج الصفحات مع تفاصيل الترقيم والعدد الإجمالي والصفحات التالية والسابقة.
/// </summary>
/// <typeparam name="T">نوع عناصر القائمة</typeparam>
public class PagedResult<T>
{
    /// <summary>عناصر الصفحة الحالية</summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>العدد الإجمالي لجميع السجلات المطابقة</summary>
    public int TotalCount { get; init; }

    /// <summary>رقم الصفحة الحالية (1-based)</summary>
    public int PageNumber { get; init; }

    /// <summary>عدد العناصر في كل صفحة</summary>
    public int PageSize { get; init; }

    /// <summary>إجمالي عدد الصفحات المتاحة</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>هل توجد صفحة سابقة</summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>هل توجد صفحة تالية</summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// إنشاء كائن جديد يمثل صفحة نتائج مع حساب الترقيم تلقائياً.
    /// </summary>
    /// <param name="items">عناصر الصفحة المعادة</param>
    /// <param name="totalCount">العدد الإجمالي لكافة السجلات</param>
    /// <param name="pageNumber">رقم الصفحة الحالية</param>
    /// <param name="pageSize">عدد العناصر في الصفحة</param>
    /// <returns>كائن PagedResult مهيأ بالبيانات والترقيم</returns>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        // بناء كائن صفحة النتائج وتعيين خصائص الترقيم والعناصر
        return new PagedResult<T>
        {
            // إسناد قائمة العناصر
            Items = items,
            // إسناد العدد الإجمالي
            TotalCount = totalCount,
            // تعيين رقم الصفحة
            PageNumber = pageNumber,
            // تعيين حجم الصفحة
            PageSize = pageSize
        };
    }
}
