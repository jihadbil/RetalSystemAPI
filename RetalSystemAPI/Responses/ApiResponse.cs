namespace RetalSystemAPI.Responses;

/// <summary>
/// غلاف استجابة API الموحد لجميع طلبات الإرجاع التي تحتوي على بيانات.
/// </summary>
/// <typeparam name="T">نوع البيانات المعادة</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public string? ErrorCode { get; init; }

    public static ApiResponse<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string message, string? code = null)
        => new() { Success = false, Message = message, ErrorCode = code };
}

/// <summary>
/// غلاف استجابة API الموحد لجميع طلبات الإرجاع التي لا تحتوي على بيانات.
/// </summary>
public class ApiResponse
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? ErrorCode { get; init; }

    public static ApiResponse Ok(string? message = null)
        => new() { Success = true, Message = message };

    public static ApiResponse Fail(string message, string? code = null)
        => new() { Success = false, Message = message, ErrorCode = code };
}
