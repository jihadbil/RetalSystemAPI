namespace RetalSystemAPI.Services.Common.Models;

/// <summary>
/// غلاف موحد لنتائج جميع الخدمات التي تعيد بيانات، مع تتبع حالة النجاح وكود ورسالة الخطأ إن وجدت.
/// </summary>
/// <typeparam name="T">نوع البيانات المعادة</typeparam>
public class ServiceResult<T>
{
    /// <summary>هل تمت العملية بنجاح</summary>
    public bool IsSuccess { get; private set; }

    /// <summary>البيانات المعادة في حال نجاح العملية</summary>
    public T? Data { get; private set; }

    /// <summary>نص رسالة الخطأ في حال الفشل</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>كود الخطأ المعياري للتعامل البرمجي معه</summary>
    public string? ErrorCode { get; private set; }

    /// <summary>
    /// إنشاء نتيجة نجاح مع البيانات المرفقة.
    /// </summary>
    /// <param name="data">البيانات الناتجة عن تنفيذ العملية</param>
    /// <returns>كائن نتيجة نجاح يحتوي على البيانات</returns>
    public static ServiceResult<T> Success(T data)
    {
        // إنشاء كائن جديد بنجاح وتعيين البيانات
        return new ServiceResult<T>
        {
            // وسم العملية كناجحة
            IsSuccess = true,
            // إسناد البيانات المرجعة
            Data = data
        };
    }

    /// <summary>
    /// إنشاء نتيجة فشل مع توضيح رسالة وكود الخطأ.
    /// </summary>
    /// <param name="message">رسالة الخطأ التوضيحية</param>
    /// <param name="code">كود الخطأ المعياري</param>
    /// <returns>كائن نتيجة فشل يحتوي على رسالة وكود الخطأ</returns>
    public static ServiceResult<T> Failure(string message, string? code = null)
    {
        // إنشاء كائن جديد بحالة فشل
        return new ServiceResult<T>
        {
            // وسم العملية كفاشلة
            IsSuccess = false,
            // حفظ رسالة الخطأ
            ErrorMessage = message,
            // حفظ كود الخطأ المعياري
            ErrorCode = code
        };
    }
}

/// <summary>
/// غلاف موحد لنتائج الخدمات التي لا تعيد بيانات (مثل عمليات الحذف أو التعديل الإجرائي).
/// </summary>
public class ServiceResult
{
    /// <summary>هل تمت العملية بنجاح</summary>
    public bool IsSuccess { get; private set; }

    /// <summary>نص رسالة الخطأ في حال الفشل</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>كود الخطأ المعياري</summary>
    public string? ErrorCode { get; private set; }

    /// <summary>
    /// إنشاء نتيجة نجاح للعملية الإجرائية.
    /// </summary>
    /// <returns>كائن نتيجة نجاح بدون بيانات</returns>
    public static ServiceResult Success()
    {
        // إنشاء كائن جديد بنجاح
        return new ServiceResult
        {
            // وسم العملية كناجحة
            IsSuccess = true
        };
    }

    /// <summary>
    /// إنشاء نتيجة فشل للعملية مع رسالة وكود الخطأ.
    /// </summary>
    /// <param name="message">رسالة الخطأ</param>
    /// <param name="code">كود الخطأ المعياري</param>
    /// <returns>كائن نتيجة فشل يحتوي على تفاصيل الخطأ</returns>
    public static ServiceResult Failure(string message, string? code = null)
    {
        // إنشاء كائن جديد بحالة فشل
        return new ServiceResult
        {
            // وسم العملية كفاشلة
            IsSuccess = false,
            // حفظ رسالة الخطأ
            ErrorMessage = message,
            // حفظ كود الخطأ
            ErrorCode = code
        };
    }
}
