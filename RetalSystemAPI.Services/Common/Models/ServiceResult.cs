namespace RetalSystemAPI.Services.Common.Models;

/// <summary>
/// غلاف موحد لنتائج جميع الخدمات التي تعيد بيانات.
/// </summary>
/// <typeparam name="T">نوع البيانات المعادة</typeparam>
public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };

    public static ServiceResult<T> Failure(string message, string? code = null) =>
        new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
}

/// <summary>
/// غلاف موحد لنتائج الخدمات التي لا تعيد بيانات (مثل عمليات الحذف أو التعديل البسيط).
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }

    public static ServiceResult Success() => new() { IsSuccess = true };

    public static ServiceResult Failure(string message, string? code = null) =>
        new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
}
