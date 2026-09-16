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
    /// <param name="data">البيانات الناتجة</param>
    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };

    /// <summary>
    /// إنشاء نتيجة فشل مع توضيح رسالة وكود الخطأ.
    /// </summary>
    /// <param name="message">رسالة الخطأ التوضيحية</param>
    /// <param name="code">كود الخطأ المعياري</param>
    public static ServiceResult<T> Failure(string message, string? code = null) =>
        new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
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
    /// إنشاء نتيجة نجاح للعملية.
    /// </summary>
    public static ServiceResult Success() => new() { IsSuccess = true };

    /// <summary>
    /// إنشاء نتيجة فشل للعملية مع رسالة وكود الخطأ.
    /// </summary>
    /// <param name="message">رسالة الخطأ</param>
    /// <param name="code">كود الخطأ</param>
    public static ServiceResult Failure(string message, string? code = null) =>
        new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
}
